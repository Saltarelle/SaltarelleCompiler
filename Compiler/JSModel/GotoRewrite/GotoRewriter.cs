using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	public class GotoRewriter {
		private class RewriteGotoStatementsVisitor : RewriterVisitorBase<object> {
			private readonly IDictionary<string, int> _stateMap;
			private readonly string _loopLabel;
			private readonly string _stateVariable;

			public override JsStatement VisitBlockStatement(JsBlockStatement statement, object data) {
	            List<JsStatement> list = null;
	            for (int i = 0; i < statement.Statements.Count; i++) {
	                var before = statement.Statements[i];

					JsStatement[] after;
					if (before is JsGotoStatement) {
						var gotoStmt = (JsGotoStatement)before;
						if (gotoStmt.TargetLabel == LabelledBlockGatherer.ExitLabelName) {
							after = new[] { new JsBreakStatement(_loopLabel) };
						}
						else {
							after = new JsStatement[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_stateVariable), JsExpression.Number(_stateMap[((JsGotoStatement)before).TargetLabel]))),
							                            new JsContinueStatement(_loopLabel)
							                          };
						}
					}
					else {
						after = new[] { VisitStatement(before, null) };
					}

	                if (list != null) {
	                    list.AddRange(after);
	                }
	                else if (after.Length > 1 || !ReferenceEquals(before, after[0])) {
	                    list = new List<JsStatement>();
	                    for (int j = 0; j < i; j++)
	                        list.Add(statement.Statements[j]);
	                    list.AddRange(after);
	                }
	            }
	            return list != null ? new JsBlockStatement(list) : statement;
	        }

			public RewriteGotoStatementsVisitor(IDictionary<string, int> stateMap, string loopLabel, string stateVariable) {
				_stateMap      = stateMap;
				_loopLabel     = loopLabel;
				_stateVariable = stateVariable;
			}

			public JsBlockStatement Rewrite(JsBlockStatement statement) {
				return (JsBlockStatement)VisitBlockStatement(statement, null);
			}
		}

		private class Visitor : RewriterVisitorBase<object> {
			private readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
			private readonly Func<string> _allocateTempVariable;

			private int _currentLoopNameIndex;
	
			public Visitor(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable) {
				_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
				_allocateTempVariable = allocateTempVariable;
			}

			public override JsCatchClause Visit(JsCatchClause clause, object data) {
				var body = DoRewrite((JsBlockStatement)VisitBlockStatement(clause.Body, data));
				return ReferenceEquals(body, clause.Body) ? clause : new JsCatchClause(clause.Identifier, body);
			}

			public override JsStatement VisitTryStatement(JsTryStatement statement, object data) {
				var guarded  = DoRewrite((JsBlockStatement)VisitBlockStatement(statement.GuardedStatement, data));
				var @catch   = statement.Catch != null ? Visit(statement.Catch, data) : null;
				var @finally = statement.Finally != null ? DoRewrite((JsBlockStatement)VisitBlockStatement(statement.Finally, data)) : null;

	            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new JsTryStatement(guarded, @catch, @finally);
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				var body = DoRewrite((JsBlockStatement)VisitBlockStatement(expression.Body, data));
	            return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				var body = DoRewrite((JsBlockStatement)VisitBlockStatement(statement.Body, data));
	            return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
			}
	
			public JsBlockStatement DoRewrite(JsBlockStatement block) {
				if (!ContainsLabelsVisitor.Analyze(block))
					return block;
	
				var labelledBlocks = new LabelledBlockGatherer(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable).Gather(block);
				var stateMap  = labelledBlocks.Select((b, i) => new { b, i }).ToDictionary(x => x.b.Name, x => x.i);
				var loopLabel = "$loop" + (++_currentLoopNameIndex).ToString(CultureInfo.InvariantCulture);
				var stateVar  = _allocateTempVariable();
	
				var gotoStatementRewriter = new RewriteGotoStatementsVisitor(stateMap, loopLabel, stateVar);
	
				return new JsBlockStatement(new JsStatement[] {
				    new JsVariableDeclarationStatement(stateVar, JsExpression.Number(0)),
				    new JsLabelledStatement(loopLabel,
					    new JsForStatement(new JsEmptyStatement(), null, null,
				            new JsSwitchStatement(JsExpression.Identifier(stateVar),
				                labelledBlocks.Select(b =>
				                    new JsSwitchSection(
				                        new[] { JsExpression.Number(stateMap[b.Name]) },
				                        gotoStatementRewriter.Rewrite(new JsBlockStatement(b.Statements)))))))
				});
			}
		}

		public static JsBlockStatement Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable) {
			var visitor = new Visitor(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable);
			return visitor.DoRewrite((JsBlockStatement)visitor.VisitBlockStatement(block, null));
		}
	}
}
