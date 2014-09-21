using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.Compiler.Expressions;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class StatementCompiler : CSharpSyntaxWalker, IRuntimeContext {
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private readonly SemanticModel _semanticModel;
		private readonly IDictionary<ISymbol, VariableData> _variables;
		private readonly ExpressionCompiler _expressionCompiler;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly string _thisAlias;
		private readonly ISet<string> _usedVariableNames;
		private readonly NestedFunctionContext _nestedFunctionContext;
		private readonly SharedValue<int> _nextLabelIndex;
		private readonly ImmutableDictionary<IRangeVariableSymbol, JsExpression> _activeRangeVariableSubstitutions;
		private Location _location;

		private ILocalSymbol _currentVariableForRethrow;
		private IDictionary<object, string> _currentGotoCaseMap;

		private List<JsStatement> _result;

		public StatementCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, SemanticModel semanticModel, IDictionary<ISymbol, VariableData> variables, IRuntimeLibrary runtimeLibrary, string thisAlias, ISet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext, ImmutableDictionary<IRangeVariableSymbol, JsExpression> activeRangeVariableSubstitutions)
			: this(metadataImporter, namer, errorReporter, semanticModel, variables, runtimeLibrary, thisAlias, usedVariableNames, nestedFunctionContext, activeRangeVariableSubstitutions, null, null, null, null)
		{
		}

		internal StatementCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, SemanticModel semanticModel, IDictionary<ISymbol, VariableData> variables, IRuntimeLibrary runtimeLibrary, string thisAlias, ISet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext, ImmutableDictionary<IRangeVariableSymbol, JsExpression> activeRangeVariableSubstitutions, ExpressionCompiler expressionCompiler, SharedValue<int> nextLabelIndex, ILocalSymbol currentVariableForRethrow, IDictionary<object, string> currentGotoCaseMap) : base(SyntaxWalkerDepth.Node) {
			_metadataImporter                 = metadataImporter;
			_namer                            = namer;
			_errorReporter                    = errorReporter;
			_semanticModel                    = semanticModel;
			_variables                        = variables;
			_runtimeLibrary                   = runtimeLibrary;
			_thisAlias                        = thisAlias;
			_usedVariableNames                = usedVariableNames;
			_nestedFunctionContext            = nestedFunctionContext;
			_currentVariableForRethrow        = currentVariableForRethrow;
			_currentGotoCaseMap               = currentGotoCaseMap;
			_activeRangeVariableSubstitutions = activeRangeVariableSubstitutions;

			_nextLabelIndex                   = nextLabelIndex ?? new SharedValue<int>(1);

			_expressionCompiler               = expressionCompiler ?? new ExpressionCompiler(semanticModel.Compilation, semanticModel, metadataImporter, namer, runtimeLibrary, errorReporter, variables, () => CreateTemporaryVariable(_location), (c, r) => new StatementCompiler(_metadataImporter, _namer, _errorReporter, _semanticModel, _variables, _runtimeLibrary, thisAlias, _usedVariableNames, c, r), thisAlias, nestedFunctionContext, activeRangeVariableSubstitutions, new Dictionary<ITypeSymbol, JsIdentifierExpression>());
			_result                           = new List<JsStatement>();
		}

		private void SetLocation(Location location) {
			_location = _errorReporter.Location = location;
		}

		internal static bool DisableStateMachineRewriteTestingUseOnly;

		private JsBlockStatement MakeIteratorBody(IteratorStateMachine sm, bool returnsIEnumerable, ITypeSymbol yieldType, string yieldResultVariable, IList<string> methodParameterNames) {
			var body = new List<JsStatement>();
			body.Add(JsStatement.Var(new[] { JsStatement.Declaration(yieldResultVariable, null) }.Concat(sm.Variables)));
			body.AddRange(sm.FinallyHandlers.Select(h => JsStatement.Var(h.Item1, h.Item2)));
			body.Add(JsStatement.Return(_runtimeLibrary.MakeEnumerator(yieldType,
			                                                           JsExpression.FunctionDefinition(new string[0], sm.MainBlock),
			                                                           JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Identifier(yieldResultVariable))),
			                                                           sm.Disposer != null ? JsExpression.FunctionDefinition(new string[0], sm.Disposer) : null,
			                                                           this)));
			if (returnsIEnumerable) {
				body = new List<JsStatement> {
				    JsStatement.Return(_runtimeLibrary.MakeEnumerable(
				        yieldType,
				        JsExpression.FunctionDefinition(new string[0],
				            JsStatement.Return(
				                JsExpression.Invocation(
				                    JsExpression.Member(
				                        JsExpression.FunctionDefinition(methodParameterNames, JsStatement.Block(body)),
				                        "call"),
				                    new JsExpression[] { JsExpression.This }.Concat(methodParameterNames.Select(JsExpression.Identifier))
				                )
				            )
				        ),
				        this
				    ))
				};
			}

			return JsStatement.Block(body);
		}

		internal JsFunctionDefinitionExpression StateMachineRewriteNormalMethod(JsFunctionDefinitionExpression function) {
			if (DisableStateMachineRewriteTestingUseOnly)
				return function;

			var usedLoopLabels = new HashSet<string>();
			var body = StateMachineRewriter.RewriteNormalMethod(function.Body,
			                                                    IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze,
			                                                    () => { var result = _namer.GetVariableName(null, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                    () => { var result = _namer.GetVariableName(_namer.StateVariableDesiredName, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                    () => { var result = _namer.GetStateMachineLoopLabel(usedLoopLabels); usedLoopLabels.Add(result); return result; });

			return ReferenceEquals(body, function.Body) ? function : JsExpression.FunctionDefinition(function.ParameterNames, body, function.Name);
		}

		private JsFunctionDefinitionExpression StateMachineRewriteIteratorBlock(JsFunctionDefinitionExpression function, bool returnsIEnumerable, ITypeSymbol yieldType) {
			if (DisableStateMachineRewriteTestingUseOnly)
				return function;

			var usedLoopLabels = new HashSet<string>();
			string yieldResultVariable = _namer.GetVariableName(_namer.YieldResultVariableDesiredName, _usedVariableNames);
			_usedVariableNames.Add(yieldResultVariable);
			var body = StateMachineRewriter.RewriteIteratorBlock(function.Body,
			                                                     IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze,
			                                                     () => { var result = _namer.GetVariableName(null, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                     () => { var result = _namer.GetVariableName(_namer.StateVariableDesiredName, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                     () => { var result = _namer.GetStateMachineLoopLabel(usedLoopLabels); usedLoopLabels.Add(result); return result; },
			                                                     () => { var result = _namer.GetVariableName(_namer.FinallyHandlerDesiredName, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                     x => JsExpression.Assign(JsExpression.Identifier(yieldResultVariable), x),
			                                                     sm => MakeIteratorBody(sm, returnsIEnumerable, yieldType, yieldResultVariable, function.ParameterNames));

			return JsExpression.FunctionDefinition(function.ParameterNames, body, function.Name);
		}

		private JsFunctionDefinitionExpression StateMachineRewriteAsyncMethod(JsFunctionDefinitionExpression function, bool returnsTask, ITypeSymbol taskGenericArgument) {
			if (DisableStateMachineRewriteTestingUseOnly)
				return function;

			var usedLoopLabels = new HashSet<string>();
			string stateMachineVariable = _namer.GetVariableName(_namer.AsyncStateMachineVariableDesiredName, _usedVariableNames);
			_usedVariableNames.Add(stateMachineVariable);
			string doFinallyBlocksVariable = _namer.GetVariableName(_namer.AsyncDoFinallyBlocksVariableDesiredName, _usedVariableNames);
			_usedVariableNames.Add(doFinallyBlocksVariable);
			string taskCompletionSourceVariable;
			if (returnsTask) {
				taskCompletionSourceVariable = _namer.GetVariableName(_namer.AsyncTaskCompletionSourceVariableDesiredName, _usedVariableNames);
				_usedVariableNames.Add(taskCompletionSourceVariable);
			}
			else {
				taskCompletionSourceVariable = null;
			}

			var body = StateMachineRewriter.RewriteAsyncMethod(function.Body,
			                                                   IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze,
			                                                   () => { var result = _namer.GetVariableName(null, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                   () => { var result = _namer.GetVariableName(_namer.StateVariableDesiredName, _usedVariableNames); _usedVariableNames.Add(result); return result; },
			                                                   () => { var result = _namer.GetStateMachineLoopLabel(usedLoopLabels); usedLoopLabels.Add(result); return result; },
			                                                   stateMachineVariable,
			                                                   doFinallyBlocksVariable,
			                                                   taskCompletionSourceVariable != null ? JsStatement.Declaration(taskCompletionSourceVariable, _runtimeLibrary.CreateTaskCompletionSource(taskGenericArgument, this)) : null,
			                                                   taskCompletionSourceVariable != null ? expr => _runtimeLibrary.SetAsyncResult(JsExpression.Identifier(taskCompletionSourceVariable), expr, this) : (Func<JsExpression, JsExpression>)null,
			                                                   taskCompletionSourceVariable != null ? expr => _runtimeLibrary.SetAsyncException(JsExpression.Identifier(taskCompletionSourceVariable), expr, this) : (Func<JsExpression, JsExpression>)null,
			                                                   taskCompletionSourceVariable != null ? () => _runtimeLibrary.GetTaskFromTaskCompletionSource(JsExpression.Identifier(taskCompletionSourceVariable), this) : (Func<JsExpression>)null,
			                                                   (f, t) => _runtimeLibrary.Bind(f, t, this));

			return ReferenceEquals(body, function.Body) ? function : JsExpression.FunctionDefinition(function.ParameterNames, body, function.Name);
		}

		private bool IsMutableValueType(ITypeSymbol type) {
			return Utils.IsMutableValueType(type, _metadataImporter);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ExpressionSyntax csharpInput, ITypeSymbol type) {
			return Utils.MaybeCloneValueType(input, csharpInput, type, _metadataImporter, _runtimeLibrary, this);
		}

		public JsFunctionDefinitionExpression CompileMethod(IReadOnlyList<IParameterSymbol> parameters, IDictionary<ISymbol, VariableData> variables, BlockSyntax body, bool staticMethodWithThisAsFirstArgument, bool expandParams, StateMachineType stateMachineType, ITypeSymbol iteratorBlockYieldTypeOrAsyncTaskGenericArgument = null) {
			SetLocation(body.GetLocation());
			try {
				var prepareParameters = MethodCompiler.PrepareParameters(parameters, variables, expandParams: expandParams, staticMethodWithThisAsFirstArgument: staticMethodWithThisAsFirstArgument);
				Visit(body);
				JsBlockStatement jsbody;
				if (_result.Count == 1 && _result[0] is JsBlockStatement) {
					if (prepareParameters.Count == 0)
						jsbody = (JsBlockStatement)_result[0];
					else
						jsbody = JsStatement.Block(prepareParameters.Concat(((JsBlockStatement)_result[0]).Statements));
				}
				else {
					jsbody = JsStatement.Block(prepareParameters.Concat(_result));
				}

				var result = JsExpression.FunctionDefinition((staticMethodWithThisAsFirstArgument ? new[] { _namer.ThisAlias } : new string[0]).Concat(parameters.Where((p, i) => i != parameters.Count - 1 || !expandParams).Select(p => variables[p].Name)), jsbody);

				switch (stateMachineType) {
					case StateMachineType.NormalMethod:
						result = StateMachineRewriteNormalMethod(result);
						break;

					case StateMachineType.IteratorBlockReturningIEnumerable:
					case StateMachineType.IteratorBlockReturningIEnumerator:
						result = StateMachineRewriteIteratorBlock(result, stateMachineType == StateMachineType.IteratorBlockReturningIEnumerable, iteratorBlockYieldTypeOrAsyncTaskGenericArgument);
						break;

					case StateMachineType.AsyncVoid:
					case StateMachineType.AsyncTask:
						result = StateMachineRewriteAsyncMethod(result, stateMachineType == StateMachineType.AsyncTask, iteratorBlockYieldTypeOrAsyncTaskGenericArgument);
						break;

					default:
						throw new ArgumentException("stateMachineType");
				}

				return result;
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock); 
			}
		}

		public JsBlockStatement Compile(StatementSyntax statement) {
			SetLocation(statement.GetLocation());
			try {
				_result = new List<JsStatement>();
				Visit(statement);
				if (_result.Count == 1 && _result[0] is JsBlockStatement)
					return (JsBlockStatement)_result[0];
				else
					return JsStatement.Block(_result);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsStatement.EmptyBlock;
			}
		}

		public IList<JsStatement> CompileConstructorInitializer(ConstructorInitializerSyntax initializer, bool currentIsStaticMethod) {
			SetLocation(initializer.GetLocation());
			try {
				var symbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(initializer).Symbol;
				if (symbol == null) {
					_errorReporter.InternalError("No symbol found for constructor initializer");
					return new JsStatement[0];
				}
				else {
					return _expressionCompiler.CompileConstructorInitializer(symbol, _semanticModel.GetArgumentMap(initializer), currentIsStaticMethod);
				}
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public IList<JsStatement> CompileImplicitBaseConstructorCall(INamedTypeSymbol type, bool currentIsStaticMethod) {
			SetLocation(type.Locations[0]);
			try {
				var ctor = type.BaseType.InstanceConstructors.Single(c => c.Parameters.Length == 0);

				return _expressionCompiler.CompileConstructorInitializer(ctor, ArgumentMap.Empty, currentIsStaticMethod);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public IList<JsStatement> CompileFieldInitializer(Location location, JsExpression jsThis, string scriptName, ISymbol member, ExpressionSyntax value) {
			SetLocation(location);
			try {
				var result = _expressionCompiler.Compile(value, true);
				var expr = _runtimeLibrary.InitializeField(jsThis, scriptName, member, result.Expression, this);
				if (expr == null)
					return result.AdditionalStatements;
				else
					return result.AdditionalStatements.Concat(new JsStatement[] { expr }).ToList();
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public JsExpression CompileDelegateCombineCall(Location location, JsExpression a, JsExpression b) {
			SetLocation(location);
			try {
				return _expressionCompiler.CompileDelegateCombineCall(a, b);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsExpression.Number(0);
			}
		}

		public JsExpression CompileDelegateRemoveCall(Location location, JsExpression a, JsExpression b) {
			SetLocation(location);
			try {
				return _expressionCompiler.CompileDelegateRemoveCall(a, b);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsExpression.Number(0);
			}
		}

		public IList<JsStatement> CompileDefaultFieldInitializer(Location location, JsExpression jsThis, string scriptName, ISymbol member, ITypeSymbol fieldType) {
			SetLocation(location);
			try {
				var expr = _runtimeLibrary.InitializeField(jsThis, scriptName, member, fieldType.IsReferenceType ? JsExpression.Null : _runtimeLibrary.Default(fieldType, this), this);
				return expr != null ? new JsStatement[] { expr } : (IList<JsStatement>)ImmutableArray<JsStatement>.Empty;
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameterSymbol tp) {
			return Utils.ResolveTypeParameter(tp, _metadataImporter, _errorReporter, _namer);
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			return Utils.EnsureCanBeEvaluatedMultipleTimes(_result, expression, expressionsThatMustBeEvaluatedBefore, () => { var temp = CreateTemporaryVariable(Location.None); return _variables[temp].Name; });
		}

		private StatementCompiler CreateInnerCompiler() {
			return new StatementCompiler(_metadataImporter, _namer, _errorReporter, _semanticModel, _variables, _runtimeLibrary, _thisAlias, _usedVariableNames, _nestedFunctionContext, _activeRangeVariableSubstitutions, _expressionCompiler, _nextLabelIndex, _currentVariableForRethrow, _currentGotoCaseMap);
		}

		private ILocalSymbol CreateTemporaryVariable(Location location) {
			string name = _namer.GetVariableName(null, _usedVariableNames);
			ILocalSymbol variable = new SimpleVariable("temporary", location);
			_variables[variable] = new VariableData(name, null, false);
			_usedVariableNames.Add(name);
			return variable;
		}

		[Flags]
		private enum CompileExpressionFlags {
			None = 0,
			ReturnValueIsImportant = 1,
			IsAssignmentSource = 2,
			IgnoreConversion = 4
		}

		private ExpressionCompileResult CompileExpression(ExpressionSyntax expr, CompileExpressionFlags flags) {
			var oldLocation = _errorReporter.Location;
			try {
				_errorReporter.Location = expr.GetLocation();
				var type = _semanticModel.GetTypeInfo(expr);
				var result = _expressionCompiler.Compile(expr, (flags & CompileExpressionFlags.ReturnValueIsImportant) != 0, (flags & CompileExpressionFlags.IgnoreConversion) != 0);
				if (((flags & CompileExpressionFlags.IsAssignmentSource) != 0) && IsMutableValueType(type.ConvertedType)) {
					result.Expression = MaybeCloneValueType(result.Expression, expr, type.ConvertedType);
				}
				return result;
			}
			finally {
				_errorReporter.Location = oldLocation;
			}
		}

		private void VisitChildren(SyntaxNode node) {
			DefaultVisit(node);
		}

		public override void Visit(SyntaxNode node) {
			SetLocation(node.GetLocation());
			VisitLeadingTrivia(node);
			base.Visit(node);
			VisitTrailingTrivia(node);
		}

		public override void VisitTrivia(SyntaxTrivia trivia) {
			switch (trivia.CSharpKind()) {
				case SyntaxKind.SingleLineCommentTrivia: {
					_result.Add(JsStatement.Comment(trivia.ToString().Substring(2)));
					break;
				}

				case SyntaxKind.MultiLineCommentTrivia: {
					string content = trivia.ToString();
					content = content.Substring(2, content.Length - 4);
					string prefix = new Regex(@"^\s*").Match(content).Captures[0].Value;
					List<string> commentLines = content.Replace("\r", "").Split('\n').Select(item => item.Trim()).SkipWhile(l => l == "").ToList();
					while (commentLines.Count > 0 && commentLines[commentLines.Count - 1] == "")
						commentLines.RemoveAt(commentLines.Count - 1);

					if (commentLines.Count > 0)
						_result.Add(JsStatement.Comment(string.Join(Environment.NewLine, commentLines.Select(item => prefix + item))));	// Replace the space at the start of each line with the same as the space in the first line.
					break;
				}
			}
		}

		private void VisitLeadingTrivia(SyntaxNode node) {
			if (node.HasLeadingTrivia) {
				foreach (var t in node.GetLeadingTrivia())
					VisitTrivia(t);
			}
		}

		private void VisitTrailingTrivia(SyntaxNode node) {
			if (node.HasTrailingTrivia) {
				foreach (var t in node.GetTrailingTrivia())
					VisitTrivia(t);
			}
		}

		private void HandleLocalDeclarations(IEnumerable<VariableDeclaratorSyntax> variables) {
			var declarations = new List<JsVariableDeclaration>();
			foreach (var d in variables) {
				SetLocation(d.GetLocation());
				var variable = _semanticModel.GetDeclaredSymbol(d);
				var data = _variables[variable];
				JsExpression jsInitializer;
				if (d.Initializer != null) {
					SetLocation(d.Initializer.GetLocation());
					var exprCompileResult = CompileExpression(d.Initializer.Value, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
					if (exprCompileResult.AdditionalStatements.Count > 0) {
						if (declarations.Count > 0) {
							_result.Add(JsStatement.Var(declarations));
							declarations = new List<JsVariableDeclaration>();
						}
						foreach (var s in exprCompileResult.AdditionalStatements) {
							_result.Add(s);
						}
					}
					jsInitializer = (data.UseByRefSemantics ? JsExpression.ObjectLiteral(new[] { new JsObjectLiteralProperty("$", exprCompileResult.Expression) }) : exprCompileResult.Expression);
				}
				else {
					if (data.UseByRefSemantics)
						jsInitializer = JsExpression.ObjectLiteral();
					else
						jsInitializer = null;
				}
				declarations.Add(JsStatement.Declaration(data.Name, jsInitializer));
			}

			if (declarations.Count > 0)
				_result.Add(JsStatement.Var(declarations));
		}

		public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) {
			HandleLocalDeclarations(node.Declaration.Variables);
		}

		public override void VisitExpressionStatement(ExpressionStatementSyntax expressionStatement) {
			_result.AddRange(_expressionCompiler.Compile(expressionStatement.Expression, false).GetStatements());
		}

		public override void VisitForStatement(ForStatementSyntax forStatement) {
			// Initializer. In case we need more than one statement, put all other statements just before this loop.
			JsStatement initializer;
			if (forStatement.Declaration != null) {
				HandleLocalDeclarations(forStatement.Declaration.Variables);
				initializer = _result[_result.Count - 1];
				_result.RemoveAt(_result.Count - 1);
			}
			else {
				JsExpression initExpr = null;
				foreach (var init in forStatement.Initializers) {
					var compiledInit = CompileExpression(init, CompileExpressionFlags.None);
					if (compiledInit.AdditionalStatements.Count == 0) {
						initExpr = (initExpr != null ? JsExpression.Comma(initExpr, compiledInit.Expression) : compiledInit.Expression);
					}
					else {
						if (initExpr != null)
							_result.Add(initExpr);
						_result.AddRange(compiledInit.AdditionalStatements);
						initExpr = compiledInit.Expression;
					}
				}
				initializer = (initExpr != null ? (JsStatement)initExpr : JsStatement.Empty);
			}

			// Condition
			JsExpression condition;
			List<JsStatement> preBody = null;
			if (forStatement.Condition != null) {
				var compiledCondition = CompileExpression(forStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
				if (compiledCondition.AdditionalStatements.Count == 0) {
					condition = compiledCondition.Expression;
				}
				else {
					// The condition requires additional statements. Transform "for (int i = 0; i < (SomeProperty = 1); i++) { ... }" to "for (var i = 0;; i++) { this.set_SomeProperty(1); if (!(i < 1)) { break; } ... }
					preBody = new List<JsStatement>();
					preBody.AddRange(compiledCondition.AdditionalStatements);
					preBody.Add(JsStatement.If(JsExpression.LogicalNot(compiledCondition.Expression), JsStatement.Break(), null));
					condition = null;
				}
			}
			else {
				condition = null;
			}

			// Iterators
			JsExpression iterator = null;
			List<JsStatement> postBody = null;
			if (forStatement.Incrementors.Count > 0) {
				var compiledIterators = forStatement.Incrementors.Select(i => CompileExpression(i, CompileExpressionFlags.None)).ToList();
				if (compiledIterators.All(i => i.AdditionalStatements.Count == 0)) {
					// No additional statements are required, add them as a single comma-separated expression to the JS iterator.
					iterator = compiledIterators.Aggregate(iterator, (current, i) => (current != null ? JsExpression.Comma(current, i.Expression) : i.Expression));
				}
				else {
					// At least one of the compiled iterators need additional statements. We could add the last expressions that don't need extra statements to the iterators section of the for loop, but for simplicity we'll just add everything to the end of the loop.
					postBody = new List<JsStatement>();
					foreach (var i in compiledIterators) {
						postBody.AddRange(i.AdditionalStatements);
						postBody.Add(i.Expression);
					}
				}
			}

			// Body
			var body = CreateInnerCompiler().Compile(forStatement.Statement);

			if (preBody != null || postBody != null) {
				body = JsStatement.Block(((IEnumerable<JsStatement>)preBody ?? new JsStatement[0]).Concat(body.Statements).Concat((IEnumerable<JsStatement>)postBody ?? new JsStatement[0]));
			}

			_result.Add(JsStatement.For(initializer, condition, iterator, body));
		}

		public override void VisitBreakStatement(BreakStatementSyntax node) {
			_result.Add(JsStatement.Break());
		}

		public override void VisitContinueStatement(ContinueStatementSyntax node) {
			_result.Add(JsStatement.Continue());
		}

		public override void VisitEmptyStatement(EmptyStatementSyntax node) {
			_result.Add(JsStatement.Empty);
		}

		public override void VisitIfStatement(IfStatementSyntax ifStatement) {
			var compiledCond = CompileExpression(ifStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
			_result.AddRange(compiledCond.AdditionalStatements);
			_result.Add(JsStatement.If(compiledCond.Expression, CreateInnerCompiler().Compile(ifStatement.Statement), ifStatement.Else != null ? CreateInnerCompiler().Compile(ifStatement.Else.Statement) : null));
		}

		public override void VisitBlock(BlockSyntax blockStatement) {
			var innerCompiler = CreateInnerCompiler();
			innerCompiler.VisitChildren(blockStatement);
			innerCompiler.VisitLeadingTrivia(blockStatement.CloseBraceToken);
			_result.Add(JsStatement.Block(innerCompiler._result));
		}

		public override void VisitDoStatement(DoStatementSyntax doWhileStatement) {
			var body = CreateInnerCompiler().Compile(doWhileStatement.Statement);
			var compiledCondition = CompileExpression(doWhileStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
			if (compiledCondition.AdditionalStatements.Count > 0)
				body = JsStatement.Block(body.Statements.Concat(compiledCondition.AdditionalStatements));
			_result.Add(JsStatement.DoWhile(compiledCondition.Expression, body));
		}

		public override void VisitWhileStatement(WhileStatementSyntax whileStatement) {
			// Condition
			JsExpression condition;
			List<JsStatement> preBody = null;
			var compiledCondition = CompileExpression(whileStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
			if (compiledCondition.AdditionalStatements.Count == 0) {
				condition = compiledCondition.Expression;
			}
			else {
				// The condition requires additional statements. Transform "while ((SomeProperty = 1) < 0) { ... }" to "while (true) { this.set_SomeProperty(1); if (!(i < 1)) { break; } ... }
				preBody = new List<JsStatement>();
				preBody.AddRange(compiledCondition.AdditionalStatements);
				preBody.Add(JsStatement.If(JsExpression.LogicalNot(compiledCondition.Expression), JsStatement.Break(), null));
				condition = JsExpression.True;
			}

			var body = CreateInnerCompiler().Compile(whileStatement.Statement);
			if (preBody != null)
				body = JsStatement.Block(preBody.Concat(body.Statements));

			_result.Add(JsStatement.While(condition, body));
		}

		public override void VisitReturnStatement(ReturnStatementSyntax returnStatement) {
			if (returnStatement.Expression != null) {
				var expr = CompileExpression(returnStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
				_result.AddRange(expr.AdditionalStatements);
				_result.Add(JsStatement.Return(expr.Expression));
			}
			else {
				_result.Add(JsStatement.Return());
			}
		}

		public override void VisitLockStatement(LockStatementSyntax lockStatement) {
			var expr = CompileExpression(lockStatement.Expression, CompileExpressionFlags.None);
			_result.AddRange(expr.AdditionalStatements);
			_result.Add(expr.Expression);
			Visit(lockStatement.Statement);
		}

		public override void VisitForEachStatement(ForEachStatementSyntax foreachStatement) {
			var info = _semanticModel.GetForEachStatementInfo(foreachStatement);

			var systemArray = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Array);
			var type = _semanticModel.GetTypeInfo(foreachStatement.Expression).Type;
			var iteratorVariable = _semanticModel.GetDeclaredSymbol(foreachStatement);

			if (type.SpecialType == SpecialType.System_Array || (type.BaseType != null && type.BaseType.SpecialType == SpecialType.System_Array) || (info.GetEnumeratorMethod != null && _metadataImporter.GetMethodSemantics(info.GetEnumeratorMethod).EnumerateAsArray)) {
				var arrayResult = CompileExpression(foreachStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IgnoreConversion);
				_result.AddRange(arrayResult.AdditionalStatements);
				var array = arrayResult.Expression;
				if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(array)) {
					var tmpArray = CreateTemporaryVariable(foreachStatement.GetLocation());
					_result.Add(JsStatement.Var(_variables[tmpArray].Name, array));
					array = JsExpression.Identifier(_variables[tmpArray].Name);
				}

				var length = systemArray.GetMembers("Length").OfType<IPropertySymbol>().SingleOrDefault();
				if (length == null) {
					_errorReporter.InternalError("Property Array.Length not found.");
					return;
				}

				var lengthAccess = _expressionCompiler.CompilePropertyRead(array, length);
				if (lengthAccess.AdditionalStatements.Count > 0) {
					_errorReporter.InternalError("Accessing property Array.Length may not return additional statements");
				}

				var index = CreateTemporaryVariable(foreachStatement.GetLocation());
				var jsIndex = JsExpression.Identifier(_variables[index].Name);
				JsExpression iteratorValue = MaybeCloneValueType(JsExpression.Index(array, jsIndex), null, info.ElementType);
				var body = new List<JsStatement>();
				if (!info.ElementConversion.IsIdentity) {
					var conversionResult = _expressionCompiler.CompileConversion(iteratorValue, info.CurrentProperty.Type, iteratorVariable.Type);
					body.AddRange(conversionResult.AdditionalStatements);
					iteratorValue = conversionResult.Expression;
				}
				if (_variables[iteratorVariable].UseByRefSemantics)
					iteratorValue = JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", iteratorValue));

				body.Add(JsStatement.Var(_variables[iteratorVariable].Name, iteratorValue));
				body.AddRange(CreateInnerCompiler().Compile(foreachStatement.Statement).Statements);

				_result.Add(JsStatement.For(JsStatement.Var(_variables[index].Name, JsExpression.Number(0)),
				                            JsExpression.Lesser(jsIndex, lengthAccess.Expression),
				                            JsExpression.PostfixPlusPlus(jsIndex),
				                            JsStatement.Block(body)));
			}
			else {
				var preBody = new List<JsStatement>();

				var getEnumeratorCall = _expressionCompiler.CompileMethodCall(foreachStatement.Expression, ImmutableArray<ExpressionSyntax>.Empty, info.GetEnumeratorMethod, true);
				_result.AddRange(getEnumeratorCall.AdditionalStatements);
				var enumerator = CreateTemporaryVariable(foreachStatement.GetLocation());
				_result.Add(JsStatement.Var(_variables[enumerator].Name, getEnumeratorCall.Expression));

				var moveNextInvocation = _expressionCompiler.CompileMethodCall(JsExpression.Identifier(_variables[enumerator].Name), ImmutableArray<ExpressionSyntax>.Empty, info.MoveNextMethod, true);
				preBody.AddRange(moveNextInvocation.AdditionalStatements);

				var getCurrent = _expressionCompiler.CompilePropertyRead(JsExpression.Identifier(_variables[enumerator].Name), info.CurrentProperty);
				preBody.AddRange(getCurrent.AdditionalStatements);
				JsExpression getCurrentValue = MaybeCloneValueType(getCurrent.Expression, null, info.ElementType);
				if (!info.ElementConversion.IsIdentity) {
					var conversionResult = _expressionCompiler.CompileConversion(getCurrentValue, info.CurrentProperty.Type, iteratorVariable.Type);
					preBody.AddRange(conversionResult.AdditionalStatements);
					getCurrentValue = conversionResult.Expression;
				}

				if (_variables[iteratorVariable].UseByRefSemantics) {
					getCurrentValue = JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", getCurrentValue));
				}

				preBody.Add(JsStatement.Var(_variables[iteratorVariable].Name, getCurrentValue));
				var body = CreateInnerCompiler().Compile(foreachStatement.Statement);

				body = JsStatement.Block(preBody.Concat(body.Statements));

				JsStatement disposer;

				var enumeratorType = info.GetEnumeratorMethod.ReturnType;
				if (enumeratorType.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_IDisposable)) {
					var disposeMethod = (IMethodSymbol)enumeratorType.FindImplementationForInterfaceMember(info.DisposeMethod);
					JsExpression target = JsExpression.Identifier(_variables[enumerator].Name);
					var disposeBody = new List<JsStatement>();
					if (disposeMethod == null) {
						disposeMethod = info.DisposeMethod;
						var conversion = _expressionCompiler.CompileConversion(target, enumeratorType, info.DisposeMethod.ContainingType);
						disposeBody.AddRange(conversion.AdditionalStatements);
						target = conversion.Expression;
					}
					// If the enumerator is implicitly convertible to IDisposable, we should dispose it.
					var compileResult = _expressionCompiler.CompileMethodCall(target, ImmutableArray<ExpressionSyntax>.Empty, disposeMethod, false);
					disposeBody.AddRange(compileResult.GetStatements());
					disposer = JsStatement.Block(disposeBody);
				}
				else if (enumeratorType.IsSealed) {
					// If the enumerator is sealed and not implicitly convertible to IDisposable, we need not dispose it.
					disposer = null;
				}
				else {
					// We don't know whether the enumerator is convertible to IDisposable, so we need to conditionally dispose it.
					var idisposable = _semanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable);
					var test = _runtimeLibrary.TypeIs(JsExpression.Identifier(_variables[enumerator].Name), enumeratorType, idisposable, this);
					var conversion = _expressionCompiler.CompileConversion(JsExpression.Identifier(_variables[enumerator].Name), enumeratorType, idisposable);
					var disposeBody = new List<JsStatement>();
					disposeBody.AddRange(conversion.AdditionalStatements);
					var disposeCall = _expressionCompiler.CompileMethodCall(conversion.Expression, ImmutableArray<ExpressionSyntax>.Empty, info.DisposeMethod, false);
					disposeBody.AddRange(disposeCall.GetStatements());

					disposer = JsStatement.If(test, JsStatement.Block(disposeBody), null);
				}
				JsStatement stmt = JsStatement.While(moveNextInvocation.Expression, body);
				if (disposer != null)
					stmt = JsStatement.Try(stmt, null, disposer);
				_result.Add(stmt);
			}
		}

		private JsBlockStatement GenerateUsingBlock(string resourceName, ITypeSymbol resourceType, ExpressionSyntax aquisitionExpression, Location tempVariableRegion, JsBlockStatement body) {
			SetLocation(aquisitionExpression.GetLocation());

			var systemIDisposable = _semanticModel.Compilation.GetSpecialType(SpecialType.System_IDisposable);
			var idisposableDisposeMethod = (IMethodSymbol)systemIDisposable.GetMembers("Dispose").Single();

			var compiledAquisition = _expressionCompiler.Compile(aquisitionExpression, true);

			var stmts = new List<JsStatement>();
			stmts.AddRange(compiledAquisition.AdditionalStatements);
			stmts.Add(JsStatement.Var(resourceName, MaybeCloneValueType(compiledAquisition.Expression, aquisitionExpression, resourceType)));

			if (resourceType.TypeKind == TypeKind.DynamicType) {
				var newResource = CreateTemporaryVariable(tempVariableRegion);
				var castExpr = _expressionCompiler.CompileConversion(JsExpression.Identifier(resourceName), resourceType, systemIDisposable);
				resourceName = _variables[newResource].Name;
				stmts.AddRange(castExpr.AdditionalStatements);
				stmts.Add(JsStatement.Var(resourceName, castExpr.Expression));
			}

			var disposeTarget = new ExpressionCompileResult(JsExpression.Identifier(resourceName), ImmutableArray<JsStatement>.Empty);
			var disposeMethod = (IMethodSymbol)resourceType.UnpackNullable().FindImplementationForInterfaceMember(idisposableDisposeMethod);
			if (disposeMethod == null) {
				disposeMethod = idisposableDisposeMethod;
				if (resourceType.TypeKind != TypeKind.DynamicType) {
					var conversion = _expressionCompiler.CompileConversion(disposeTarget.Expression, resourceType, systemIDisposable);
					if (conversion.AdditionalStatements.Count > 0)
						_errorReporter.InternalError("Upcast to IDisposable cannot return additional statements");
					disposeTarget = conversion;
				}
			}
			else {
				disposeTarget = new ExpressionCompileResult(MaybeCloneValueType(disposeTarget.Expression, null, resourceType), disposeTarget.AdditionalStatements);
			}

			var compiledDisposeCall = _expressionCompiler.CompileMethodCall(disposeTarget.Expression, ImmutableArray<ExpressionSyntax>.Empty, disposeMethod, false);

			JsStatement releaseStmt = JsStatement.Block(disposeTarget.AdditionalStatements.Concat(compiledDisposeCall.GetStatements()));
			if (resourceType.TypeKind != TypeKind.DynamicType) {
				// if (d != null) ((IDisposable)d).Dispose()
				releaseStmt = resourceType.IsValueType && !resourceType.IsNullable() ? releaseStmt : JsStatement.If(_runtimeLibrary.ReferenceNotEquals(JsExpression.Identifier(resourceName), JsExpression.Null, this), releaseStmt, null);
			}

			stmts.Add(JsStatement.Try(body, null, releaseStmt));

			return JsStatement.Block(stmts);
		}

		public override void VisitUsingStatement(UsingStatementSyntax usingStatement) {
			var stmt = CreateInnerCompiler().Compile(usingStatement.Statement);

			if (usingStatement.Declaration != null) {
				foreach (var resource in usingStatement.Declaration.Variables.Reverse()) {
					stmt = GenerateUsingBlock(_variables[_semanticModel.GetDeclaredSymbol(resource)].Name, _semanticModel.GetTypeInfo(resource.Initializer.Value).ConvertedType, resource.Initializer.Value, usingStatement.GetLocation(), stmt);
				}
			}
			else {
				var resource = CreateTemporaryVariable(usingStatement.GetLocation());
				stmt = GenerateUsingBlock(_variables[resource].Name, _semanticModel.GetTypeInfo(usingStatement.Expression).Type, usingStatement.Expression, usingStatement.GetLocation(), stmt);
			}

			_result.Add(stmt);
		}

		private JsBlockStatement CompileCatchClause(string catchVariableName, CatchClauseSyntax catchClause, bool isCatchAll, bool isOnly) {
			SetLocation(catchClause.GetLocation());
			JsStatement variableDeclaration = null;
			if (catchClause.Declaration != null && catchClause.Declaration.Identifier.CSharpKind() != SyntaxKind.None) {
				var caughtSymbol = _semanticModel.GetDeclaredSymbol(catchClause.Declaration);
				JsExpression compiledAssignment;
				if (isCatchAll) {
					// If this is the only handler we need to construct the exception
					compiledAssignment = isOnly ? _runtimeLibrary.MakeException(JsExpression.Identifier(catchVariableName), this) : JsExpression.Identifier(catchVariableName);
				}
				else {
					var conversion = _expressionCompiler.CompileConversion(JsExpression.Identifier(catchVariableName), _semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Exception).FullName), caughtSymbol.Type);
					if (conversion.AdditionalStatements.Count > 0)
						_errorReporter.InternalError("Downcast of expression may not return additional statements");
					compiledAssignment = conversion.Expression;
				}

				variableDeclaration = JsStatement.Var(_variables[caughtSymbol].Name, compiledAssignment);
			}

			var result = CreateInnerCompiler().Compile(catchClause.Block);
			if (variableDeclaration != null)
				result = JsStatement.Block(new[] { variableDeclaration }.Concat(result.Statements));
			return result;
		}

		private bool IsCatchAll(CatchClauseSyntax catchClause) {
			if (catchClause.Declaration == null)
				return true;
			return Equals(_semanticModel.GetSymbolInfo(catchClause.Declaration.Type).Symbol, _semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Exception).FullName));
		}

		private IReadOnlyList<CatchClauseSyntax> GetReachableCatches(IReadOnlyList<CatchClauseSyntax> clauses) {
			// All catches are reachable except for the catch-all clause if Exception is also caught
			if (clauses.Count >= 2 && clauses[clauses.Count - 1].Declaration == null) {
				var systemException = _semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Exception).FullName);
				var type = _semanticModel.GetSymbolInfo(clauses[clauses.Count - 2].Declaration.Type).Symbol;
				if (Equals(type, systemException)) {
					var result = new List<CatchClauseSyntax>(clauses);
					result.RemoveAt(result.Count - 1);
					return result;
				}
			}
			return clauses;
		}

		public override void VisitTryStatement(TryStatementSyntax tryStatement) {
			var tryBlock = CreateInnerCompiler().Compile(tryStatement.Block);
			JsCatchClause catchClause = null;
			if (tryStatement.Catches.Count > 0) {
				var oldVariableForRethrow = _currentVariableForRethrow;

				_currentVariableForRethrow = CreateTemporaryVariable(tryStatement.Catches.First().GetLocation());
				string catchVariableName = _variables[_currentVariableForRethrow].Name;

				var catches = GetReachableCatches(tryStatement.Catches);

				bool lastIsCatchall = IsCatchAll(catches[catches.Count - 1]);
				JsStatement current = lastIsCatchall
				                    ? CompileCatchClause(_variables[_currentVariableForRethrow].Name, catches[catches.Count - 1], true, catches.Count == 1)
				                    : JsStatement.Block(JsStatement.Throw(JsExpression.Identifier(catchVariableName)));

				for (int i = catches.Count - (lastIsCatchall ? 2 : 1); i >= 0; i--) {
					var catchType = (ITypeSymbol)_semanticModel.GetSymbolInfo(catches[i].Declaration.Type).Symbol;
					var test = _runtimeLibrary.TypeIs(JsExpression.Identifier(catchVariableName), _semanticModel.Compilation.GetTypeByMetadataName(typeof(Exception).FullName), catchType, this);
					current = JsStatement.If(test, CompileCatchClause(_variables[_currentVariableForRethrow].Name, catches[i], false, catches.Count == 1), current);
				}

				if (!lastIsCatchall || catches.Count > 1) {
					// We need to wrap the exception.
					current = JsStatement.Block(JsExpression.Assign(JsExpression.Identifier(catchVariableName), _runtimeLibrary.MakeException(JsExpression.Identifier(catchVariableName), this)), current);
				}

				catchClause = JsStatement.Catch(catchVariableName, current);
				_currentVariableForRethrow = oldVariableForRethrow;
			}

			var finallyBlock = tryStatement.Finally != null ? CreateInnerCompiler().Compile(tryStatement.Finally.Block) : null;

			_result.Add(JsStatement.Try(tryBlock, catchClause, finallyBlock));
		}

		public override void VisitThrowStatement(ThrowStatementSyntax throwStatement) {
			if (throwStatement.Expression == null) {
				_result.Add(JsStatement.Throw(JsExpression.Identifier(_variables[_currentVariableForRethrow].Name)));
			}
			else {
				var compiledExpr = CompileExpression(throwStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant);
				_result.AddRange(compiledExpr.AdditionalStatements);
				_result.Add(JsStatement.Throw(compiledExpr.Expression));
			}
		}

		public override void VisitYieldStatement(YieldStatementSyntax yieldStatement) {
			switch (yieldStatement.CSharpKind()) {
				case SyntaxKind.YieldReturnStatement:
					var compiledExpr = CompileExpression(yieldStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
					_result.AddRange(compiledExpr.AdditionalStatements);
					_result.Add(JsStatement.Yield(compiledExpr.Expression));
					break;

				case SyntaxKind.YieldBreakStatement:
					_result.Add(JsStatement.Yield(null));
					break;

				default:
					_errorReporter.InternalError("Unsupported statement " + yieldStatement);
					break;
			}
		}

		public override void VisitGotoStatement(GotoStatementSyntax gotoStatement) {
			switch (gotoStatement.CSharpKind()) {
				case SyntaxKind.GotoCaseStatement:
					var value = _semanticModel.GetConstantValue(gotoStatement.Expression).Value;
					_result.Add(JsStatement.Goto(_currentGotoCaseMap[NormalizeSwitchLabelValue(value)]));
					break;

				case SyntaxKind.GotoDefaultStatement:
					_result.Add(JsStatement.Goto(_currentGotoCaseMap[_gotoCaseMapDefaultKey]));
					break;

				case SyntaxKind.GotoStatement: {
					var label = (ILabelSymbol)_semanticModel.GetSymbolInfo(gotoStatement.Expression).Symbol;
					_result.Add(JsStatement.Goto(label.Name));
					break;
				}

				default:
					_errorReporter.InternalError("Unsupported statement " + gotoStatement);
					break;
			}

		}

		public override void VisitLabeledStatement(LabeledStatementSyntax labelStatement) {
			int index = _result.Count;
			Visit(labelStatement.Statement);
			_result[index] = JsStatement.Label(labelStatement.Identifier.Text, _result[index]);
		}

		public override void VisitFixedStatement(FixedStatementSyntax fixedStatement) {
			_errorReporter.InternalError("fixed statement is not supported");
		}

		public override void VisitUnsafeStatement(UnsafeStatementSyntax unsafeStatement) {
			_errorReporter.InternalError("unsafe statement is not supported");
		}

		private static readonly object _gotoCaseMapDefaultKey = new object();
		private static readonly object _gotoCaseMapCaseNullKey = new object();
		
		private static object NormalizeSwitchLabelValue(object value) {
			if (value == null)
				return _gotoCaseMapCaseNullKey;
			if (value is string)
				return value;
			else
				return Convert.ChangeType(value, typeof(long));
		}

		private class GatherGotoCaseAndDefaultDataVisitor : CSharpSyntaxWalker {
			private Dictionary<object, SwitchSectionSyntax> _sectionLookup;
			private Dictionary<SwitchSectionSyntax, string> _labels;
			private Dictionary<object, string> _gotoCaseMap;
			private readonly SemanticModel _semanticModel;
			private readonly SharedValue<int> _nextLabelIndex;

			public GatherGotoCaseAndDefaultDataVisitor(SemanticModel semanticModel, SharedValue<int> nextLabelIndex) {
				_semanticModel = semanticModel;
				_nextLabelIndex = nextLabelIndex;
			}

			public Tuple<Dictionary<SwitchSectionSyntax, string>, Dictionary<object, string>> Process(SwitchStatementSyntax switchStatement) {
				_labels      = new Dictionary<SwitchSectionSyntax, string>();
				_gotoCaseMap = new Dictionary<object, string>();
				_sectionLookup = (  from section in switchStatement.Sections
				                    from label in section.Labels
				                  select new { section, value = (label.CSharpKind() == SyntaxKind.CaseSwitchLabel ? NormalizeSwitchLabelValue(_semanticModel.GetConstantValue(label.Value).Value) : _gotoCaseMapDefaultKey) }
				                 ).ToDictionary(x => x.value, x => x.section);

				foreach (var section in switchStatement.Sections)
					Visit(section);

				return Tuple.Create(_labels, _gotoCaseMap);
			}

			public override void VisitGotoStatement(GotoStatementSyntax gotoStatement) {
				object labelValue;
				switch (gotoStatement.CSharpKind()) {
					case SyntaxKind.GotoCaseStatement:
						labelValue = NormalizeSwitchLabelValue(_semanticModel.GetConstantValue(gotoStatement.Expression).Value);
						break;

					case SyntaxKind.GotoDefaultStatement:
						labelValue = _gotoCaseMapDefaultKey;
						break;

					default:
						return;
				}

				var targetSection = _sectionLookup[labelValue];
				if (!_labels.ContainsKey(targetSection)) {
					_labels.Add(targetSection, string.Format(CultureInfo.InvariantCulture, "$label{0}", _nextLabelIndex.Value++));
				}
				if (!_gotoCaseMap.ContainsKey(labelValue))
					_gotoCaseMap[labelValue] = _labels[targetSection];
			}

			public override void VisitSwitchStatement(SwitchStatementSyntax switchStatement) {
				// Switch statements start a new context so we don't want to go there.
			}
		}

		private object GetSwitchLabelValue(SwitchLabelSyntax label) {
			var field = _semanticModel.GetSymbolInfo(label.Value).Symbol as IFieldSymbol;
			if (field != null) {
				var sem = _metadataImporter.GetFieldSemantics(field);
				if (sem.Type == FieldScriptSemantics.ImplType.Constant)
					return sem.Value;
			}

			return _semanticModel.GetConstantValue(label.Value).Value;
		}

		public override void VisitSwitchStatement(SwitchStatementSyntax switchStatement) {
			var compiledExpr = CompileExpression(switchStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant);
			_result.AddRange(compiledExpr.AdditionalStatements);

			var oldGotoCaseMap = _currentGotoCaseMap;

			var gotoCaseData = new GatherGotoCaseAndDefaultDataVisitor(_semanticModel, _nextLabelIndex).Process(switchStatement);
			_currentGotoCaseMap = gotoCaseData.Item2;

			var caseClauses = new List<JsSwitchSection>();
			foreach (var section in switchStatement.Sections) {
				SetLocation(section.GetLocation());
				var values = new List<JsExpression>();
				foreach (var label in section.Labels) {
					if (label.CSharpKind() == SyntaxKind.DefaultSwitchLabel) {
						values.Add(null);
					}
					else {
						object value = GetSwitchLabelValue(label);

						if (value == null) {
							values.Add(JsExpression.Null);
						}
						else if (value is string) {
							values.Add(JsExpression.String((string)value));
						}
						else {
							values.Add(JsExpression.Number((long)Convert.ChangeType(value, typeof(long))));
						}
					}
				}

				var ic = CreateInnerCompiler();
				IList<JsStatement> statements;
				if (section.Statements.Count == 1 && section.Statements.First() is BlockSyntax) {
					statements = ic.Compile(section.Statements.First()).Statements;
				}
				else {
					ic.VisitChildren(section);
					statements = ic._result;
				}

				if (gotoCaseData.Item1.ContainsKey(section))
					statements = new[] { JsStatement.Label(gotoCaseData.Item1[section], statements[0]) }.Concat(statements.Skip(1)).ToList();

				caseClauses.Add(JsStatement.SwitchSection(values, JsStatement.Block(statements)));
			}

			_result.Add(JsStatement.Switch(compiledExpr.Expression, caseClauses));
			_currentGotoCaseMap = oldGotoCaseMap;
		}
	}
}
