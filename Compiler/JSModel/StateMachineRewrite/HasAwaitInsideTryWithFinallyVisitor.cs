using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class HasAwaitInsideTryWithFinallyVisitor : RewriterVisitorBase<object> {
		bool _result;

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitTryStatement(JsTryStatement statement, object data) {
			if (statement.Finally != null) {
				_result |= FindInterestingConstructsVisitor.Analyze(statement.GuardedStatement, InterestingConstruct.Await);
				return statement;
			}
			else {
				return base.VisitTryStatement(statement, data);
			}
		}

		public bool Analyze(JsBlockStatement block) {
			_result = false;
			VisitStatement(block, null);
			return _result;
		}
	}
}