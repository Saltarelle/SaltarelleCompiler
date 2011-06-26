using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
    public interface IStatementVisitor<out TReturn> {
        TReturn Visit(BlockStatement statement);
        TReturn Visit(BreakStatement statement);
        TReturn Visit(ContinueStatement statement);
        TReturn Visit(DoWhileStatement statement);
        TReturn Visit(EmptyStatement statement);
        TReturn Visit(ExpressionStatement statement);
        TReturn Visit(ForEachInStatement statement);
        TReturn Visit(ForStatement statement);
        TReturn Visit(IfStatement statement);
        TReturn Visit(ReturnStatement statement);
        TReturn Visit(SwitchStatement statement);
        TReturn Visit(ThrowStatement statement);
        TReturn Visit(TryCatchFinallyStatement statement);
        TReturn Visit(VariableDeclarationStatement statement);
        TReturn Visit(WhileStatement statement);
        TReturn Visit(WithStatement statement);
    }
}
