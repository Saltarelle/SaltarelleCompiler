using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class FinalizerRewriter : RewriterVisitorBase<object>, IStateMachineRewriterIntermediateStatementsVisitor<JsStatement, object> {
		private readonly string _stateVariableName;
		private readonly Dictionary<string, State> _labelStates = new Dictionary<string, State>();

		public FinalizerRewriter(string stateVariableName, Dictionary<string, State> labelStates) {
			_stateVariableName = stateVariableName;
			_labelStates = labelStates;
		}

		public JsBlockStatement Process(JsBlockStatement statement) {
			return (JsBlockStatement)VisitStatement(statement, null);
		}

		public override JsStatement VisitGotoStatement(JsGotoStatement statement, object data) {
			throw new InvalidOperationException("Shouldn't happen");
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override IList<JsStatement> VisitStatements(IList<JsStatement> statements, object data) {
			var firstPass = VisitCollection(statements, (s, i) => {
				if (s is JsSetNextStateStatement) {
					for (int j = i + 1; j < statements.Count; j++) {
						var next = statements[j];
						if (!StateMachineRewriter.IsExecutableStatement(next))
							continue;
						else if (next is JsSetNextStateStatement || next is JsGotoStateStatement)
							return ImmutableList<JsStatement>.Empty;	// The current statement is directly overridden by the next one - ignore it.
						else
							break;
					}
				}

				var after = VisitStatement(s, data);
				var afterBlock = after as JsBlockStatement;
				if (afterBlock != null && afterBlock.MergeWithParent)
					return afterBlock.Statements;
				else
					return new[] { after };
			});

			return VisitCollection(firstPass, (s, i) => {
				if (s is JsSequencePoint && i < firstPass.Count - 1 && firstPass[i + 1] is JsSequencePoint) {
					// Ignore sequence points immediately followed by another sequence point
					return ImmutableList<JsStatement>.Empty;
				}
				else {
					return new[] { s };
				}
			});
		}

		public JsStatement VisitGotoStateStatement(JsGotoStateStatement statement, object data) {
			var result = new List<JsStatement>();
			State targetState;
			if (statement.TargetState == null) {
				if (!_labelStates.TryGetValue(statement.TargetLabel, out targetState))
					throw new InvalidOperationException("The label " + statement.TargetLabel + " does not exist.");
			}
			else
				targetState = statement.TargetState.Value;

			var remaining = statement.CurrentState.FinallyStack;
			for (int i = 0, n = remaining.Count() - targetState.FinallyStack.Count(); i < n; i++) {
				var current = remaining.Peek();
				remaining = remaining.Pop();
				result.Add(JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(remaining.IsEmpty ? -1 : remaining.Peek().Item1)));
				result.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier(current.Item2), "call"), JsExpression.This));
			}

			result.Add(MakeSetNextStateStatement(targetState.StateValue));
			result.Add(targetState.StateValue == -1 ? (JsStatement)JsStatement.Break(targetState.LoopLabelName) : JsStatement.Continue(targetState.LoopLabelName));
			result.Add(JsStatement.SequencePoint(null));
			return JsStatement.BlockMerged(result);
		}

		public JsStatement VisitSetNextStateStatement(JsSetNextStateStatement stmt, object data) {
			return MakeSetNextStateStatement(stmt.TargetStateValue);
		}

		private JsStatement MakeSetNextStateStatement(int targetStateValue) {
			return JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(targetStateValue));
		}
	}
}