using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	internal class ContainsYieldReturnVisitor : RewriterVisitorBase<object> {
		bool _result;

		private ContainsYieldReturnVisitor() {
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			_result = true;
			return statement;
		}

		public static bool Analyze(JsStatement statement) {
			var obj = new ContainsYieldReturnVisitor();
			obj.VisitStatement(statement, null);
			return obj._result;
		}
	}
}