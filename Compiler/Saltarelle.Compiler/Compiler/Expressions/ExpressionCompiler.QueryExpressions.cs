using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private Dictionary<IRangeVariableSymbol, JsExpression> _activeRangeVariableSubstitutions;

		private struct QueryExpressionCompilationInfo {
			public readonly string ParameterName;
			public readonly JsExpression Result;
			public readonly ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>> CurrentTransparentType;

			public QueryExpressionCompilationInfo(string parameterName, JsExpression result, ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>> currentTransparentType) {
				ParameterName = parameterName;
				Result = result;
				CurrentTransparentType = currentTransparentType;
			}
		}

		private JsExpression HandleQueryExpression(QueryExpressionSyntax query) {
			var current = HandleFirstFromClause(query.FromClause);
			return HandleQueryBody(query.Body, current);
		}

		private JsExpression CompileQueryLambda(INamedTypeSymbol delegateType, ExpressionSyntax expression, QueryExpressionCompilationInfo info, IEnumerable<string> additionalParameters, Func<JsExpression, JsExpression> returnValueFactory) {
			return BindToCaptureObject(expression, delegateType, newContext => {
				var oldSubstitutions = _activeRangeVariableSubstitutions;
				try {
					_activeRangeVariableSubstitutions = new Dictionary<IRangeVariableSymbol, JsExpression>();
					foreach (var x in info.CurrentTransparentType)
						_activeRangeVariableSubstitutions[x.Item1] = x.Item2.Aggregate((JsExpression)JsExpression.Identifier(info.ParameterName), JsExpression.Member);

					var jsBody = CloneAndCompile(expression, true, nestedFunctionContext: newContext);
					var result = returnValueFactory(jsBody.Expression);
					return JsExpression.FunctionDefinition(new[] { info.ParameterName }.Concat(additionalParameters), JsStatement.Block(jsBody.AdditionalStatements.Concat(new[] { JsStatement.Return(result) })));
				}
				finally {
					_activeRangeVariableSubstitutions = oldSubstitutions;
				}
			});
		}

		private JsExpression CompileQueryMethodInvocation(IMethodSymbol method, JsExpression target, params JsExpression[] args) {
			if (method.ReducedFrom != null) {
				var unreduced = method.UnReduceIfExtensionMethod();
				var impl = _metadataImporter.GetMethodSemantics(unreduced.OriginalDefinition);
				return CompileMethodInvocation(impl, unreduced, new[] { _runtimeLibrary.InstantiateType(unreduced.ContainingType, this), target }.Concat(args).ToList(), false);
			}
			else {
				var impl = _metadataImporter.GetMethodSemantics(method.OriginalDefinition);
				return CompileMethodInvocation(impl, method, new[] { target }.Concat(args).ToList(), false);
			}
		}

		private QueryExpressionCompilationInfo AddMemberToTransparentType(IRangeVariableSymbol symbol, INamedTypeSymbol delegateType, ExpressionSyntax value, QueryExpressionCompilationInfo info) {
			var jsValue = CompileQueryLambda(delegateType, value, info, new string[0], v => JsExpression.ObjectLiteral(new JsObjectLiteralProperty(info.ParameterName, JsExpression.Identifier(info.ParameterName)), new JsObjectLiteralProperty(_variables[symbol].Name, v)));

			var parameter = _createTemporaryVariable();
			var parameterName = _variables[parameter].Name;
			var newTransparentType = ImmutableArray.CreateRange(        info.CurrentTransparentType.Select(x => Tuple.Create(x.Item1, ImmutableArray.CreateRange(new[] { info.ParameterName }.Concat(x.Item2))))
			                                                    .Concat(new[] { Tuple.Create(symbol, ImmutableArray.Create(_variables[symbol].Name)) }));

			return new QueryExpressionCompilationInfo(parameterName, jsValue, newTransparentType);
		}

		private QueryExpressionCompilationInfo AddMemberToTransparentType(IRangeVariableSymbol symbol, INamedTypeSymbol delegateType, QueryExpressionCompilationInfo info) {
			var name = _variables[symbol].Name;
			JsExpression jsValue = JsExpression.FunctionDefinition(new[] { info.ParameterName, name }, JsStatement.Return(JsExpression.ObjectLiteral(new JsObjectLiteralProperty(info.ParameterName, JsExpression.Identifier(info.ParameterName)), new JsObjectLiteralProperty(name, JsExpression.Identifier(name)))));
			var delegateSemantics = _metadataImporter.GetDelegateSemantics(delegateType.OriginalDefinition);
			if (delegateSemantics.BindThisToFirstParameter)
				jsValue = _runtimeLibrary.BindFirstParameterToThis(jsValue, this);

			var parameter = _createTemporaryVariable();
			var parameterName = _variables[parameter].Name;
			var newTransparentType = ImmutableArray.CreateRange(        info.CurrentTransparentType.Select(x => Tuple.Create(x.Item1, ImmutableArray.CreateRange(new[] { info.ParameterName }.Concat(x.Item2))))
			                                                    .Concat(new[] { Tuple.Create(symbol, ImmutableArray.Create(_variables[symbol].Name)) }));

			return new QueryExpressionCompilationInfo(parameterName, jsValue, newTransparentType);
		}

		private QueryExpressionCompilationInfo HandleFirstFromClause(FromClauseSyntax node) {
			var result = InnerCompile(node.Expression, true);
			var info = _semanticModel.GetQueryClauseInfo(node);
			if (info.CastInfo.Symbol != null) {
				result = CompileQueryMethodInvocation((IMethodSymbol)info.CastInfo.Symbol, result);
			} 

			var rv = _semanticModel.GetDeclaredSymbol(node);
			return new QueryExpressionCompilationInfo(_variables[rv].Name, result, ImmutableArray.Create(Tuple.Create(rv, ImmutableArray<string>.Empty)));
		}

		private JsExpression HandleQueryBody(QueryBodySyntax body, QueryExpressionCompilationInfo current) {
			bool eatSelect = false;
			for (int i = 0; i < body.Clauses.Count; i++) {
				var clause = body.Clauses[i];

				if (clause is LetClauseSyntax) {
					current = HandleLetClause((LetClauseSyntax)clause, current);
				}
				else if (clause is FromClauseSyntax) {
					var fromClause = (FromClauseSyntax)clause;
					if (i == body.Clauses.Count - 1 && body.SelectOrGroup is SelectClauseSyntax) {
						current = HandleAdditionalFromClause(fromClause, (SelectClauseSyntax)body.SelectOrGroup, current);
						eatSelect = true;
					}
					else {
						current = HandleAdditionalFromClause(fromClause, null, current);
					}
				}
				else if (clause is JoinClauseSyntax) {
					var joinClause = (JoinClauseSyntax)clause;
					if (i == body.Clauses.Count - 1 && body.SelectOrGroup is SelectClauseSyntax) {
						current = HandleJoinClause(joinClause, (SelectClauseSyntax)body.SelectOrGroup, current);
						eatSelect = true;
					}
					else {
						current = HandleJoinClause(joinClause, null, current);
					}
				}
				else if (clause is WhereClauseSyntax) {
					current = HandleWhereClause((WhereClauseSyntax)clause, current);
				}
				else if (clause is OrderByClauseSyntax) {
					current = HandleOrderByClause((OrderByClauseSyntax)clause, current);
				}
				else {
					_errorReporter.InternalError("Invalid query clause " + clause);
					return JsExpression.Null;
				}
			}
			var result = eatSelect ? current.Result : HandleSelectOrGroupClause(body.SelectOrGroup, current);

			if (body.Continuation != null) {
				var continuationVariable = _semanticModel.GetDeclaredSymbol(body.Continuation);
				result = HandleQueryBody(body.Continuation.Body, new QueryExpressionCompilationInfo(_variables[continuationVariable].Name, result, ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>>.Empty));
			}

			return result;
		}

		private QueryExpressionCompilationInfo HandleLetClause(LetClauseSyntax clause, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var newInfo = AddMemberToTransparentType(_semanticModel.GetDeclaredSymbol(clause), (INamedTypeSymbol)method.Parameters[0].Type, clause.Expression, current);
			return new QueryExpressionCompilationInfo(newInfo.ParameterName, CompileQueryMethodInvocation(method, current.Result, newInfo.Result), newInfo.CurrentTransparentType);
		}

		private QueryExpressionCompilationInfo HandleAdditionalFromClause(FromClauseSyntax clause, SelectClauseSyntax followingSelect, QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var innerSelection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, clause.Expression, current, new string[0], x => clauseInfo.CastInfo.Symbol != null ? CompileQueryMethodInvocation((IMethodSymbol)clauseInfo.CastInfo.Symbol, x) : x);

			if (followingSelect != null) {
				var projection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, followingSelect.Expression, current, new[] { _variables[_semanticModel.GetDeclaredSymbol(clause)].Name }, x => x);
				return new QueryExpressionCompilationInfo(null, CompileQueryMethodInvocation(method, current.Result, innerSelection, projection), ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>>.Empty);
			}
			else {
				var newInfo = AddMemberToTransparentType(_semanticModel.GetDeclaredSymbol(clause), (INamedTypeSymbol)method.Parameters[1].Type, current);
				return new QueryExpressionCompilationInfo(newInfo.ParameterName, CompileQueryMethodInvocation(method, current.Result, innerSelection, newInfo.Result), newInfo.CurrentTransparentType);
			}
		}

		private QueryExpressionCompilationInfo HandleJoinClause(JoinClauseSyntax clause, SelectClauseSyntax followingSelect, QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var newVariable = _semanticModel.GetDeclaredSymbol(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var other = CloneAndCompile(clause.InExpression, false);
			var otherExpr = other.AdditionalStatements.Count == 0 ? other.Expression : JsExpression.Invocation(JsExpression.FunctionDefinition(new string[0], JsStatement.Block(other.GetStatementsWithReturn())));
			if (clauseInfo.CastInfo.Symbol != null) {
				otherExpr = CompileQueryMethodInvocation((IMethodSymbol)clauseInfo.CastInfo.Symbol, otherExpr);
			}
			var leftSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, clause.LeftExpression, current, new string[0], x => x);
			var rightSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, clause.RightExpression, new QueryExpressionCompilationInfo(_variables[newVariable].Name, JsExpression.Null, ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>>.Empty), new string[0], x => x);

			var secondArgToProjector = (clause.Into != null ? _semanticModel.GetDeclaredSymbol(clause.Into) : newVariable);

			if (followingSelect != null) {
				var projection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, followingSelect.Expression, current, new[] { _variables[secondArgToProjector].Name }, x => x);
				return new QueryExpressionCompilationInfo(null, CompileQueryMethodInvocation(method, current.Result, otherExpr, leftSelector, rightSelector, projection), ImmutableArray<Tuple<IRangeVariableSymbol, ImmutableArray<string>>>.Empty);
			}
			else {
				var newInfo = AddMemberToTransparentType(secondArgToProjector, (INamedTypeSymbol)method.Parameters[1].Type, current);
				return new QueryExpressionCompilationInfo(newInfo.ParameterName, CompileQueryMethodInvocation(method, current.Result, otherExpr, leftSelector, rightSelector, newInfo.Result), newInfo.CurrentTransparentType);
			}
		}

		private QueryExpressionCompilationInfo HandleWhereClause(WhereClauseSyntax clause, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, clause.Condition, current, new string[0], x => x);
			return new QueryExpressionCompilationInfo(current.ParameterName, CompileQueryMethodInvocation(method, current.Result, lambda), current.CurrentTransparentType);
		}

		private QueryExpressionCompilationInfo HandleOrderByClause(OrderByClauseSyntax clause, QueryExpressionCompilationInfo current) {
			foreach (var ordering in clause.Orderings) {
				var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(ordering).Symbol;
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, ordering.Expression, current, new string[0], x => x);
				current = new QueryExpressionCompilationInfo(current.ParameterName, CompileQueryMethodInvocation(method, current.Result, lambda), current.CurrentTransparentType);
			}
			return current;
		}

		private JsExpression HandleSelectOrGroupClause(SelectOrGroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			if (node is SelectClauseSyntax) {
				return HandleSelectClause((SelectClauseSyntax)node, current);
			}
			else if (node is GroupClauseSyntax) {
				return HandleGroupClause((GroupClauseSyntax)node, current);
			}
			else {
				_errorReporter.InternalError("Invalid node " + node);
				return JsExpression.Null;
			}
		}

		private JsExpression HandleSelectClause(SelectClauseSyntax node, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			if (method != null) {
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.Expression, current, new string[0], x => x);
				return CompileQueryMethodInvocation(method, current.Result, lambda);
			}
			else {
				return current.Result;
			}
		}

		private JsExpression HandleGroupClause(GroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			var grouping = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.ByExpression, current, new string[0], x => x);

			switch (method.Parameters.Length) {
				case 1: {
					return CompileQueryMethodInvocation(method, current.Result, grouping);
				}

				case 2: {
					var selector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, node.GroupExpression, current, new string[0], x => x);
					return CompileQueryMethodInvocation(method, current.Result, grouping, selector);
				}

				default: {
					_errorReporter.InternalError("Invalid GroupBy call");
					return JsExpression.Null;
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
