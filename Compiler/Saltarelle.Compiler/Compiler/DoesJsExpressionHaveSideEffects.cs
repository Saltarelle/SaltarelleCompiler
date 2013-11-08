using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Compiler {
	public class DoesJsExpressionHaveSideEffects : RewriterVisitorBase<object> {
		private bool _result;

		public static bool Analyze(JsExpression expression) {
			var v = new DoesJsExpressionHaveSideEffects();
			expression.Accept(v, null);
			return v._result;
		}

		public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
			_result = true;
			return expression;
		}

		public override JsExpression VisitBinaryExpression(JsBinaryExpression expression, object data) {
			if (expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast) {
				_result = true;
				return expression;
			}
			else {
				return base.VisitBinaryExpression(expression, data);
			}
		}

		public override JsExpression VisitUnaryExpression(JsUnaryExpression expression, object data) {
			switch (expression.NodeType) {
				case ExpressionNodeType.PrefixPlusPlus:
				case ExpressionNodeType.PrefixMinusMinus:
				case ExpressionNodeType.PostfixPlusPlus:
				case ExpressionNodeType.PostfixMinusMinus:
				case ExpressionNodeType.Delete:
					_result = true;
					return expression;
				default:
					return base.VisitUnaryExpression(expression, data);
			}
		}
	}
}