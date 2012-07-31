using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
    public interface IStatementVisitor<out TReturn, in TData> {
        TReturn Visit(JsStatement statement, TData data);
        TReturn Visit(JsComment statement, TData data);
        TReturn Visit(JsBlockStatement statement, TData data);
        TReturn Visit(JsBreakStatement statement, TData data);
        TReturn Visit(JsContinueStatement statement, TData data);
        TReturn Visit(JsDoWhileStatement statement, TData data);
        TReturn Visit(JsEmptyStatement statement, TData data);
        TReturn Visit(JsExpressionStatement statement, TData data);
        TReturn Visit(JsForEachInStatement statement, TData data);
        TReturn Visit(JsForStatement statement, TData data);
        TReturn Visit(JsIfStatement statement, TData data);
        TReturn Visit(JsReturnStatement statement, TData data);
        TReturn Visit(JsSwitchStatement statement, TData data);
        TReturn Visit(JsThrowStatement statement, TData data);
        TReturn Visit(JsTryCatchFinallyStatement statement, TData data);
        TReturn Visit(JsVariableDeclarationStatement statement, TData data);
        TReturn Visit(JsWhileStatement statement, TData data);
        TReturn Visit(JsWithStatement statement, TData data);
		TReturn Visit(JsLabelledStatement statement, TData data);
		TReturn Visit(JsGotoStatement statement, TData data);
		TReturn Visit(JsYieldReturnStatement statement, TData data);
		TReturn Visit(JsYieldBreakStatement statement, TData data);
		TReturn Visit(JsFunctionStatement statement, TData data);
    }
}
