using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite {
	internal class UsesThisVisitor : RewriterVisitorBase<object> {
		bool _result;

		private UsesThisVisitor() {
			_result = false;
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsExpression VisitThisExpression(JsThisExpression expression, object data) {
			_result = true;
			return expression;
		}

		public static bool Analyze(JsStatement statement) {
			var obj = new UsesThisVisitor();
			obj.VisitStatement(statement, null);
			return obj._result;
		}
	}
}