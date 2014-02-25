﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using ExpressionType = System.Linq.Expressions.ExpressionType;

namespace Saltarelle.Compiler.Compiler {
	public enum StateMachineType {
		NormalMethod,
		IteratorBlockReturningIEnumerable,
		IteratorBlockReturningIEnumerator,
		AsyncVoid,
		AsyncTask
	}

	public class StatementCompiler : DepthFirstAstVisitor, IRuntimeContext {
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private readonly ICompilation _compilation;
		private readonly CSharpAstResolver _resolver;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;
		private readonly ExpressionCompiler _expressionCompiler;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly string _thisAlias;
		private readonly ISet<string> _usedVariableNames;
		private readonly NestedFunctionContext _nestedFunctionContext;
		private readonly SharedValue<int> _nextLabelIndex;
		private readonly IMethod _methodBeingCompiled;
		private readonly ITypeDefinition _typeBeingCompiled;
		private DomRegion _region;

		private IVariable _currentVariableForRethrow;
		private IDictionary<object, string> _currentGotoCaseMap;

		private List<JsStatement> _result;

		public StatementCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, IRuntimeLibrary runtimeLibrary, string thisAlias, ISet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext, IMethod methodBeingCompiled, ITypeDefinition typeBeingCompiled)
			: this(metadataImporter, namer, errorReporter, compilation, resolver, variables, nestedFunctions, runtimeLibrary, thisAlias, usedVariableNames, nestedFunctionContext, methodBeingCompiled, typeBeingCompiled, null, null, null, null)
		{
		}

