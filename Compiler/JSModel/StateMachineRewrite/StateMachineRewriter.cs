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

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
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

			return new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel).Process(block);
		}

		public static JsBlockStatement Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel);
			return obj.DoRewrite((JsBlockStatement)obj.VisitStatement(block, null));
		}

		public static JsBlockStatement RewriteIteratorBlock(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, Func<IteratorStateMachine, JsBlockStatement> makeIteratorBody) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel);
			var singleRewriter = new SingleStateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel);
			var sm = singleRewriter.ProcessIteratorBlock((JsBlockStatement)obj.VisitStatement(block, null), allocateFinallyHandler, makeSetCurrent);
			return makeIteratorBody(sm);
		}
	}
}
