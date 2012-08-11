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
		private readonly bool _isIteratorBlock;

		private readonly List<Tuple<string, JsFunctionDefinitionExpression>> _finallyHandlers = new List<Tuple<string, JsFunctionDefinitionExpression>>();
		private readonly List<Tuple<int, List<string>>> _stateFinallyHandlers = new List<Tuple<int, List<string>>>();

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;
			_isIteratorBlock = isIteratorBlock;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			var body = DoRewrite((JsBlockStatement)VisitStatement(expression.Body, data));
	        return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			var body = DoRewrite((JsBlockStatement)VisitStatement(statement.Body, data));
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

			var rewriteResult = new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel, AddFinallyHandler, _makeSetCurrent).Process(block, _isIteratorBlock);
			_stateFinallyHandlers.AddRange(rewriteResult.Item2.Where(x => x.Item2.Count > 0));

			var hoistResult = VariableHoistingVisitor.Process(rewriteResult.Item1);
			JsBlockStatement body;
			if (!ReferenceEquals(hoistResult.Item1, rewriteResult.Item1)) {
				body = new JsBlockStatement(new[] { new JsVariableDeclarationStatement(hoistResult.Item2.Select(v => new JsVariableDeclaration(v, null))) }.Concat(hoistResult.Item1.Statements));
			}
			else
				body = rewriteResult.Item1;
			return body;
		}

		public static StateMachine Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel, allocateFinallyHandler, makeSetCurrent, isIteratorBlock);
			var mainBlock = obj.DoRewrite((JsBlockStatement)obj.VisitStatement(block, null));
			#warning TODO: Obviously needs fixing
			return new StateMachine(mainBlock, obj._finallyHandlers, DisposeGenerator.GenerateDisposer("$tmp1", obj._stateFinallyHandlers));
		}
	}
}