		internal StatementCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, IRuntimeLibrary runtimeLibrary, string thisAlias, ISet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext, IMethod methodBeingCompiled, ITypeDefinition typeBeingCompiled, ExpressionCompiler expressionCompiler, SharedValue<int> nextLabelIndex, IVariable currentVariableForRethrow, IDictionary<object, string> currentGotoCaseMap) {
			_metadataImporter           = metadataImporter;
			_namer                      = namer;
			_errorReporter              = errorReporter;
			_compilation                = compilation;
			_resolver                   = resolver;
			_variables                  = variables;
			_nestedFunctions            = nestedFunctions;
			_runtimeLibrary             = runtimeLibrary;
			_thisAlias                  = thisAlias;
			_usedVariableNames          = usedVariableNames;
			_nestedFunctionContext      = nestedFunctionContext;
			_methodBeingCompiled        = methodBeingCompiled;
			_typeBeingCompiled          = typeBeingCompiled;
			_currentVariableForRethrow  = currentVariableForRethrow;
			_currentGotoCaseMap         = currentGotoCaseMap;

			_nextLabelIndex             = nextLabelIndex ?? new SharedValue<int>(1);

			_expressionCompiler         = expressionCompiler ?? new ExpressionCompiler(compilation, metadataImporter, namer, runtimeLibrary, errorReporter, variables, nestedFunctions, v => CreateTemporaryVariable(v, _region), c => new StatementCompiler(_metadataImporter, _namer, _errorReporter, _compilation, _resolver, _variables, _nestedFunctions, _runtimeLibrary, thisAlias, _usedVariableNames, c, _methodBeingCompiled, _typeBeingCompiled), thisAlias, nestedFunctionContext, null, _methodBeingCompiled, _typeBeingCompiled);
			_result                     = new List<JsStatement>();
		}

		private void SetRegion(DomRegion region) {
			_region = region;
			_errorReporter.Region = region;
		}

		internal static bool DisableStateMachineRewriteTestingUseOnly;

		private JsBlockStatement MakeIteratorBody(IteratorStateMachine sm, bool returnsIEnumerable, IType yieldType, string yieldResultVariable, IList<string> methodParameterNames) {
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

		private JsFunctionDefinitionExpression StateMachineRewriteIteratorBlock(JsFunctionDefinitionExpression function, bool returnsIEnumerable, IType yieldType) {
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

		private JsFunctionDefinitionExpression StateMachineRewriteAsyncMethod(JsFunctionDefinitionExpression function, bool returnsTask, IType taskGenericArgument) {
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

		private bool IsMutableValueType(IType type) {
			return Utils.IsMutableValueType(type, _metadataImporter);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ResolveResult csharpInput, IType type) {
			return Utils.MaybeCloneValueType(input, csharpInput, type, _metadataImporter, _runtimeLibrary, this);
		}

		public JsFunctionDefinitionExpression CompileMethod(IList<IParameter> parameters, IDictionary<IVariable, VariableData> variables, BlockStatement body, bool staticMethodWithThisAsFirstArgument, bool expandParams, StateMachineType stateMachineType, IType iteratorBlockYieldTypeOrAsyncTaskGenericArgument = null) {
			SetRegion(body.GetRegion());
			try {
				_result = MethodCompiler.PrepareParameters(parameters, variables, expandParams: expandParams, staticMethodWithThisAsFirstArgument: staticMethodWithThisAsFirstArgument);
				VisitChildren(body);
				JsBlockStatement jsbody;
				if (_result.Count == 1 && _result[0] is JsBlockStatement)
					jsbody = (JsBlockStatement)_result[0];
				else
					jsbody = JsStatement.Block(_result);

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

		public JsBlockStatement Compile(Statement statement) {
			SetRegion(statement.GetRegion());
			try {
				_result = new List<JsStatement>();
				statement.AcceptVisitor(this);
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

		public IList<JsStatement> CompileConstructorInitializer(ConstructorInitializer initializer, bool currentIsStaticMethod) {
			SetRegion(initializer.GetRegion());
			try {
				var rr = _resolver.Resolve(initializer);
				if (rr is DynamicInvocationResolveResult) {
					_errorReporter.Message(Messages._7998, initializer.ConstructorInitializerType == ConstructorInitializerType.Base ? "dynamic invocation of base constructor" : "dynamic constructor chaining");
					return new JsStatement[0];
				}
				else {
					var csirr = (CSharpInvocationResolveResult)rr;
					return _expressionCompiler.CompileConstructorInitializer((IMethod)csirr.Member, csirr.GetArgumentsForCall(), csirr.GetArgumentToParameterMap(), csirr.InitializerStatements, currentIsStaticMethod);
				}
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public IList<JsStatement> CompileImplicitBaseConstructorCall(ITypeDefinition type, bool currentIsStaticMethod) {
			SetRegion(type.Region);
			try {
				var baseType = type.DirectBaseTypes.Single(t => t.Kind == TypeKind.Class);
				return _expressionCompiler.CompileConstructorInitializer(baseType.GetConstructors().Single(c => c.Parameters.Count == 0), new ResolveResult[0], new int[0], new ResolveResult[0], currentIsStaticMethod);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public IList<JsStatement> CompileFieldInitializer(DomRegion region, JsExpression jsThis, string scriptName, IMember member, Expression value) {
			SetRegion(region);
			try {
				var result = _expressionCompiler.Compile(ResolveWithConversion(value), true);
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

		public JsExpression CompileDelegateCombineCall(DomRegion region, JsExpression a, JsExpression b) {
			SetRegion(region);
			try {
				return _expressionCompiler.CompileDelegateCombineCall(a, b);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsExpression.Number(0);
			}
		}

		public JsExpression CompileDelegateRemoveCall(DomRegion region, JsExpression a, JsExpression b) {
			SetRegion(region);
			try {
				return _expressionCompiler.CompileDelegateRemoveCall(a, b);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return JsExpression.Number(0);
			}
		}

		public IList<JsStatement> CompileDefaultFieldInitializer(DomRegion region, JsExpression jsThis, string scriptName, IMember member) {
			SetRegion(region);
			try {
				var expr = _runtimeLibrary.InitializeField(jsThis, scriptName, member, _runtimeLibrary.Default(member.ReturnType, this), this);
				return expr != null ? new JsStatement[] { expr } : (IList<JsStatement>)EmptyList<JsStatement>.Instance;
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		private StatementCompiler CreateInnerCompiler() {
			return new StatementCompiler(_metadataImporter, _namer, _errorReporter, _compilation, _resolver, _variables, _nestedFunctions, _runtimeLibrary, _thisAlias, _usedVariableNames, _nestedFunctionContext, _methodBeingCompiled, _typeBeingCompiled, _expressionCompiler, _nextLabelIndex, _currentVariableForRethrow, _currentGotoCaseMap);
		}

		private IVariable CreateTemporaryVariable(IType type, DomRegion region) {
			string name = _namer.GetVariableName(null, _usedVariableNames);
			IVariable variable = new SimpleVariable(type, "temporary", region);
			_variables[variable] = new VariableData(name, null, false);
			_usedVariableNames.Add(name);
			return variable;
		}

		private ResolveResult ResolveWithConversion(Expression expr) {
			var rr = _resolver.Resolve(expr);
			var conversion = _resolver.GetConversion(expr);
			if (!conversion.IsIdentityConversion)
				rr = new ConversionResolveResult(_resolver.GetExpectedType(expr), rr, conversion);
			return rr;
		}

		[Flags]
		private enum CompileExpressionFlags {
			None = 0,
			ReturnValueIsImportant = 1,
			IsAssignmentSource = 2,
		}

		private ExpressionCompileResult CompileExpression(Expression expr, CompileExpressionFlags flags) {
			var oldRegion = _errorReporter.Region;
			try {
				_errorReporter.Region = expr.GetRegion();
				var rr = ResolveWithConversion(expr);
				var result = _expressionCompiler.Compile(rr, (flags & CompileExpressionFlags.ReturnValueIsImportant) != 0);
				if (((flags & CompileExpressionFlags.IsAssignmentSource) != 0) && IsMutableValueType(rr.Type)) {
					result.Expression = MaybeCloneValueType(result.Expression, rr, rr.Type);
				}
				return result;
			}
			finally {
				_errorReporter.Region = oldRegion;
			}
		}

		protected override void VisitChildren(AstNode node) {
			for (var child = node.FirstChild; child != null; child = child.NextSibling) {
				if (child is LabelStatement) {
					string name = ((LabelStatement)child).Label;
					do {
						child = child.NextSibling;
					} while (child.Role != BlockStatement.StatementRole && child.Role != Roles.EmbeddedStatement);
					int index = _result.Count;
					child.AcceptVisitor(this);
					_result[index] = JsStatement.Label(name, _result[index]);
				}
				else {
					SetRegion(child.GetRegion());
					child.AcceptVisitor(this);
				}
			}
		}

		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameter tp) {
			return Utils.ResolveTypeParameter(tp, _typeBeingCompiled, _methodBeingCompiled, _metadataImporter, _errorReporter, _namer);
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			return Utils.EnsureCanBeEvaluatedMultipleTimes(_result, expression, expressionsThatMustBeEvaluatedBefore, () => { var temp = CreateTemporaryVariable(SpecialType.UnknownType, DomRegion.Empty); return _variables[temp].Name; });
		}

		public override void VisitComment(Comment comment) {
			switch (comment.CommentType) {
				case CommentType.SingleLine: {
					_result.Add(JsStatement.Comment(comment.Content));
					break;
				}

				case CommentType.MultiLine: {
					string prefix = new Regex(@"^\s*").Match(comment.Content).Captures[0].Value;
					List<string> commentLines = comment.Content.Replace("\r", "").Split('\n').Select(item => item.Trim()).SkipWhile(l => l == "").ToList();
					while (commentLines.Count > 0 && commentLines[commentLines.Count - 1] == "")
						commentLines.RemoveAt(commentLines.Count - 1);

					if (commentLines.Count > 0)
						_result.Add(JsStatement.Comment(string.Join(Environment.NewLine, commentLines.Select(item => prefix + item))));	// Replace the space at the start of each line with the same as the space in the first line.
					break;
				}
					
				case CommentType.Documentation:
				case CommentType.MultiLineDocumentation:
					// Better to use the NRefactory XML support if we want these.
					break;
				case CommentType.InactiveCode:
					// Should not appear in script.
					break;
				default:
					throw new ArgumentException("Invalid comment type " + comment.CommentType);
			}
		}

		public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement) {
			var declarations = new List<JsVariableDeclaration>();
			foreach (var d in variableDeclarationStatement.Variables) {
				SetRegion(d.GetRegion());
				var variable = ((LocalResolveResult)_resolver.Resolve(d)).Variable;
				var data = _variables[variable];
				JsExpression jsInitializer;
				if (!d.Initializer.IsNull) {
					SetRegion(d.Initializer.GetRegion());
					var exprCompileResult = CompileExpression(d.Initializer, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
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

		private bool IsPartialMethodDeclaration(IMethod method) {
			var ur = (IUnresolvedMethod)method.UnresolvedMember;
			return ur.IsPartial && !ur.HasBody;
		}

		public override void VisitExpressionStatement(ExpressionStatement expressionStatement) {
			var resolveResult = ResolveWithConversion(expressionStatement.Expression);
			if (resolveResult is InvocationResolveResult) {
				var irr = (InvocationResolveResult)resolveResult;
				if (irr.IsConditionallyRemoved || IsPartialMethodDeclaration((IMethod)irr.Member)) {	// This test is OK according to https://github.com/icsharpcode/NRefactory/issues/12
					// Invocation of a partial method without definition - remove (yes, I too feel the arguments should be evaluated but the spec says no.
					return;
				}
			}

			var compiled = _expressionCompiler.Compile(resolveResult, false);
			_result.AddRange(compiled.AdditionalStatements);
			if (compiled.Expression.NodeType != ExpressionNodeType.Null)	// The statement "null;" is illegal in C#, so it must have appeared because there was no suitable expression to return.
				_result.Add(compiled.Expression);
		}

		public override void VisitForStatement(ForStatement forStatement) {
			// Initializer. In case we need more than one statement, put all other statements just before this loop.
			JsStatement initializer;
			if (forStatement.Initializers.Count == 1 && forStatement.Initializers.First() is VariableDeclarationStatement) {
				forStatement.Initializers.First().AcceptVisitor(this);
				initializer = _result[_result.Count - 1];
				_result.RemoveAt(_result.Count - 1);
			}
			else {
				JsExpression initExpr = null;
				foreach (var init in forStatement.Initializers) {
					var compiledInit = CompileExpression(((ExpressionStatement)init).Expression, CompileExpressionFlags.None);
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
			if (!forStatement.Condition.IsNull) {
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
			if (forStatement.Iterators.Count > 0) {
				var compiledIterators = forStatement.Iterators.Select(i => CompileExpression(((ExpressionStatement)i).Expression, CompileExpressionFlags.None)).ToList();
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
			var body = CreateInnerCompiler().Compile(forStatement.EmbeddedStatement);

			if (preBody != null || postBody != null) {
				body = JsStatement.Block(((IEnumerable<JsStatement>)preBody ?? new JsStatement[0]).Concat(body.Statements).Concat((IEnumerable<JsStatement>)postBody ?? new JsStatement[0]));
			}

			_result.Add(JsStatement.For(initializer, condition, iterator, body));
		}

		public override void VisitBreakStatement(BreakStatement breakStatement) {
			_result.Add(JsStatement.Break());
		}

		public override void VisitContinueStatement(ContinueStatement continueStatement) {
			_result.Add(JsStatement.Continue());
		}

		public override void VisitEmptyStatement(EmptyStatement emptyStatement) {
			_result.Add(JsStatement.Empty);
		}

		public override void VisitIfElseStatement(IfElseStatement ifElseStatement) {
			var compiledCond = CompileExpression(ifElseStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
			_result.AddRange(compiledCond.AdditionalStatements);
			_result.Add(JsStatement.If(compiledCond.Expression, CreateInnerCompiler().Compile(ifElseStatement.TrueStatement), !ifElseStatement.FalseStatement.IsNull ? CreateInnerCompiler().Compile(ifElseStatement.FalseStatement) : null));
		}

		public override void VisitBlockStatement(BlockStatement blockStatement) {
			var innerCompiler = CreateInnerCompiler();
			innerCompiler.VisitChildren(blockStatement);
			_result.Add(JsStatement.Block(innerCompiler._result));
		}

		public override void VisitCheckedStatement(CheckedStatement checkedStatement) {
			checkedStatement.Body.AcceptVisitor(this);
		}

		public override void VisitUncheckedStatement(UncheckedStatement uncheckedStatement) {
			uncheckedStatement.Body.AcceptVisitor(this);
		}

		public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement) {
			var body = CreateInnerCompiler().Compile(doWhileStatement.EmbeddedStatement);
			var compiledCondition = CompileExpression(doWhileStatement.Condition, CompileExpressionFlags.ReturnValueIsImportant);
			if (compiledCondition.AdditionalStatements.Count > 0)
				body = JsStatement.Block(body.Statements.Concat(compiledCondition.AdditionalStatements));
			_result.Add(JsStatement.DoWhile(compiledCondition.Expression, body));
		}

		public override void VisitWhileStatement(WhileStatement whileStatement) {
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

			var body = CreateInnerCompiler().Compile(whileStatement.EmbeddedStatement);
			if (preBody != null)
				body = JsStatement.Block(preBody.Concat(body.Statements));

			_result.Add(JsStatement.While(condition, body));
		}

		public override void VisitReturnStatement(ReturnStatement returnStatement) {
			if (!returnStatement.Expression.IsNull) {
				var expr = CompileExpression(returnStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
				_result.AddRange(expr.AdditionalStatements);
				_result.Add(JsStatement.Return(expr.Expression));
			}
			else {
				_result.Add(JsStatement.Return());
			}
		}

		public override void VisitLockStatement(LockStatement lockStatement) {
			var expr = CompileExpression(lockStatement.Expression, CompileExpressionFlags.None);
			_result.AddRange(expr.AdditionalStatements);
			_result.Add(expr.Expression);
			lockStatement.EmbeddedStatement.AcceptVisitor(this);
		}

		public override void VisitForeachStatement(ForeachStatement foreachStatement) {
			var ferr = (ForEachResolveResult)_resolver.Resolve(foreachStatement);
			var iterator = (LocalResolveResult)_resolver.Resolve(foreachStatement.VariableNameToken);

			var getEnumeratorMethod = (ferr.GetEnumeratorCall is InvocationResolveResult ? ((InvocationResolveResult)ferr.GetEnumeratorCall).Member as IMethod : null);

			var systemArray = _compilation.FindType(KnownTypeCode.Array);
			var inExpression = ResolveWithConversion(foreachStatement.InExpression);
			if (Equals(inExpression.Type, systemArray) || inExpression.Type.DirectBaseTypes.Contains(systemArray) || (getEnumeratorMethod != null && _metadataImporter.GetMethodSemantics(getEnumeratorMethod).EnumerateAsArray)) {
				var arrayResult = CompileExpression(foreachStatement.InExpression, CompileExpressionFlags.ReturnValueIsImportant);
				_result.AddRange(arrayResult.AdditionalStatements);
				var array = arrayResult.Expression;
				if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(array)) {
					var tmpArray = CreateTemporaryVariable(ferr.CollectionType, foreachStatement.GetRegion());
					_result.Add(JsStatement.Var(_variables[tmpArray].Name, array));
					array = JsExpression.Identifier(_variables[tmpArray].Name);
				}

				var length = systemArray.GetProperties().SingleOrDefault(p => p.Name == "Length");
				if (length == null) {
					_errorReporter.InternalError("Property Array.Length not found.");
					return;
				}
				var lengthSem = _metadataImporter.GetPropertySemantics(length);
				if (lengthSem.Type != PropertyScriptSemantics.ImplType.Field) {
					_errorReporter.InternalError("Property Array.Length is not implemented as a field.");
					return;
				}

				var index = CreateTemporaryVariable(_compilation.FindType(KnownTypeCode.Int32), foreachStatement.GetRegion());
				var jsIndex = JsExpression.Identifier(_variables[index].Name);
				JsExpression iteratorValue = MaybeCloneValueType(JsExpression.Index(array, jsIndex), null, ferr.ElementType);
				if (_variables[iterator.Variable].UseByRefSemantics)
					iteratorValue = JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", iteratorValue));

				var body = new[] { JsStatement.Var(_variables[iterator.Variable].Name, iteratorValue) }
				          .Concat(CreateInnerCompiler().Compile(foreachStatement.EmbeddedStatement).Statements);

				_result.Add(JsStatement.For(JsStatement.Var(_variables[index].Name, JsExpression.Number(0)),
				                            JsExpression.Lesser(jsIndex, JsExpression.Member(array, lengthSem.FieldName)),
				                            JsExpression.PostfixPlusPlus(jsIndex),
				                            JsStatement.Block(body)));
			}
			else {
				var getEnumeratorCall = _expressionCompiler.Compile(ferr.GetEnumeratorCall, true);
				_result.AddRange(getEnumeratorCall.AdditionalStatements);
				var enumerator = CreateTemporaryVariable(ferr.EnumeratorType, foreachStatement.GetRegion());
				_result.Add(JsStatement.Var(_variables[enumerator].Name, getEnumeratorCall.Expression));

				var moveNextInvocation = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new LocalResolveResult(enumerator), ferr.MoveNextMethod, new ResolveResult[0]), true);
				if (moveNextInvocation.AdditionalStatements.Count > 0)
					_errorReporter.InternalError("MoveNext() invocation is not allowed to require additional statements.");

				var getCurrent = _expressionCompiler.Compile(new MemberResolveResult(new LocalResolveResult(enumerator), ferr.CurrentProperty), true);
				JsExpression getCurrentValue = MaybeCloneValueType(getCurrent.Expression, null, ferr.ElementType);
				if (_variables[iterator.Variable].UseByRefSemantics)
					getCurrentValue = JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", getCurrentValue));

				var preBody = getCurrent.AdditionalStatements.Concat(new[] { JsStatement.Var(_variables[iterator.Variable].Name, getCurrentValue) }).ToList();
				var body = CreateInnerCompiler().Compile(foreachStatement.EmbeddedStatement);

				body = JsStatement.Block(preBody.Concat(body.Statements));

				JsStatement disposer;

				var systemIDisposable = _compilation.FindType(KnownTypeCode.IDisposable);
				var disposeMethod = systemIDisposable.GetMethods().Single(m => m.Name == "Dispose");
				var conversions = CSharpConversions.Get(_compilation);
				var disposableConversion = conversions.ImplicitConversion(enumerator.Type, systemIDisposable);
				if (disposableConversion.IsValid) {
					// If the enumerator is implicitly convertible to IDisposable, we should dispose it.
					var compileResult = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new ConversionResolveResult(systemIDisposable, new LocalResolveResult(enumerator), disposableConversion), disposeMethod, new ResolveResult[0]), false);
					if (compileResult.AdditionalStatements.Count != 0)
						_errorReporter.InternalError("Call to IDisposable.Dispose must not return additional statements.");
					disposer = compileResult.Expression;
				}
				else if (enumerator.Type.GetDefinition().IsSealed) {
					// If the enumerator is sealed and not implicitly convertible to IDisposable, we need not dispose it.
					disposer = null;
				}
				else {
					// We don't know whether the enumerator is convertible to IDisposable, so we need to conditionally dispose it.
					var test = _expressionCompiler.Compile(new TypeIsResolveResult(new LocalResolveResult(enumerator), systemIDisposable, _compilation.FindType(KnownTypeCode.Boolean)), true);
					if (test.AdditionalStatements.Count > 0)
						_errorReporter.InternalError("\"is\" test must not return additional statements.");
					var innerStatements = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new ConversionResolveResult(systemIDisposable, new LocalResolveResult(enumerator), conversions.ExplicitConversion(enumerator.Type, systemIDisposable)), disposeMethod, new ResolveResult[0]), false);
					disposer = JsStatement.If(test.Expression, JsStatement.Block(innerStatements.AdditionalStatements.Concat(new JsStatement[] { innerStatements.Expression })), null);
				}
				JsStatement stmt = JsStatement.While(moveNextInvocation.Expression, body);
				if (disposer != null)
					stmt = JsStatement.Try(stmt, null, disposer);
				_result.Add(stmt);
			}
		}

		private JsBlockStatement GenerateUsingBlock(LocalResolveResult resource, Expression aquisitionExpression, DomRegion tempVariableRegion, JsBlockStatement body) {
			SetRegion(aquisitionExpression.GetRegion());
			var boolType = _compilation.FindType(KnownTypeCode.Boolean);
			var systemIDisposable = _compilation.FindType(KnownTypeCode.IDisposable);
			var disposeMethod = systemIDisposable.GetMethods().Single(m => m.Name == "Dispose");
			var conversions = CSharpConversions.Get(_compilation);

			var aquisitionResolveResult = _resolver.Resolve(aquisitionExpression);
			var compiledAquisition = _expressionCompiler.Compile(aquisitionResolveResult, true);

			var stmts = new List<JsStatement>();
			stmts.AddRange(compiledAquisition.AdditionalStatements);
			stmts.Add(JsStatement.Var(_variables[resource.Variable].Name, MaybeCloneValueType(compiledAquisition.Expression, aquisitionResolveResult, resource.Type)));

			bool isDynamic = resource.Type.Kind == TypeKind.Dynamic;

			if (isDynamic) {
				var newResource = CreateTemporaryVariable(systemIDisposable, tempVariableRegion);
				var castExpr = _expressionCompiler.Compile(new ConversionResolveResult(systemIDisposable, resource, conversions.ExplicitConversion(resource, systemIDisposable)), true);
				stmts.AddRange(castExpr.AdditionalStatements);
				stmts.Add(JsStatement.Var(_variables[newResource].Name, castExpr.Expression));
				resource = new LocalResolveResult(newResource);
			}

			var compiledDisposeCall = _expressionCompiler.Compile(
			                              new CSharpInvocationResolveResult(
			                                  new ConversionResolveResult(systemIDisposable, resource, conversions.ImplicitConversion(resource, systemIDisposable)),
			                                  disposeMethod,
			                                  new ResolveResult[0]
			                              ), false);
			if (compiledDisposeCall.AdditionalStatements.Count > 0)
				_errorReporter.InternalError("Type test cannot return additional statements.");

			JsStatement releaseStmt;
			if (isDynamic) {
				releaseStmt = compiledDisposeCall.Expression;
			}
			else {
				// if (d != null) ((IDisposable)d).Dispose()
				var compiledTest = _expressionCompiler.Compile(new OperatorResolveResult(boolType, ExpressionType.NotEqual, resource, new ConstantResolveResult(resource.Type, null)), true);
				if (compiledTest.AdditionalStatements.Count > 0)
					_errorReporter.InternalError("Null test cannot return additional statements.");
				releaseStmt = resource.Type.IsReferenceType == false && !resource.Type.IsKnownType(KnownTypeCode.NullableOfT) ? (JsStatement)compiledDisposeCall.Expression : JsStatement.If(compiledTest.Expression, compiledDisposeCall.Expression, null);
			}

			stmts.Add(JsStatement.Try(body, null, releaseStmt));

			return JsStatement.Block(stmts);
		}

		public override void VisitUsingStatement(UsingStatement usingStatement) {
			var stmt = CreateInnerCompiler().Compile(usingStatement.EmbeddedStatement);

			var vds = usingStatement.ResourceAcquisition as VariableDeclarationStatement;
			if (vds != null) {
				foreach (var resource in vds.Variables.Reverse()) {
					stmt = GenerateUsingBlock(((LocalResolveResult)_resolver.Resolve(resource)), resource.Initializer, usingStatement.GetRegion(), stmt);
				}
			}
			else {
				var resource = CreateTemporaryVariable(_resolver.Resolve((Expression)usingStatement.ResourceAcquisition).Type, usingStatement.GetRegion());
				stmt = GenerateUsingBlock(new LocalResolveResult(resource), (Expression)usingStatement.ResourceAcquisition, usingStatement.GetRegion(), stmt);
			}

			_result.Add(stmt);
		}

		private void RemoveCatchClausesAfterExceptionType(List<CatchClause> catchClauses, IType exceptionType) {
			for (int i = 0; i < catchClauses.Count; i++) {
				var type = _resolver.Resolve(catchClauses[i].Type).Type;
				if (type.Equals(exceptionType)) {
					catchClauses.RemoveRange(i + 1, catchClauses.Count - i - 1);
					return;
				}
			}
		}

		private JsBlockStatement CompileCatchClause(LocalResolveResult catchVariable, CatchClause catchClause, bool isCatchAll, bool isOnly) {
			SetRegion(catchClause.GetRegion());
			JsStatement variableDeclaration = null;
			if (!catchClause.VariableNameToken.IsNull) {
				JsExpression compiledAssignment;
				if (isCatchAll)	// If this is the only handler we need to construct the exception
					compiledAssignment = isOnly ? _runtimeLibrary.MakeException(JsExpression.Identifier(_variables[catchVariable.Variable].Name), this) : JsExpression.Identifier(_variables[catchVariable.Variable].Name);
				else
					compiledAssignment = _runtimeLibrary.Downcast(JsExpression.Identifier(_variables[catchVariable.Variable].Name), _compilation.FindType(KnownTypeCode.Exception), _resolver.Resolve(catchClause.Type).Type, this);

				variableDeclaration = JsStatement.Var(_variables[((LocalResolveResult)_resolver.Resolve(catchClause.VariableNameToken)).Variable].Name, compiledAssignment);
			}

			var result = CreateInnerCompiler().Compile(catchClause.Body);
			if (variableDeclaration != null)
				result = JsStatement.Block(new[] { variableDeclaration }.Concat(result.Statements));
			return result;
		}

		public override void VisitTryCatchStatement(TryCatchStatement tryCatchStatement) {
			var tryBlock = CreateInnerCompiler().Compile(tryCatchStatement.TryBlock);
			JsCatchClause catchClause = null;
			if (tryCatchStatement.CatchClauses.Count > 0) {
				var oldVariableForRethrow = _currentVariableForRethrow;

				_currentVariableForRethrow = CreateTemporaryVariable(_compilation.FindType(KnownTypeCode.Object), tryCatchStatement.CatchClauses.First().GetRegion());
				string catchVariableName = _variables[_currentVariableForRethrow].Name;

				var catchClauses = tryCatchStatement.CatchClauses.ToList();
				var systemException = _compilation.FindType(KnownTypeCode.Exception);
				RemoveCatchClausesAfterExceptionType(catchClauses, systemException);

				bool lastIsCatchall = (catchClauses[catchClauses.Count - 1].Type.IsNull || _resolver.Resolve(catchClauses[catchClauses.Count - 1].Type).Type.Equals(systemException));
				JsStatement current = lastIsCatchall
				                    ? CompileCatchClause(new LocalResolveResult(_currentVariableForRethrow), catchClauses[catchClauses.Count - 1], true, catchClauses.Count == 1)
				                    : JsStatement.Block(JsStatement.Throw(JsExpression.Identifier(catchVariableName)));

				for (int i = catchClauses.Count - (lastIsCatchall ? 2 : 1); i >= 0; i--) {
					var test = _runtimeLibrary.TypeIs(JsExpression.Identifier(catchVariableName), _currentVariableForRethrow.Type, _resolver.Resolve(catchClauses[i].Type).Type, this);
					current = JsStatement.If(test, CompileCatchClause(new LocalResolveResult(_currentVariableForRethrow), catchClauses[i], false, catchClauses.Count == 1), current);
				}

				if (!lastIsCatchall || catchClauses.Count > 1) {
					// We need to wrap the exception.
					current = JsStatement.Block(JsExpression.Assign(JsExpression.Identifier(catchVariableName), _runtimeLibrary.MakeException(JsExpression.Identifier(catchVariableName), this)), current);
				}

				catchClause = JsStatement.Catch(catchVariableName, current);
				_currentVariableForRethrow = oldVariableForRethrow;
			}

			var finallyBlock = (!tryCatchStatement.FinallyBlock.IsNull ? CreateInnerCompiler().Compile(tryCatchStatement.FinallyBlock) : null);

			_result.Add(JsStatement.Try(tryBlock, catchClause, finallyBlock));
		}

		public override void VisitThrowStatement(ThrowStatement throwStatement) {
			if (throwStatement.Expression.IsNull) {
				_result.Add(JsStatement.Throw(JsExpression.Identifier(_variables[_currentVariableForRethrow].Name)));
			}
			else {
				var compiledExpr = CompileExpression(throwStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant);
				_result.AddRange(compiledExpr.AdditionalStatements);
				_result.Add(JsStatement.Throw(compiledExpr.Expression));
			}
		}

		public override void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) {
			_result.Add(JsStatement.Yield(null));
		}

		public override void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) {
			var compiledExpr = CompileExpression(yieldReturnStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant | CompileExpressionFlags.IsAssignmentSource);
			_result.AddRange(compiledExpr.AdditionalStatements);
			_result.Add(JsStatement.Yield(compiledExpr.Expression));
		}

		public override void VisitGotoStatement(GotoStatement gotoStatement) {
			_result.Add(JsStatement.Goto(gotoStatement.Label));
		}

		public override void VisitLabelStatement(LabelStatement labelStatement) {
			throw new InvalidOperationException("Visited a LabelStatement in the statement compiler, this should have been taken care of in parent.");
		}

		public override void VisitFixedStatement(FixedStatement fixedStatement) {
			throw new InvalidOperationException("fixed statement is not supported");	// Should be caught during the compilation step.
		}

		public override void VisitUnsafeStatement(UnsafeStatement unsafeStatement) {
			throw new InvalidOperationException("unsafe statement is not supported");	// Should be caught during the compilation step.
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

		private class GatherGotoCaseAndDefaultDataVisitor : DepthFirstAstVisitor {
			private Dictionary<object, SwitchSection> _sectionLookup;
			private Dictionary<SwitchSection, string> _labels;
			private Dictionary<object, string> _gotoCaseMap;
			private readonly CSharpAstResolver _resolver;
			private readonly SharedValue<int> _nextLabelIndex;

			public GatherGotoCaseAndDefaultDataVisitor(CSharpAstResolver resolver, SharedValue<int> nextLabelIndex) {
				_resolver = resolver;
				_nextLabelIndex = nextLabelIndex;
			}

			public Tuple<Dictionary<SwitchSection, string>, Dictionary<object, string>> Process(SwitchStatement switchStatement) {
				_labels      = new Dictionary<SwitchSection, string>();
				_gotoCaseMap = new Dictionary<object, string>();
				_sectionLookup = (  from section in switchStatement.SwitchSections
				                    from label in section.CaseLabels
				                  select new { section, rr = !label.Expression.IsNull ? _resolver.Resolve(label.Expression) : null }
				                 ).ToDictionary(x => x.rr != null ? NormalizeSwitchLabelValue(x.rr.ConstantValue) : _gotoCaseMapDefaultKey, x => x.section);

				foreach (var section in switchStatement.SwitchSections)
					section.AcceptVisitor(this);

				return Tuple.Create(_labels, _gotoCaseMap);
			}

			private void HandleGoto(object labelValue) {
				var targetSection = _sectionLookup[labelValue];
				if (!_labels.ContainsKey(targetSection)) {
					_labels.Add(targetSection, string.Format(CultureInfo.InvariantCulture, "$label{0}", _nextLabelIndex.Value++));
				}
				if (!_gotoCaseMap.ContainsKey(labelValue))
					_gotoCaseMap[labelValue] = _labels[targetSection];
			}

			public override void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) {
				HandleGoto(NormalizeSwitchLabelValue(_resolver.Resolve(gotoCaseStatement.LabelExpression).ConstantValue));
			}

			public override void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) {
				HandleGoto(_gotoCaseMapDefaultKey);
			}

			public override void VisitSwitchStatement(SwitchStatement switchStatement) {
				// Switch statements start a new context so we don't want to go there.
			}
		}

		public override void VisitSwitchStatement(SwitchStatement switchStatement) {
			var compiledExpr = CompileExpression(switchStatement.Expression, CompileExpressionFlags.ReturnValueIsImportant);
			_result.AddRange(compiledExpr.AdditionalStatements);

			var oldGotoCaseMap = _currentGotoCaseMap;

			var gotoCaseData = new GatherGotoCaseAndDefaultDataVisitor(_resolver, _nextLabelIndex).Process(switchStatement);
			_currentGotoCaseMap = gotoCaseData.Item2;

			var caseClauses = new List<JsSwitchSection>();
			foreach (var section in switchStatement.SwitchSections) {
				SetRegion(section.GetRegion());
				var values = new List<JsExpression>();
				foreach (var v in section.CaseLabels) {
					if (v.Expression.IsNull) {
						values.Add(null);	// Default
					}
					else {
						var rr = _resolver.Resolve(v.Expression);
						object value = rr.ConstantValue;
						if (rr is MemberResolveResult && ((MemberResolveResult)rr).Member is IField) {
							var sem = _metadataImporter.GetFieldSemantics((IField)((MemberResolveResult)rr).Member);
							if (sem.Type == FieldScriptSemantics.ImplType.Constant)
								value = sem.Value;
						}

						if (value == null) {
							values.Add(JsExpression.Null);
						}
						else if (value is string) {
							values.Add(JsExpression.String((string)value));
						}
						else {
							values.Add(JsExpression.Number((int)Convert.ChangeType(value, typeof(int))));
						}
					}
				}

				var ic = CreateInnerCompiler();
				IList<JsStatement> statements;
				if (section.Statements.Count == 1 && section.Statements.First() is BlockStatement) {
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

		public override void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) {
			var value = _resolver.Resolve(gotoCaseStatement.LabelExpression).ConstantValue;
			_result.Add(JsStatement.Goto(_currentGotoCaseMap[NormalizeSwitchLabelValue(value)]));
		}

		public override void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) {
			_result.Add(JsStatement.Goto(_currentGotoCaseMap[_gotoCaseMapDefaultKey]));
		}
	}
}
