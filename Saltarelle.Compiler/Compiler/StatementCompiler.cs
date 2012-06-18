using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using ExpressionType = System.Linq.Expressions.ExpressionType;

namespace Saltarelle.Compiler.Compiler {
	public class StatementCompiler : DepthFirstAstVisitor {
		private readonly INamingConventionResolver _namingConvention;
		private readonly IErrorReporter _errorReporter;
		private readonly ICompilation _compilation;
		private readonly CSharpAstResolver _resolver;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;
		private readonly ExpressionCompiler _expressionCompiler;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly string _thisAlias;
		private readonly HashSet<string> _usedVariableNames;
		private readonly NestedFunctionContext _nestedFunctionContext;
		private readonly SharedValue<int> _nextLabelIndex;

		private IVariable _currentVariableForRethrow;
		private IDictionary<object, string> _currentGotoCaseMap;

		private List<JsStatement> _result;

		public StatementCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, IRuntimeLibrary runtimeLibrary, string thisAlias, HashSet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext)
			: this(namingConvention, errorReporter, compilation, resolver, variables, nestedFunctions, runtimeLibrary, thisAlias, usedVariableNames, nestedFunctionContext, null, null, null, null)
		{
		}

		internal StatementCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, IRuntimeLibrary runtimeLibrary, string thisAlias, HashSet<string> usedVariableNames, NestedFunctionContext nestedFunctionContext, ExpressionCompiler expressionCompiler, SharedValue<int> nextLabelIndex, IVariable currentVariableForRethrow, IDictionary<object, string> currentGotoCaseMap) {
			_namingConvention           = namingConvention;
			_errorReporter              = errorReporter;
			_compilation                = compilation;
			_resolver                   = resolver;
			_variables                  = variables;
			_nestedFunctions            = nestedFunctions;
			_runtimeLibrary             = runtimeLibrary;
			_thisAlias                  = thisAlias;
			_usedVariableNames          = usedVariableNames;
			_nestedFunctionContext      = nestedFunctionContext;
			_currentVariableForRethrow  = currentVariableForRethrow;
			_currentGotoCaseMap         = currentGotoCaseMap;

			_nextLabelIndex             = nextLabelIndex ?? new SharedValue<int>(1);

			_expressionCompiler         = expressionCompiler ?? new ExpressionCompiler(compilation, namingConvention, runtimeLibrary, errorReporter, variables, nestedFunctions, CreateTemporaryVariable, c => new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _variables, _nestedFunctions, _runtimeLibrary, thisAlias, _usedVariableNames, c), thisAlias, nestedFunctionContext, null);
			_result                     = new List<JsStatement>();
		}

		public JsBlockStatement Compile(Statement statement) {
			_result = new List<JsStatement>();
			statement.AcceptVisitor(this);
			if (_result.Count == 1 && _result[0] is JsBlockStatement)
				return (JsBlockStatement)_result[0];
			else
				return new JsBlockStatement(_result);
		}

		public IList<JsStatement> CompileConstructorInitializer(ConstructorInitializer initializer, bool currentIsStaticMethod) {
			var rr = (CSharpInvocationResolveResult)_resolver.Resolve(initializer);
			return _expressionCompiler.CompileConstructorInitializer((IMethod)rr.Member, rr.GetArgumentsForCall(), rr.GetArgumentToParameterMap(), rr.InitializerStatements, currentIsStaticMethod);
		}

		public IList<JsStatement> CompileImplicitBaseConstructorCall(IType type, bool currentIsStaticMethod) {
			var baseType = type.DirectBaseTypes.Single(t => t.Kind == TypeKind.Class);
			return _expressionCompiler.CompileConstructorInitializer(baseType.GetConstructors().Single(c => c.Parameters.Count == 0), new ResolveResult[0], new int[0], new ResolveResult[0],  currentIsStaticMethod);
		}

        public IList<JsStatement> CompileFieldInitializer(JsExpression field, Expression expression) {
            var result = _expressionCompiler.Compile(ResolveWithConversion(expression), true);
            return result.AdditionalStatements.Concat(new[] { new JsExpressionStatement(JsExpression.Assign(field, result.Expression)) }).ToList();
        }

		public JsExpression CompileDelegateCombineCall(JsExpression a, JsExpression b) {
			return _expressionCompiler.CompileDelegateCombineCall(a, b);
		}

		public JsExpression CompileDelegateRemoveCall(JsExpression a, JsExpression b) {
			return _expressionCompiler.CompileDelegateRemoveCall(a, b);
		}

		public IList<JsStatement> CompileDefaultFieldInitializer(JsExpression field, IType type) {
			JsExpression value;
			if (type.IsReferenceType == true) {
				value = JsExpression.Null;
			}
			else if (type.IsReferenceType == null) {
				value = _runtimeLibrary.Default(type);
			}
			else {
				var code = type.GetDefinition().KnownTypeCode;
				switch (code) {
					case KnownTypeCode.Boolean:
						value = JsExpression.False;
						break;
					case KnownTypeCode.NullableOfT:
						value = JsExpression.Null;
						break;
					default:
						// This might not hold in the future, but it does today. Since we don't support user-defined structs, we know that the only value types we have are numbers.
						value = JsExpression.Number(0);
						break;
				}
			}

			return new[] { new JsExpressionStatement(JsExpression.Assign(field, value)) };
		}

		private StatementCompiler CreateInnerCompiler() {
			return new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _variables, _nestedFunctions, _runtimeLibrary, _thisAlias, _usedVariableNames, _nestedFunctionContext, _expressionCompiler, _nextLabelIndex, _currentVariableForRethrow, _currentGotoCaseMap);
		}

		private IVariable CreateTemporaryVariable(IType type) {
			string name = _namingConvention.GetVariableName(null, _usedVariableNames);
			IVariable variable = new SimpleVariable(type, "temporary");
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

		private ExpressionCompiler.Result CompileExpression(Expression expr, bool returnValueIsImportant) {
			return _expressionCompiler.Compile(ResolveWithConversion(expr), returnValueIsImportant);
		}

		public override void VisitComment(Comment comment) {
			switch (comment.CommentType) {
				case CommentType.SingleLine: {
					_result.Add(new JsComment(comment.Content));
					break;
				}

				case CommentType.MultiLine: {
					string prefix = new Regex(@"^\s*").Match(comment.Content).Captures[0].Value;
					List<string> commentLines = comment.Content.Replace("\r", "").Split('\n').Select(item => item.Trim()).SkipWhile(l => l == "").ToList();
					while (commentLines.Count > 0 && commentLines[commentLines.Count - 1] == "")
						commentLines.RemoveAt(commentLines.Count - 1);

					if (commentLines.Count > 0)
						_result.Add(new JsComment(string.Join(Environment.NewLine, commentLines.Select(item => prefix + item))));	// Replace the space at the start of each line with the same as the space in the first line.
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
				var variable = ((LocalResolveResult)_resolver.Resolve(d)).Variable;
				var data = _variables[variable];
				JsExpression jsInitializer;
				if (!d.Initializer.IsNull) {
					var initializer = ResolveWithConversion(d.Initializer);

					var exprCompileResult = _expressionCompiler.Compile(initializer, true);
					if (exprCompileResult.AdditionalStatements.Count > 0) {
						if (declarations.Count > 0) {
							_result.Add(new JsVariableDeclarationStatement(declarations));
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
				declarations.Add(new JsVariableDeclaration(data.Name, jsInitializer));
			}

			if (declarations.Count > 0)
				_result.Add(new JsVariableDeclarationStatement(declarations));
		}

		public override void VisitExpressionStatement(ExpressionStatement expressionStatement) {
			var resolveResult = ResolveWithConversion(expressionStatement.Expression);
			if (   resolveResult is InvocationResolveResult
			    && ((InvocationResolveResult)resolveResult).Member is IMethod
				&& ((IUnresolvedMethod)((InvocationResolveResult)resolveResult).Member.UnresolvedMember).IsPartialMethodDeclaration)	// This test is OK according to https://github.com/icsharpcode/NRefactory/issues/12
			{
				// Invocation of a partial method without definition - remove (yes, I too feel the arguments should be evaluated but the spec says no.
				return;
			}

			var compiled = _expressionCompiler.Compile(resolveResult, false);
			_result.AddRange(compiled.AdditionalStatements);
			_result.Add(new JsExpressionStatement(compiled.Expression));
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
					var compiledInit = CompileExpression(((ExpressionStatement)init).Expression, false);
					if (compiledInit.AdditionalStatements.Count == 0) {
						initExpr = (initExpr != null ? JsExpression.Comma(initExpr, compiledInit.Expression) : compiledInit.Expression);
					}
					else {
						if (initExpr != null)
							_result.Add(new JsExpressionStatement(initExpr));
						_result.AddRange(compiledInit.AdditionalStatements);
						initExpr = compiledInit.Expression;
					}
				}
				initializer = (initExpr != null ? (JsStatement)new JsExpressionStatement(initExpr) : (JsStatement)new JsEmptyStatement());
			}

			// Condition
			JsExpression condition;
			List<JsStatement> preBody = null;
			if (!forStatement.Condition.IsNull) {
				var compiledCondition = CompileExpression(forStatement.Condition, true);
				if (compiledCondition.AdditionalStatements.Count == 0) {
					condition = compiledCondition.Expression;
				}
				else {
					// The condition requires additional statements. Transform "for (int i = 0; i < (SomeProperty = 1); i++) { ... }" to "for (var i = 0;; i++) { this.set_SomeProperty(1); if (!(i < 1)) { break; } ... }
					preBody = new List<JsStatement>();
					preBody.AddRange(compiledCondition.AdditionalStatements);
					preBody.Add(new JsIfStatement(JsExpression.LogicalNot(compiledCondition.Expression), new JsBreakStatement(), null));
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
				var compiledIterators = forStatement.Iterators.Select(i => CompileExpression(((ExpressionStatement)i).Expression, false)).ToList();
				if (compiledIterators.All(i => i.AdditionalStatements.Count == 0)) {
					// No additional statements are required, add them as a single comma-separated expression to the JS iterator.
					iterator = compiledIterators.Aggregate(iterator, (current, i) => (current != null ? JsExpression.Comma(current, i.Expression) : i.Expression));
				}
				else {
					// At least one of the compiled iterators need additional statements. We could add the last expressions that don't need extra statements to the iterators section of the for loop, but for simplicity we'll just add everything to the end of the loop.
					postBody = new List<JsStatement>();
					foreach (var i in compiledIterators) {
						postBody.AddRange(i.AdditionalStatements);
						postBody.Add(new JsExpressionStatement(i.Expression));
					}
				}
			}

			// Body
			var body = CreateInnerCompiler().Compile(forStatement.EmbeddedStatement);

			if (preBody != null || postBody != null) {
				body = new JsBlockStatement(((IEnumerable<JsStatement>)preBody ?? new JsStatement[0]).Concat(body.Statements).Concat((IEnumerable<JsStatement>)postBody ?? new JsStatement[0]));
			}

			_result.Add(new JsForStatement(initializer, condition, iterator, body));
		}

		public override void VisitBreakStatement(BreakStatement breakStatement) {
			_result.Add(new JsBreakStatement());
		}

		public override void VisitContinueStatement(ContinueStatement continueStatement) {
			_result.Add(new JsContinueStatement());
		}

		public override void VisitEmptyStatement(EmptyStatement emptyStatement) {
			_result.Add(new JsEmptyStatement());
		}

		public override void VisitIfElseStatement(IfElseStatement ifElseStatement) {
			var compiledCond = CompileExpression(ifElseStatement.Condition, true);
			_result.AddRange(compiledCond.AdditionalStatements);
			_result.Add(new JsIfStatement(compiledCond.Expression, CreateInnerCompiler().Compile(ifElseStatement.TrueStatement), !ifElseStatement.FalseStatement.IsNull ? CreateInnerCompiler().Compile(ifElseStatement.FalseStatement) : null));
		}

		public override void VisitBlockStatement(BlockStatement blockStatement) {
			var innerCompiler = CreateInnerCompiler();
			foreach (var c in blockStatement.Children)
				c.AcceptVisitor(innerCompiler);
			_result.Add(new JsBlockStatement(innerCompiler._result));
		}

		public override void VisitCheckedStatement(CheckedStatement checkedStatement) {
			checkedStatement.Body.AcceptVisitor(this);
		}

		public override void VisitUncheckedStatement(UncheckedStatement uncheckedStatement) {
			uncheckedStatement.Body.AcceptVisitor(this);
		}

		public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement) {
			var body = CreateInnerCompiler().Compile(doWhileStatement.EmbeddedStatement);
			var compiledCondition = CompileExpression(doWhileStatement.Condition, true);
			if (compiledCondition.AdditionalStatements.Count > 0)
				body = new JsBlockStatement(body.Statements.Concat(compiledCondition.AdditionalStatements));
			_result.Add(new JsDoWhileStatement(compiledCondition.Expression, body));
		}

		public override void VisitWhileStatement(WhileStatement whileStatement) {
			// Condition
			JsExpression condition;
			List<JsStatement> preBody = null;
			var compiledCondition = CompileExpression(whileStatement.Condition, true);
			if (compiledCondition.AdditionalStatements.Count == 0) {
				condition = compiledCondition.Expression;
			}
			else {
				// The condition requires additional statements. Transform "while ((SomeProperty = 1) < 0) { ... }" to "while (true) { this.set_SomeProperty(1); if (!(i < 1)) { break; } ... }
				preBody = new List<JsStatement>();
				preBody.AddRange(compiledCondition.AdditionalStatements);
				preBody.Add(new JsIfStatement(JsExpression.LogicalNot(compiledCondition.Expression), new JsBreakStatement(), null));
				condition = JsExpression.True;
			}

			var body = CreateInnerCompiler().Compile(whileStatement.EmbeddedStatement);
			if (preBody != null)
				body = new JsBlockStatement(preBody.Concat(body.Statements));

			_result.Add(new JsWhileStatement(condition, body));
		}

		public override void VisitReturnStatement(ReturnStatement returnStatement) {
			if (!returnStatement.Expression.IsNull) {
				var expr = CompileExpression(returnStatement.Expression, true);
				_result.AddRange(expr.AdditionalStatements);
				_result.Add(new JsReturnStatement(expr.Expression));
			}
			else {
				_result.Add(new JsReturnStatement());
			}
		}

		public override void VisitLockStatement(LockStatement lockStatement) {
			var expr = CompileExpression(lockStatement.Expression, false);
			_result.AddRange(expr.AdditionalStatements);
			_result.Add(new JsExpressionStatement(expr.Expression));
			lockStatement.EmbeddedStatement.AcceptVisitor(this);
		}

		public override void VisitForeachStatement(ForeachStatement foreachStatement) {
			var ferr = (ForEachResolveResult)_resolver.Resolve(foreachStatement);

			var getEnumeratorCall = _expressionCompiler.Compile(ferr.GetEnumeratorCall, true);
			_result.AddRange(getEnumeratorCall.AdditionalStatements);
			var enumerator = CreateTemporaryVariable(ferr.EnumeratorType);
			_result.Add(new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[enumerator].Name, getEnumeratorCall.Expression)));

			var moveNextInvocation = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new LocalResolveResult(enumerator), ferr.MoveNextMethod, new ResolveResult[0]), true);
			if (moveNextInvocation.AdditionalStatements.Count > 0)
				_errorReporter.Error("MoveNext() invocation is not allowed to require additional statements.");

			var getCurrent = _expressionCompiler.Compile(new MemberResolveResult(new LocalResolveResult(enumerator), ferr.CurrentProperty), true);
			var iterator = (LocalResolveResult)_resolver.Resolve(foreachStatement.VariableNameToken);
			var preBody = getCurrent.AdditionalStatements.Concat(new[] { new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[iterator.Variable].Name, getCurrent.Expression)) }).ToList();
			var body = CreateInnerCompiler().Compile(foreachStatement.EmbeddedStatement);

			body = new JsBlockStatement(preBody.Concat(body.Statements));

			JsStatement disposer;
			var systemArray = _compilation.FindType(KnownTypeCode.Array);
			var inExpression = ResolveWithConversion(foreachStatement.InExpression);	// Needed to check whether we're enumerating an array (can avoid dispose). ferr.CollectionType will in this case be IEnumerable, so it's not useful.
			if (inExpression.Type == systemArray || inExpression.Type.DirectBaseTypes.Contains(systemArray)) {
				// Don't dispose array enumerators (we should according to C#, but it uglifies the script and we know it's a no-op.)
				disposer = null;
			}
			else {
				var systemIDisposable = _compilation.FindType(KnownTypeCode.IDisposable);
				var disposeMethod = systemIDisposable.GetMethods().Single(m => m.Name == "Dispose");
				var conversions = CSharpConversions.Get(_compilation);
				var disposableConversion = conversions.ImplicitConversion(enumerator.Type, systemIDisposable);
				if (disposableConversion.IsValid) {
					// If the enumerator is implicitly convertible to IDisposable, we should dispose it.
					var compileResult = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new ConversionResolveResult(systemIDisposable, new LocalResolveResult(enumerator), disposableConversion), disposeMethod, new ResolveResult[0]), false);
					if (compileResult.AdditionalStatements.Count != 0)
						_errorReporter.Error("Call to IDisposable.Dispose must not return additional statements.");
					disposer = new JsExpressionStatement(compileResult.Expression);
				}
				else if (enumerator.Type.GetDefinition().IsSealed) {
					// If the enumerator is sealed and not implicitly convertible to IDisposable, we need not dispose it.
					disposer = null;
				}
				else {
					// We don't know whether the enumerator is convertible to IDisposable, so we need to conditionally dispose it.
					var test = _expressionCompiler.Compile(new TypeIsResolveResult(new LocalResolveResult(enumerator), systemIDisposable, _compilation.FindType(KnownTypeCode.Boolean)), true);
					if (test.AdditionalStatements.Count > 0)
						_errorReporter.Error("\"is\" test must not return additional statements.");
					var innerStatements = _expressionCompiler.Compile(new CSharpInvocationResolveResult(new ConversionResolveResult(systemIDisposable, new LocalResolveResult(enumerator), conversions.ExplicitConversion(enumerator.Type, systemIDisposable)), disposeMethod, new ResolveResult[0]), false);
					disposer = new JsIfStatement(test.Expression, new JsBlockStatement(innerStatements.AdditionalStatements.Concat(new[] { new JsExpressionStatement(innerStatements.Expression) })), null);
				}
			}

			JsStatement stmt = new JsWhileStatement(moveNextInvocation.Expression, body);
			if (disposer != null)
				stmt = new JsTryCatchFinallyStatement(stmt, null, disposer);

			_result.Add(stmt);
		}

		private JsBlockStatement GenerateUsingBlock(LocalResolveResult resource, Expression aquisitionExpression, JsBlockStatement body) {
			var boolType = _compilation.FindType(KnownTypeCode.Boolean);
			var systemIDisposable = _compilation.FindType(KnownTypeCode.IDisposable);
			var disposeMethod = systemIDisposable.GetMethods().Single(m => m.Name == "Dispose");
			var conversions = CSharpConversions.Get(_compilation);

			var compiledAquisition = CompileExpression(aquisitionExpression, true);

			var stmts = new List<JsStatement>();
			stmts.AddRange(compiledAquisition.AdditionalStatements);
			stmts.Add(new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[resource.Variable].Name, compiledAquisition.Expression)));

			bool isDynamic = resource.Type.Kind == TypeKind.Dynamic;

			if (isDynamic) {
				var newResource = CreateTemporaryVariable(systemIDisposable);
				var castExpr = _expressionCompiler.Compile(new ConversionResolveResult(systemIDisposable, resource, conversions.ExplicitConversion(resource, systemIDisposable)), true);
				stmts.AddRange(castExpr.AdditionalStatements);
				stmts.Add(new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[newResource].Name, castExpr.Expression)));
				resource = new LocalResolveResult(newResource);
			}

			var compiledDisposeCall = _expressionCompiler.Compile(
			                              new CSharpInvocationResolveResult(
			                                  new ConversionResolveResult(systemIDisposable, resource, conversions.ImplicitConversion(resource, systemIDisposable)),
			                                  disposeMethod,
			                                  new ResolveResult[0]
			                              ), false);
			if (compiledDisposeCall.AdditionalStatements.Count > 0)
				_errorReporter.Error("Type test cannot return additional statements.");

			JsStatement releaseStmt;
			if (isDynamic) {
				releaseStmt = new JsExpressionStatement(compiledDisposeCall.Expression);
			}
			else {
				// if (d != null) ((IDisposable)d).Dispose()
				var compiledTest = _expressionCompiler.Compile(new OperatorResolveResult(boolType, ExpressionType.NotEqual, resource, new ConstantResolveResult(resource.Type, null)), true);
				if (compiledTest.AdditionalStatements.Count > 0)
					_errorReporter.Error("Null test cannot return additional statements.");
				releaseStmt = new JsIfStatement(compiledTest.Expression, new JsExpressionStatement(compiledDisposeCall.Expression), null);
			}

			stmts.Add(new JsTryCatchFinallyStatement(body, null, releaseStmt));

			return new JsBlockStatement(stmts);
		}

		public override void VisitUsingStatement(UsingStatement usingStatement) {
			var stmt = CreateInnerCompiler().Compile(usingStatement.EmbeddedStatement);

			var vds = usingStatement.ResourceAcquisition as VariableDeclarationStatement;
			if (vds != null) {
				foreach (var resource in vds.Variables.Reverse()) {
					stmt = GenerateUsingBlock(((LocalResolveResult)_resolver.Resolve(resource)), resource.Initializer, stmt);
				}
			}
			else {
				var resource = CreateTemporaryVariable(ResolveWithConversion((Expression)usingStatement.ResourceAcquisition).Type);
				stmt = GenerateUsingBlock(new LocalResolveResult(resource), (Expression)usingStatement.ResourceAcquisition, stmt);
			}

			_result.Add(stmt);
		}

		private void RemoveCatchClausesAfterExceptionType(List<CatchClause> catchClauses, IType exceptionType) {
			for (int i = 0; i < catchClauses.Count; i++) {
				var type = _resolver.Resolve(catchClauses[i].Type).Type;
				if (type == exceptionType) {
					catchClauses.RemoveRange(i + 1, catchClauses.Count - i - 1);
					return;
				}
			}
		}

		private JsBlockStatement CompileCatchClause(LocalResolveResult catchVariable, CatchClause catchClause, bool isCatchAll, bool isOnly) {
			JsStatement variableDeclaration = null;
			if (!catchClause.VariableNameToken.IsNull) {
				JsExpression compiledAssignment;
				if (isCatchAll)	// If this is the only handler we need to construct the exception
					compiledAssignment = isOnly ? _runtimeLibrary.MakeException(JsExpression.Identifier(_variables[catchVariable.Variable].Name)) : JsExpression.Identifier(_variables[catchVariable.Variable].Name);
				else
					compiledAssignment = _runtimeLibrary.Downcast(JsExpression.Identifier(_variables[catchVariable.Variable].Name), _compilation.FindType(KnownTypeCode.Exception), _resolver.Resolve(catchClause.Type).Type);

				variableDeclaration = new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[((LocalResolveResult)_resolver.Resolve(catchClause.VariableNameToken)).Variable].Name, compiledAssignment));
			}

			var result = CreateInnerCompiler().Compile(catchClause.Body);
			if (variableDeclaration != null)
				result = new JsBlockStatement(new[] { variableDeclaration }.Concat(result.Statements));
			return result;
		}

		public override void VisitTryCatchStatement(TryCatchStatement tryCatchStatement) {
			var tryBlock = CreateInnerCompiler().Compile(tryCatchStatement.TryBlock);
			JsCatchClause catchClause = null;
			if (tryCatchStatement.CatchClauses.Count > 0) {
				var oldVariableForRethrow = _currentVariableForRethrow;

				_currentVariableForRethrow = CreateTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
				string catchVariableName = _variables[_currentVariableForRethrow].Name;

				var catchClauses = tryCatchStatement.CatchClauses.ToList();
				var systemException = _compilation.FindType(KnownTypeCode.Exception);
				RemoveCatchClausesAfterExceptionType(catchClauses, systemException);

				bool lastIsCatchall = (catchClauses[catchClauses.Count - 1].Type.IsNull || _resolver.Resolve(catchClauses[catchClauses.Count - 1].Type).Type == systemException);
				JsStatement current = lastIsCatchall
					                ? CompileCatchClause(new LocalResolveResult(_currentVariableForRethrow), catchClauses[catchClauses.Count - 1], true, catchClauses.Count == 1)
					                : new JsBlockStatement(new JsThrowStatement(JsExpression.Identifier(catchVariableName)));

				for (int i = catchClauses.Count - (lastIsCatchall ? 2 : 1); i >= 0; i--) {
					var test = _runtimeLibrary.TypeIs(JsExpression.Identifier(catchVariableName), _resolver.Resolve(catchClauses[i].Type).Type);
					current = new JsIfStatement(test, CompileCatchClause(new LocalResolveResult(_currentVariableForRethrow), catchClauses[i], false, catchClauses.Count == 1), current);
				}

				if (!lastIsCatchall || catchClauses.Count > 1) {
					// We need to wrap the exception.
					current = new JsBlockStatement(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(catchVariableName), _runtimeLibrary.MakeException(JsExpression.Identifier(catchVariableName)))), current);
				}

				catchClause = new JsCatchClause(catchVariableName, current);
				_currentVariableForRethrow = oldVariableForRethrow;
			}

			var finallyBlock = (!tryCatchStatement.FinallyBlock.IsNull ? CreateInnerCompiler().Compile(tryCatchStatement.FinallyBlock) : null);

			_result.Add(new JsTryCatchFinallyStatement(tryBlock, catchClause, finallyBlock));
		}

		public override void VisitThrowStatement(ThrowStatement throwStatement) {
			if (throwStatement.Expression.IsNull) {
				_result.Add(new JsThrowStatement(JsExpression.Identifier(_variables[_currentVariableForRethrow].Name)));
			}
			else {
				var compiledExpr = CompileExpression(throwStatement.Expression, true);
				_result.AddRange(compiledExpr.AdditionalStatements);
				_result.Add(new JsThrowStatement(compiledExpr.Expression));
			}
		}

		public override void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) {
			_result.Add(new JsYieldBreakStatement());
		}

		public override void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) {
			var compiledExpr = CompileExpression(yieldReturnStatement.Expression, true);
			_result.AddRange(compiledExpr.AdditionalStatements);
			_result.Add(new JsYieldReturnStatement(compiledExpr.Expression));
		}

		public override void VisitGotoStatement(GotoStatement gotoStatement) {
			_result.Add(new JsGotoStatement(gotoStatement.Label));
		}

		public override void VisitLabelStatement(LabelStatement labelStatement) {
			_result.Add(new JsLabelStatement(labelStatement.Label));
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
			var compiledExpr = CompileExpression(switchStatement.Expression, true);
			_result.AddRange(compiledExpr.AdditionalStatements);

			var oldGotoCaseMap = _currentGotoCaseMap;

			var gotoCaseData = new GatherGotoCaseAndDefaultDataVisitor(_resolver, _nextLabelIndex).Process(switchStatement);
			_currentGotoCaseMap = gotoCaseData.Item2;

			var caseClauses = new List<JsSwitchSection>();
			foreach (var section in switchStatement.SwitchSections) {
				var values = new List<JsExpression>();
				foreach (var v in section.CaseLabels) {
					if (v.Expression.IsNull) {
						values.Add(null);	// Default
					}
					else {
						var rr = _resolver.Resolve(v.Expression);
						if (rr.ConstantValue == null) {
							values.Add(JsExpression.Null);
						}
						else if (rr.ConstantValue is string) {
							values.Add(JsExpression.String((string)rr.ConstantValue));
						}
						else {
							values.Add(JsExpression.Number((int)Convert.ChangeType(rr.ConstantValue, typeof(int))));
						}
					}
				}

				var statements = section.Statements.SelectMany(stmt => CreateInnerCompiler().Compile(stmt).Statements);

				if (gotoCaseData.Item1.ContainsKey(section))
					statements = new[] { new JsLabelStatement(gotoCaseData.Item1[section]) }.Concat(statements);

				caseClauses.Add(new JsSwitchSection(values, new JsBlockStatement(statements)));
			}

			_result.Add(new JsSwitchStatement(compiledExpr.Expression, caseClauses));
			_currentGotoCaseMap = oldGotoCaseMap;
		}

		public override void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) {
			var value = _resolver.Resolve(gotoCaseStatement.LabelExpression).ConstantValue;
			_result.Add(new JsGotoStatement(_currentGotoCaseMap[NormalizeSwitchLabelValue(value)]));
		}

		public override void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) {
			_result.Add(new JsGotoStatement(_currentGotoCaseMap[_gotoCaseMapDefaultKey]));
		}
	}
}
