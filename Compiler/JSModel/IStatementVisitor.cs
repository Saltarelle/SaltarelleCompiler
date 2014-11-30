using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
	public interface IStatementVisitor<out TReturn, in TData> {
		TReturn VisitStatement(JsStatement statement, TData data);
		TReturn VisitComment(JsComment statement, TData data);
		TReturn VisitBlockStatement(JsBlockStatement statement, TData data);
		TReturn VisitBreakStatement(JsBreakStatement statement, TData data);
		TReturn VisitContinueStatement(JsContinueStatement statement, TData data);
		TReturn VisitDoWhileStatement(JsDoWhileStatement statement, TData data);
		TReturn VisitEmptyStatement(JsEmptyStatement statement, TData data);
		TReturn VisitExpressionStatement(JsExpressionStatement statement, TData data);
		TReturn VisitForEachInStatement(JsForEachInStatement statement, TData data);
		TReturn VisitForStatement(JsForStatement statement, TData data);
		TReturn VisitIfStatement(JsIfStatement statement, TData data);
		TReturn VisitReturnStatement(JsReturnStatement statement, TData data);
		TReturn VisitSwitchStatement(JsSwitchStatement statement, TData data);
		TReturn VisitThrowStatement(JsThrowStatement statement, TData data);
		TReturn VisitTryStatement(JsTryStatement statement, TData data);
		TReturn VisitVariableDeclarationStatement(JsVariableDeclarationStatement statement, TData data);
		TReturn VisitWhileStatement(JsWhileStatement statement, TData data);
		TReturn VisitWithStatement(JsWithStatement statement, TData data);
		TReturn VisitLabel(JsLabel statement, TData data);
		TReturn VisitGotoStatement(JsGotoStatement statement, TData data);
		TReturn VisitYieldStatement(JsYieldStatement statement, TData data);
		TReturn VisitAwaitStatement(JsAwaitStatement statement, TData data);
		TReturn VisitFunctionStatement(JsFunctionStatement statement, TData data);
		TReturn VisitSequencePoint(JsSequencePoint jsSequencePoint, TData data);
	}
}
