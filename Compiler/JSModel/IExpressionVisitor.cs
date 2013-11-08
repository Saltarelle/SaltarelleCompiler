using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel {
	public interface IExpressionVisitor<out TReturn, in TData> {
		TReturn VisitExpression(JsExpression expression, TData data);
		TReturn VisitArrayLiteralExpression(JsArrayLiteralExpression expression, TData data);
		TReturn VisitBinaryExpression(JsBinaryExpression expression, TData data);
		TReturn VisitCommaExpression(JsCommaExpression expression, TData data);
		TReturn VisitConditionalExpression(JsConditionalExpression expression, TData data);
		TReturn VisitConstantExpression(JsConstantExpression expression, TData data);
		TReturn VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, TData data);
		TReturn VisitIdentifierExpression(JsIdentifierExpression expression, TData data);
		TReturn VisitInvocationExpression(JsInvocationExpression expression, TData data);
		TReturn VisitObjectLiteralExpression(JsObjectLiteralExpression expression, TData data);
		TReturn VisitMemberAccessExpression(JsMemberAccessExpression expression, TData data);
		TReturn VisitNewExpression(JsNewExpression expression, TData data);
		TReturn VisitUnaryExpression(JsUnaryExpression expression, TData data);
		TReturn VisitTypeReferenceExpression(JsTypeReferenceExpression expression, TData data);
		TReturn VisitThisExpression(JsThisExpression expression, TData data);
	}
}
