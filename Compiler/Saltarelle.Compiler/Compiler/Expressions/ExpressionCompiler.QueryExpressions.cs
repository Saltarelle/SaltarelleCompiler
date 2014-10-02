using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		internal interface IQueryContextVisitor<TData, TResult> {
			TResult VisitRange(RangeQueryContext c, TData data);
			TResult VisitTransparentType(TransparentTypeQueryContext c, TData data);
		}

		internal abstract class QueryContext {
			public readonly string Name;

			protected QueryContext(string name) {
				Name = name;
			}

			public QueryContext WrapInTransparentType(string newName, ITypeSymbol transparentType, ITypeSymbol oldType, IRangeVariableSymbol variableToAdd, ITypeSymbol variableToAddType, string variableToAddName) {
				return new TransparentTypeQueryContext(newName, transparentType, this, oldType, new RangeQueryContext(variableToAdd, variableToAddName), variableToAddType);
			}

			public abstract TResult Accept<TData, TResult>(IQueryContextVisitor<TData, TResult> visitor, TData data);
		}

		internal class RangeQueryContext : QueryContext {
			public readonly IRangeVariableSymbol Variable;

			public RangeQueryContext(IRangeVariableSymbol variable, string name) : base(name) {
				Variable = variable;
			}

			public override TResult Accept<TData, TResult>(IQueryContextVisitor<TData, TResult> visitor, TData data) {
				return visitor.VisitRange(this, data);
			}
		}

		internal class TransparentTypeQueryContext : QueryContext {
			public readonly ITypeSymbol TransparentType;
			public readonly QueryContext Left;
			public readonly ITypeSymbol LeftType;
			public readonly QueryContext Right;
			public readonly ITypeSymbol RightType;

			public TransparentTypeQueryContext(string name, ITypeSymbol transparentType, QueryContext left, ITypeSymbol leftType, QueryContext right, ITypeSymbol rightType) : base(name) {
				TransparentType = transparentType;
				Left = left;
				LeftType = leftType;
				Right = right;
				RightType = rightType;
			}

			public override TResult Accept<TData, TResult>(IQueryContextVisitor<TData, TResult> visitor, TData data) {
				return visitor.VisitTransparentType(this, data);
			}
		}

		private class RangeVariableSubstitutionMerger : IQueryContextVisitor<JsExpression, int> {
			private ImmutableDictionary<IRangeVariableSymbol, JsExpression> _substitutions;

			private RangeVariableSubstitutionMerger(ImmutableDictionary<IRangeVariableSymbol, JsExpression> substitutions) {
				_substitutions = substitutions;
			}

			public int VisitRange(RangeQueryContext c, JsExpression data) {
				_substitutions = _substitutions.SetItem(c.Variable, data != null ? JsExpression.Member(data, c.Name) : JsExpression.Identifier(c.Name));
				return 0;
			}

			public int VisitTransparentType(TransparentTypeQueryContext c, JsExpression data) {
				var current = data != null ? JsExpression.Member(data, c.Name) : JsExpression.Identifier(c.Name);
				c.Left.Accept(this, current);
				c.Right.Accept(this, current);
				return 0;
			}

			public static ImmutableDictionary<IRangeVariableSymbol, JsExpression> Process(ImmutableDictionary<IRangeVariableSymbol, JsExpression> old, QueryContext c) {
				var obj = new RangeVariableSubstitutionMerger(old);
				c.Accept(obj, null);
				return obj._substitutions;
			}
		}

		private class TransparentTypeCacher : IQueryContextVisitor<int, int> {
			private ExpressionCompiler _expressionCompiler;

			public TransparentTypeCacher(ExpressionCompiler expressionCompiler) {
				_expressionCompiler = expressionCompiler;
			}

			public int VisitRange(RangeQueryContext c, int data) {
				return 0;
			}

			public int VisitTransparentType(TransparentTypeQueryContext c, int data) {
				c.Left.Accept(this, data);
				c.Right.Accept(this, data);

				if (!_expressionCompiler._transparentTypeCache.ContainsKey(c.TransparentType)) {
					_expressionCompiler.InstantiateTransparentType(c.TransparentType, new[] { Tuple.Create(c.Left is TransparentTypeQueryContext ? _expressionCompiler._transparentTypeCache[c.LeftType] : _expressionCompiler.InstantiateTypeForExpressionTree(c.LeftType), c.Left.Name), Tuple.Create(c.Right is TransparentTypeQueryContext ? _expressionCompiler._transparentTypeCache[c.RightType] : _expressionCompiler.InstantiateTypeForExpressionTree(c.RightType), c.Right.Name) });
				}
				return 0;
			}

			public static void Process(ExpressionCompiler compiler, QueryContext context) {
				var obj = new TransparentTypeCacher(compiler);
				context.Accept(obj, 0);
			}
		}

		internal struct QueryExpressionCompilationInfo {
			public readonly JsExpression Result;
			public readonly ITypeSymbol CurrentType;
			public readonly QueryContext CurrentContext;

			public QueryExpressionCompilationInfo(JsExpression result, ITypeSymbol currentType, QueryContext currentContext) {
				Result = result;
				CurrentType = currentType;
				CurrentContext = currentContext;
			}
		}

		private JsExpression GetExpressionTypeForQueryContext(QueryContext context, ITypeSymbol typeIfRange) {
			return context is TransparentTypeQueryContext ? _transparentTypeCache[((TransparentTypeQueryContext)context).TransparentType] : InstantiateTypeForExpressionTree(typeIfRange);
		}

		private JsExpression HandleQueryExpression(QueryExpressionSyntax query) {
			var current = HandleFirstFromClause(query.FromClause);
			return HandleQueryBody(query.Body, current).Item2;
		}

		private JsExpression CompileQueryLambda(INamedTypeSymbol delegateType, ExpressionSyntax expression, QueryExpressionCompilationInfo info, Tuple<IRangeVariableSymbol, ITypeSymbol, string> additionalParameter, Func<JsExpression, JsExpression> returnValueFactory, Func<JsExpression, JsExpression> returnValueFactoryExpression) {
			if (delegateType.IsExpressionOfT()) {
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
				var result = CreateExpressionTreeBuilder().BuildQueryExpressionTree(info.CurrentContext, GetExpressionTypeForQueryContext(info.CurrentContext, delegateType.DelegateInvokeMethod.Parameters[0].Type), additionalParameter != null ? Tuple.Create(additionalParameter.Item1, InstantiateTypeForExpressionTree(additionalParameter.Item2), additionalParameter.Item3) : null, expression, returnValueFactoryExpression);
				_additionalStatements.AddRange(result.AdditionalStatements);
				return result.Expression;
			}
			else {
				var oldSubstitutions = _activeRangeVariableSubstitutions;
				try {
					_activeRangeVariableSubstitutions = RangeVariableSubstitutionMerger.Process(_activeRangeVariableSubstitutions, info.CurrentContext);

					return BindToCaptureObject(expression, delegateType, newContext => {
						var jsBody = CloneAndCompile(expression, true, nestedFunctionContext: newContext);
						var result = returnValueFactory(jsBody.Expression);
						return JsExpression.FunctionDefinition(additionalParameter != null ? new[] { info.CurrentContext.Name, additionalParameter.Item3 } : new[] { info.CurrentContext.Name }, JsStatement.Block(jsBody.AdditionalStatements.Concat(new[] { JsStatement.Return(result) })));
					});
				}
				finally {
					_activeRangeVariableSubstitutions = oldSubstitutions;
				}
			}
		}

		private JsExpression CompileQueryMethodInvocation(IMethodSymbol method, SyntaxNode node, ITypeSymbol targetType, JsExpression target, params JsExpression[] args) {
			if (method.Parameters.Length > args.Length) {
				var impl = _metadataImporter.GetMethodSemantics(method.UnReduceIfExtensionMethod().OriginalDefinition);

				var newArgs = new JsExpression[Math.Min(method.Parameters.Length, Math.Max(args.Length, (impl.OmitUnspecifiedArgumentsFrom ?? int.MaxValue) - (method.ReducedFrom != null ? 1 : 0)))];
				for (int i = 0; i < args.Length; i++)
					newArgs[i] = args[i];
				for (int i = args.Length; i < newArgs.Length; i++) {
					newArgs[i] = JSModel.Utils.MakeConstantExpression(_semanticModel.GetDefaultValueInInvocation(method.Parameters[i], node));
				}

				args = newArgs;
			}

			if (method.ContainingType.TypeKind == TypeKind.Delegate && method.Name == "Invoke") {
				_errorReporter.Message(Messages._7998, "delegate invocation in query pattern");
				return JsExpression.Null;
			}
			else if (method.ReducedFrom != null) {
				method = method.UnReduceIfExtensionMethod();
				var impl = _metadataImporter.GetMethodSemantics(method.OriginalDefinition);
				if (targetType != null)
					target = PerformConversion(target, _semanticModel.Compilation.ClassifyConversion(targetType, method.Parameters[0].Type), targetType, method.Parameters[0].Type, null);
				return CompileMethodInvocation(impl, method, new[] { InstantiateType(method.ContainingType), target }.Concat(args).ToList(), false);
			}
			else {
				var impl = _metadataImporter.GetMethodSemantics(method.OriginalDefinition);
				if (targetType != null)
					target = PerformConversion(target, _semanticModel.Compilation.ClassifyConversion(targetType, method.ContainingType), targetType, method.ContainingType, null);
				return CompileMethodInvocation(impl, method, new[] { target }.Concat(args).ToList(), false);
			}
		}

		private QueryExpressionCompilationInfo AddMemberToTransparentType(IRangeVariableSymbol symbol, INamedTypeSymbol delegateType, ITypeSymbol callReturnType, ExpressionSyntax value, QueryExpressionCompilationInfo info) {
			var parameter = _createTemporaryVariable();
			if (delegateType.IsExpressionOfT()) {
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
				var oldType = delegateType.DelegateInvokeMethod.Parameters[0].Type;
				var newType = delegateType.DelegateInvokeMethod.ReturnType;
				var newContext = info.CurrentContext.WrapInTransparentType(_variables[parameter].Name, newType, oldType, symbol, _semanticModel.GetTypeInfo(value).ConvertedType, _variables[symbol].Name);
				TransparentTypeCacher.Process(this, newContext);

				var result = CreateExpressionTreeBuilder().AddMemberToTransparentType(info.CurrentContext, _variables[symbol].Name, value, GetExpressionTypeForQueryContext(info.CurrentContext, oldType), _transparentTypeCache[newType]);
				_additionalStatements.AddRange(result.AdditionalStatements);
				var jsValue = result.Expression;
				return new QueryExpressionCompilationInfo(jsValue, callReturnType, newContext);
			}
			else {
				var oldType = delegateType.DelegateInvokeMethod.Parameters[0].Type;
				var newType = delegateType.DelegateInvokeMethod.ReturnType;
				var jsValue = CompileQueryLambda(delegateType, value, info, null, v => JsExpression.ObjectLiteral(new JsObjectLiteralProperty(info.CurrentContext.Name, JsExpression.Identifier(info.CurrentContext.Name)), new JsObjectLiteralProperty(_variables[symbol].Name, v)), x => x);
				var newContext = info.CurrentContext.WrapInTransparentType(_variables[parameter].Name, newType, oldType, symbol, _semanticModel.GetTypeInfo(value).ConvertedType, _variables[symbol].Name);
				return new QueryExpressionCompilationInfo(jsValue, callReturnType, newContext);
			}
		}

		private QueryExpressionCompilationInfo AddMemberToTransparentType(IRangeVariableSymbol symbol, INamedTypeSymbol delegateType, ITypeSymbol callReturnType, QueryExpressionCompilationInfo info) {
			var parameter = _createTemporaryVariable();

			if (delegateType.IsExpressionOfT()) {
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
				var oldType = delegateType.DelegateInvokeMethod.Parameters[0].Type;
				var newType = delegateType.DelegateInvokeMethod.Parameters[1].Type;
				var newTransparentType = delegateType.DelegateInvokeMethod.ReturnType;
				var newContext = info.CurrentContext.WrapInTransparentType(_variables[parameter].Name, newTransparentType, oldType, symbol, newType, _variables[symbol].Name);
				TransparentTypeCacher.Process(this, newContext);
				var result = CreateExpressionTreeBuilder().AddMemberToTransparentType(info.CurrentContext, _variables[symbol].Name, InstantiateTypeForExpressionTree(newType), GetExpressionTypeForQueryContext(info.CurrentContext, oldType), _transparentTypeCache[newTransparentType]);
				_additionalStatements.AddRange(result.AdditionalStatements);
				return new QueryExpressionCompilationInfo(result.Expression, callReturnType, newContext);
			}
			else {
				var name = _variables[symbol].Name;
				JsExpression jsValue = JsExpression.FunctionDefinition(new[] { info.CurrentContext.Name, name }, JsStatement.Return(JsExpression.ObjectLiteral(new JsObjectLiteralProperty(info.CurrentContext.Name, JsExpression.Identifier(info.CurrentContext.Name)), new JsObjectLiteralProperty(name, JsExpression.Identifier(name)))));
				var delegateSemantics = _metadataImporter.GetDelegateSemantics(delegateType.OriginalDefinition);
				if (delegateSemantics.BindThisToFirstParameter)
					jsValue = _runtimeLibrary.BindFirstParameterToThis(jsValue, this);

				var newContext = info.CurrentContext.WrapInTransparentType(_variables[parameter].Name, delegateType.DelegateInvokeMethod.ReturnType, delegateType.DelegateInvokeMethod.Parameters[0].Type, symbol, delegateType.DelegateInvokeMethod.Parameters[1].Type, _variables[symbol].Name);
				return new QueryExpressionCompilationInfo(jsValue, callReturnType, newContext);
			}
		}

		private QueryExpressionCompilationInfo HandleFirstFromClause(FromClauseSyntax node) {
			var result = InnerCompile(node.Expression, false);
			var info = _semanticModel.GetQueryClauseInfo(node);
			var type = _semanticModel.GetTypeInfo(node.Expression).ConvertedType;
			if (info.CastInfo.Symbol != null) {
				result = CompileQueryMethodInvocation((IMethodSymbol)info.CastInfo.Symbol, node, type, result);
				type = ((IMethodSymbol)info.CastInfo.Symbol).ReturnType;
			} 

			var rv = _semanticModel.GetDeclaredSymbol(node);
			return new QueryExpressionCompilationInfo(result, type, new RangeQueryContext(rv, _variables[rv].Name));
		}

		private Tuple<ITypeSymbol, JsExpression> HandleQueryBody(QueryBodySyntax body, QueryExpressionCompilationInfo current) {
			for (int i = 0; i < body.Clauses.Count; i++) {
				var clause = body.Clauses[i];

				if (clause is LetClauseSyntax) {
					current = HandleLetClause((LetClauseSyntax)clause, current);
				}
				else if (clause is FromClauseSyntax) {
					current = HandleAdditionalFromClause((FromClauseSyntax)clause, i == body.Clauses.Count - 1 ? body.SelectOrGroup as SelectClauseSyntax : null, current);
				}
				else if (clause is JoinClauseSyntax) {
					current = HandleJoinClause((JoinClauseSyntax)clause, i == body.Clauses.Count - 1 ? body.SelectOrGroup as SelectClauseSyntax : null, current);
				}
				else if (clause is WhereClauseSyntax) {
					current = HandleWhereClause((WhereClauseSyntax)clause, current);
				}
				else if (clause is OrderByClauseSyntax) {
					current = HandleOrderByClause((OrderByClauseSyntax)clause, current);
				}
				else {
					_errorReporter.InternalError("Invalid query clause " + clause);
					return Tuple.Create((ITypeSymbol)_semanticModel.Compilation.GetSpecialType(SpecialType.System_Object), (JsExpression)JsExpression.Null);
				}
			}
			var result = HandleSelectOrGroupClause(body.SelectOrGroup, current);

			if (body.Continuation != null) {
				var continuationVariable = _semanticModel.GetDeclaredSymbol(body.Continuation);
				result = HandleQueryBody(body.Continuation.Body, new QueryExpressionCompilationInfo(result.Item2, result.Item1, new RangeQueryContext(continuationVariable, _variables[continuationVariable].Name)));
			}

			return result;
		}

		private QueryExpressionCompilationInfo HandleLetClause(LetClauseSyntax clause, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var newInfo = AddMemberToTransparentType(_semanticModel.GetDeclaredSymbol(clause), (INamedTypeSymbol)method.Parameters[0].Type, method.ReturnType, clause.Expression, current);
			return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, newInfo.Result), method.ReturnType, newInfo.CurrentContext);
		}

		private QueryExpressionCompilationInfo HandleAdditionalFromClause(FromClauseSyntax clause, SelectClauseSyntax followingSelect, QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var innerSelection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, clause.Expression, current, null, x => clauseInfo.CastInfo.Symbol != null ? CompileQueryMethodInvocation((IMethodSymbol)clauseInfo.CastInfo.Symbol, clause, _semanticModel.GetTypeInfo(clause.Expression).ConvertedType, x) : x, x => clauseInfo.CastInfo.Symbol != null ? CreateExpressionTreeBuilder().Call((IMethodSymbol)clauseInfo.CastInfo.Symbol, x, new JsExpression[0]) : x);

			var lastDelegateType = (INamedTypeSymbol)method.Parameters[1].Type;
			if (followingSelect != null) {
				var rv = (IRangeVariableSymbol)_semanticModel.GetDeclaredSymbol(clause);
				var projection = CompileQueryLambda(lastDelegateType, followingSelect.Expression, current, Tuple.Create(rv, ((INamedTypeSymbol)lastDelegateType.UnpackExpression()).DelegateInvokeMethod.Parameters[1].Type, _variables[rv].Name), x => x, x => x);
				return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, innerSelection, projection), method.ReturnType, null);
			}
			else {
				var newInfo = AddMemberToTransparentType(_semanticModel.GetDeclaredSymbol(clause), lastDelegateType, method.ReturnType, current);
				return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, innerSelection, newInfo.Result), method.ReturnType, newInfo.CurrentContext);
			}
		}

		private QueryExpressionCompilationInfo HandleJoinClause(JoinClauseSyntax clause, SelectClauseSyntax followingSelect, QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var newVariable = _semanticModel.GetDeclaredSymbol(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var other = CloneAndCompile(clause.InExpression, true);
			JsExpression otherExpr;
			if (other.AdditionalStatements.Count == 0) {
				otherExpr = other.Expression;
			}
			else {
				// This is code I don't like particularly well, but the alternative that would also ensure that expressions are evaluated in the correct order would be terribly complex, so we'll live with it.
				otherExpr = JsExpression.Invocation(BindToCaptureObject(clause.InExpression, (INamedTypeSymbol)method.Parameters[0].Type, n => JsExpression.FunctionDefinition(new string[0], JsStatement.Block(CloneAndCompile(clause.InExpression, true, n).GetStatementsWithReturn()))));
			}

			if (clauseInfo.CastInfo.Symbol != null) {
				otherExpr = CompileQueryMethodInvocation((IMethodSymbol)clauseInfo.CastInfo.Symbol, clause, null, otherExpr);
			}
			var leftSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, clause.LeftExpression, current, null, x => x, x => x);
			var rightSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[2].Type, clause.RightExpression, new QueryExpressionCompilationInfo(JsExpression.Null, _semanticModel.GetTypeInfo(clause.RightExpression).ConvertedType, new RangeQueryContext(newVariable, _variables[newVariable].Name)), null, x => x, x => x);

			var secondArgToProjector = (clause.Into != null ? _semanticModel.GetDeclaredSymbol(clause.Into) : newVariable);

			if (followingSelect != null) {
				var projection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[3].Type, followingSelect.Expression, current, Tuple.Create(secondArgToProjector, ((INamedTypeSymbol)method.Parameters[3].Type.UnpackExpression()).DelegateInvokeMethod.Parameters[1].Type, _variables[secondArgToProjector].Name), x => x, x => x);
				return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, followingSelect, current.CurrentType, current.Result, otherExpr, leftSelector, rightSelector, projection), method.ReturnType, null);
			}
			else {
				var newInfo = AddMemberToTransparentType(secondArgToProjector, (INamedTypeSymbol)method.Parameters[3].Type, method.ReturnType, current);
				return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, otherExpr, leftSelector, rightSelector, newInfo.Result), method.ReturnType, newInfo.CurrentContext);
			}
		}

		private QueryExpressionCompilationInfo HandleWhereClause(WhereClauseSyntax clause, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, clause.Condition, current, null, x => x, x => x);
			return new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, lambda), method.ReturnType, current.CurrentContext);
		}

		private QueryExpressionCompilationInfo HandleOrderByClause(OrderByClauseSyntax clause, QueryExpressionCompilationInfo current) {
			foreach (var ordering in clause.Orderings) {
				var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(ordering).Symbol;
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, ordering.Expression, current, null, x => x, x => x);
				current = new QueryExpressionCompilationInfo(CompileQueryMethodInvocation(method, clause, current.CurrentType, current.Result, lambda), method.ReturnType, current.CurrentContext);
			}
			return current;
		}

		private Tuple<ITypeSymbol, JsExpression> HandleSelectOrGroupClause(SelectOrGroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			if (node is SelectClauseSyntax) {
				return HandleSelectClause((SelectClauseSyntax)node, current);
			}
			else if (node is GroupClauseSyntax) {
				return HandleGroupClause((GroupClauseSyntax)node, current);
			}
			else {
				_errorReporter.InternalError("Invalid node " + node);
				return Tuple.Create((ITypeSymbol)_semanticModel.Compilation.GetSpecialType(SpecialType.System_Object), (JsExpression)JsExpression.Null);
			}
		}

		private Tuple<ITypeSymbol, JsExpression> HandleSelectClause(SelectClauseSyntax node, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			if (method != null) {
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.Expression, current, null, x => x, x => x);
				return Tuple.Create(method.ReturnType, CompileQueryMethodInvocation(method, node, current.CurrentType, current.Result, lambda));
			}
			else {
				return Tuple.Create(current.CurrentType, current.Result);
			}
		}

		private Tuple<ITypeSymbol, JsExpression> HandleGroupClause(GroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			var grouping = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.ByExpression, current, null, x => x, x => x);

			switch (method.Parameters.Length) {
				case 1: {
					return Tuple.Create(method.ReturnType, CompileQueryMethodInvocation(method, node, current.CurrentType, current.Result, grouping));
				}

				case 2: {
					var selector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, node.GroupExpression, current, null, x => x, x => x);
					return Tuple.Create(method.ReturnType, CompileQueryMethodInvocation(method, node, current.CurrentType, current.Result, grouping, selector));
				}

				default: {
					_errorReporter.InternalError("Invalid GroupBy call");
					return Tuple.Create((ITypeSymbol)_semanticModel.Compilation.GetSpecialType(SpecialType.System_Object), (JsExpression)JsExpression.Null);
				}
			}
		}

		private JsExpression CompileRangeVariableAccess(IRangeVariableSymbol symbol) {
			JsExpression result;
			if (_activeRangeVariableSubstitutions == null || !_activeRangeVariableSubstitutions.TryGetValue(symbol, out result))
				result = JsExpression.Identifier(_variables[symbol].Name);
			return result;
		}
	}
}
