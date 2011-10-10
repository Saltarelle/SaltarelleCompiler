using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel {
    public interface IExpressionVisitor<out TReturn, in TData> {
        TReturn Visit(JsExpression expression, TData data);
        TReturn Visit(JsArrayLiteralExpression expression, TData data);
        TReturn Visit(JsBinaryExpression expression, TData data);
        TReturn Visit(JsCommaExpression expression, TData data);
        TReturn Visit(JsConditionalExpression expression, TData data);
        TReturn Visit(JsConstantExpression expression, TData data);
        TReturn Visit(JsFunctionDefinitionExpression expression, TData data);
        TReturn Visit(JsIdentifierExpression expression, TData data);
        TReturn Visit(JsInvocationExpression expression, TData data);
        TReturn Visit(JsObjectLiteralExpression expression, TData data);
        TReturn Visit(JsMemberAccessExpression expression, TData data);
        TReturn Visit(JsNewExpression expression, TData data);
        TReturn Visit(JsUnaryExpression expression, TData data);
        TReturn Visit(JsTypeReferenceExpression expression, TData data);
    }
}
