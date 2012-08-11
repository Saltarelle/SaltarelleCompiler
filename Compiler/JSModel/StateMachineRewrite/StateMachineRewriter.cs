using System;
using System.Collections.Generic;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite {
	public class StateMachineRewriter : RewriterVisitorBase<object> {
		private readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		private readonly Func<string> _allocateTempVariable;
		private readonly Func<string> _allocateLoopLabel;
		private readonly Func<string> _allocateFinallyHandler;
		private readonly Func<JsExpression, JsExpression> _makeSetCurrent;
		private readonly Func<IteratorStateMachine, JsBlockStatement> _makeIteratorBody;

		private readonly List<Tuple<string, JsFunctionDefinitionExpression>> _finallyHandlers = new List<Tuple<string, JsFunctionDefinitionExpression>>();

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, Func<IteratorStateMachine, JsBlockStatement> makeIteratorBody) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;
			_makeIteratorBody = makeIteratorBody;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			var body = DoRewrite((JsBlockStatement)VisitStatement(expression.Body, data));
	        return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			var body = DoRewrite((JsBlockStatement)VisitStatement(statement.Body, data));
	        return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
		}

		private JsBlockStatement DoRewrite(JsBlockStatement block) {
			if (!FindInterestingConstructsVisitor.Analyze(block, InterestingConstruct.Label | InterestingConstruct.YieldReturn | InterestingConstruct.YieldBreak))
				return block;

			var result = new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel).Process(block);

			var hoistResult = VariableHoistingVisitor.Process(result);
			if (!ReferenceEquals(hoistResult.Item1, result)) {
				result = new JsBlockStatement(new[] { new JsVariableDeclarationStatement(hoistResult.Item2.Select(v => new JsVariableDeclaration(v, null))) }.Concat(hoistResult.Item1.Statements));
			}
			return result;
		}

		public static JsBlockStatement Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel, null, null, null);
			return obj.DoRewrite((JsBlockStatement)obj.VisitStatement(block, null));
		}

		public static JsBlockStatement RewriteIteratorBlock(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, Func<IteratorStateMachine, JsBlockStatement> makeIteratorBody) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel, allocateFinallyHandler, makeSetCurrent, makeIteratorBody);
			var singleRewriter = new SingleStateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel);
			var sm = singleRewriter.ProcessIteratorBlock(block, allocateFinallyHandler, makeSetCurrent);
			return makeIteratorBody(sm);
		}
	}
}
