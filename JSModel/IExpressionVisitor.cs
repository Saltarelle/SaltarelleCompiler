using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel {
    public interface IExpressionVisitor<TReturn> {
        TReturn Visit(ArrayLiteralExpression expression);
        TReturn Visit(BinaryExpression expression);
        TReturn Visit(CommaExpression expression);
        TReturn Visit(ConditionalExpression expression);
        TReturn Visit(ConstantExpression expression);
        TReturn Visit(FunctionExpression expression);
        TReturn Visit(IdentifierExpression expression);
        TReturn Visit(InvocationExpression expression);
        TReturn Visit(JsonExpression expression);
        TReturn Visit(MemberAccessExpression expression);
        TReturn Visit(NewExpression expression);
        TReturn Visit(UnaryExpression expression);
    }
}
