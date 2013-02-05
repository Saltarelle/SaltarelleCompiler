using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Compiler {
	internal class IsJsExpressionComplexEnoughToGetATemporaryVariable : RewriterVisitorBase<object> {
		private bool _result;

		public static bool Analyze(JsExpression expression) {
			var v = new IsJsExpressionComplexEnoughToGetATemporaryVariable();
			expression.Accept(v, null);
			return v._result;
		}

		public override JsExpression VisitArrayLiteralExpression(JsArrayLiteralExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitBinaryExpression(JsBinaryExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitCommaExpression(JsCommaExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitObjectLiteralExpression(JsObjectLiteralExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitUnaryExpression(JsUnaryExpression expression, object data) {
			_result = true;
			return expression;
		}
	}
}