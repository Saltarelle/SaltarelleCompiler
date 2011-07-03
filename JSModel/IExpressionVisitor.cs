using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel {
    public interface IExpressionVisitor<out TReturn, in TData> {
        TReturn Visit(Expression expression, TData data);
        TReturn Visit(ArrayLiteralExpression expression, TData data);
        TReturn Visit(BinaryExpression expression, TData data);
        TReturn Visit(CommaExpression expression, TData data);
        TReturn Visit(ConditionalExpression expression, TData data);
        TReturn Visit(ConstantExpression expression, TData data);
        TReturn Visit(FunctionExpression expression, TData data);
        TReturn Visit(IdentifierExpression expression, TData data);
        TReturn Visit(InvocationExpression expression, TData data);
        TReturn Visit(ObjectLiteralExpression expression, TData data);
        TReturn Visit(MemberAccessExpression expression, TData data);
        TReturn Visit(NewExpression expression, TData data);
        TReturn Visit(UnaryExpression expression, TData data);
    }
}
