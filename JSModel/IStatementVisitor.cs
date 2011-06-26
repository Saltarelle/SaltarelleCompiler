using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
    public interface IStatementVisitor<out TReturn, in TData> {
        TReturn Visit(Statement statement, TData data);
        TReturn Visit(BlockStatement statement, TData data);
        TReturn Visit(BreakStatement statement, TData data);
        TReturn Visit(ContinueStatement statement, TData data);
        TReturn Visit(DoWhileStatement statement, TData data);
        TReturn Visit(EmptyStatement statement, TData data);
        TReturn Visit(ExpressionStatement statement, TData data);
        TReturn Visit(ForEachInStatement statement, TData data);
        TReturn Visit(ForStatement statement, TData data);
        TReturn Visit(IfStatement statement, TData data);
        TReturn Visit(ReturnStatement statement, TData data);
        TReturn Visit(SwitchStatement statement, TData data);
        TReturn Visit(ThrowStatement statement, TData data);
        TReturn Visit(TryCatchFinallyStatement statement, TData data);
        TReturn Visit(VariableDeclarationStatement statement, TData data);
        TReturn Visit(WhileStatement statement, TData data);
        TReturn Visit(WithStatement statement, TData data);
    }
}
