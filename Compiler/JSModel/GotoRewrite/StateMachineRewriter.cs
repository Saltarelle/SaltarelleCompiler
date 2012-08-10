using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	public class StateMachineRewriter : RewriterVisitorBase<object> {
		private readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		private readonly Func<string> _allocateTempVariable;
		private readonly Func<string> _allocateLoopLabel;
		private readonly Func<string> _allocateFinallyHandler;
		private readonly Func<JsExpression, JsExpression> _makeSetCurrent;
		private readonly bool _isIteratorBlock;

		private readonly List<Tuple<string, JsFunctionDefinitionExpression>> _finallyHandlers = new List<Tuple<string, JsFunctionDefinitionExpression>>();

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;
			_isIteratorBlock = isIteratorBlock;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			var body = DoRewrite((JsBlockStatement)VisitBlockStatement(expression.Body, data));
	        return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			var body = DoRewrite((JsBlockStatement)VisitBlockStatement(statement.Body, data));
	        return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
		}

		private string AddFinallyHandler(JsBlockStatement body) {
			var name = _allocateFinallyHandler();
			_finallyHandlers.Add(Tuple.Create(name, new JsFunctionDefinitionExpression(new string[0], body, null)));
			return name;
		}

		private JsBlockStatement DoRewrite(JsBlockStatement block) {
			if (!FindInterestingConstructsVisitor.Analyze(block, InterestingConstruct.Label | InterestingConstruct.YieldReturn | InterestingConstruct.YieldBreak))
				return block;

			return new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel, AddFinallyHandler, _makeSetCurrent).Process(block, _isIteratorBlock);
		}

		public static StateMachine Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel, allocateFinallyHandler, makeSetCurrent, isIteratorBlock);
			var mainBlock = obj.DoRewrite((JsBlockStatement)obj.VisitBlockStatement(block, null));
			return new StateMachine(mainBlock, obj._finallyHandlers, new JsBlockStatement());
		}
	}
}
