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

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private struct QueryExpressionCompilationInfo {
			public readonly ImmutableArray<string> ParameterNames;
			public readonly JsExpression Result;

			public QueryExpressionCompilationInfo(ImmutableArray<string> parameterNames, JsExpression result) {
				ParameterNames = parameterNames;
				Result = result;
			}
		}

		private JsExpression HandleQueryExpression(QueryExpressionSyntax query) {
			var current = HandleFirstFromClause(query.FromClause);
			return HandleQueryBody(query.Body, current);
		}

		private JsExpression CompileQueryLambda(QueryExpressionCompilationInfo info, ISymbol symbol, ExpressionSyntax expression) {
			#warning TODO Need something better here
			var jsBody = CloneAndCompile(expression, true);
			var lambda = JsExpression.FunctionDefinition(info.ParameterNames, JsStatement.Block(jsBody.GetStatementsWithReturn()));

			var impl = _metadataImporter.GetMethodSemantics((IMethodSymbol)symbol);
			return CompileMethodInvocation(impl, (IMethodSymbol)symbol, new[] { info.Result, lambda }, false);
		}
/*
		JsExpression VisitNested(ExpressionSyntax node, string transparentParameterName) {
			var oldRangeVariableSubstitutions = _activeRangeVariableSubstitutions;
			try {
				if (transparentParameterName != null && _currentTransparentType.Count > 1) {
					_activeRangeVariableSubstitutions = new Dictionary<IRangeVariableSymbol, JsExpression>(_activeRangeVariableSubstitutions);
					foreach (var t in _currentTransparentType)
						_activeRangeVariableSubstitutions[t.Item1] = t.Item2.Aggregate((JsExpression)JsExpression.Identifier(transparentParameterName), (current, m) => JsExpression.Member(current, m));
				}
				return InnerCompile(node, false);
			}
			finally {
				_activeRangeVariableSubstitutions = oldRangeVariableSubstitutions;
			}
		}

		public override JsExpression VisitQueryBody(QueryBodySyntax node) {
			var current = HandleFromClause(node.
				_currentQueryResult = clause.Accept(this);
			if (node.SelectOrGroup != null) {
				if (node.SelectOrGroup is SelectClauseSyntax)
				_currentQueryResult = node.SelectOrGroup.Accept(this);
			}
			if (node.Continuation != null)
				_currentQueryResult = node.Continuation.Accept(this);
			return _currentQueryResult;
		}*/

		private QueryExpressionCompilationInfo HandleFirstFromClause(FromClauseSyntax node) {
			return new QueryExpressionCompilationInfo(ImmutableArray.Create(_variables[_semanticModel.GetDeclaredSymbol(node)].Name), InnerCompile(node.Expression, true));
		}

		private JsExpression HandleQueryBody(QueryBodySyntax body, QueryExpressionCompilationInfo current) {
			foreach (var clause in body.Clauses) {
				current = HandleQueryClause(clause, current);
			}
			return HandleSelectOrGroup(body.SelectOrGroup, current);
			// TODO: Continuation
		}

		private QueryExpressionCompilationInfo HandleQueryClause(QueryClauseSyntax clause, QueryExpressionCompilationInfo current) {
			return current;
		}

		private JsExpression HandleSelectOrGroup(SelectOrGroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			if (node is SelectClauseSyntax) {
				return HandleSelect((SelectClauseSyntax)node, current);
			}
			else if (node is GroupClauseSyntax) {
				return HandleGroup((GroupClauseSyntax)node, current);
			}
			else {
				_errorReporter.InternalError("Invalid node " + node);
				return JsExpression.Null;
			}
		}

		private JsExpression HandleSelect(SelectClauseSyntax node, QueryExpressionCompilationInfo current) {
			return CompileQueryLambda(current, _semanticModel.GetSymbolInfo(node).Symbol, node.Expression);
		}

		private JsExpression HandleGroup(GroupClauseSyntax node, QueryExpressionCompilationInfo current) {
			return current.Result;
		}

#if false
		public override JsExpression VisitFromClause(FromClauseSyntax node) {
			var symbol = _semanticModel.GetDeclaredSymbol(node);
			var castInfo = _semanticModel.GetQueryClauseInfo(node).CastInfo;
			if (_currentQueryResult == null) {
				AddFirstMemberToCurrentTransparentType(symbol);

				if (castInfo.Symbol == null) {
					return VisitNested(node.Expression, null);
				}
				//else {
				//	return VisitNested(ParenthesizeIfNeeded(queryFromClause.Expression), null).Invoke("Cast", new[] { queryFromClause.Type.Clone() }, new Expression[0]);
				//}
			}
			//else {
			//	var innerSelectorParam = CreateParameterForCurrentRangeVariable();
			//	var lambdaContent = VisitNested(queryFromClause.Expression, innerSelectorParam);
			//	if (!queryFromClause.Type.IsNull) {
			//		lambdaContent = lambdaContent.Invoke("Cast", new[] { queryFromClause.Type.Clone() }, new Expression[0]);
			//	}
			//	var innerSelector = CreateLambda(new[] { innerSelectorParam }, lambdaContent);
			//
			//	var clonedIdentifier = (Identifier)queryFromClause.IdentifierToken.Clone();
			//
			//	var resultParam = CreateParameterForCurrentRangeVariable();
			//	Expression body;
			//	// Second from clause - SelectMany
			//	var select = GetNextQueryClause(queryFromClause) as QuerySelectClause;
			//	if (select != null) {
			//		body = VisitNested(select.Expression, resultParam);
			//		eatSelect = true;
			//	}
			//	else {
			//		body = AddMemberToCurrentTransparentType(resultParam, queryFromClause.IdentifierToken, new IdentifierExpression(queryFromClause.Identifier), false);
			//	}
			//
			//	var resultSelectorParam2 = CreateParameter(clonedIdentifier);
			//	var resultSelector = CreateLambda(new[] { resultParam, resultSelectorParam2 }, body);
			//	rangeVariables[queryFromClause.IdentifierToken] = resultSelectorParam2;
			//
			//	return currentResult.Invoke("SelectMany", innerSelector, resultSelector);
			//}
			return JsExpression.Null;
		}

		public override JsExpression VisitJoinClause(JoinClauseSyntax node) {
			return JsExpression.Null;
		}

		public override JsExpression VisitLetClause(LetClauseSyntax node) {
			return JsExpression.Null;
		}

		public override JsExpression VisitOrderByClause(OrderByClauseSyntax node) {
			return JsExpression.Null;
		}

		public override JsExpression VisitWhereClause(WhereClauseSyntax node) {
			return JsExpression.Null;
		}

		public override JsExpression VisitSelectClause(SelectClauseSyntax node) {
			if (_eatSelect) {
				_eatSelect = false;
				return _currentQueryResult;
			}
			//else if (((QueryExpression)querySelectClause.Parent).Clauses.Count > 2 && IsSingleRangeVariable(querySelectClause.Expression)) {
			//	// A simple query that ends with a trivial select should be removed.
			//	return _currentQueryResult;
			//}

			//var param = CreateParameterForCurrentRangeVariable();
			//var lambda = CreateLambda(new[] { param }, VisitNested(querySelectClause.Expression, param));
			//return currentResult.Invoke("Select", lambda);
			return JsExpression.Null;
		}

		public override JsExpression VisitGroupClause(GroupClauseSyntax node) {
			return JsExpression.Null;
		}
#endif
		private JsExpression CompileRangeVariableAccess(IRangeVariableSymbol symbol) {
			return JsExpression.Identifier(_variables[symbol].Name);
		}
	}
}
