using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	internal class NeedsRewriteVisitor : RewriterVisitorBase<object>, IGotoStateStatementVisitor<JsStatement, object> {
		bool _yieldBreakNeedsRewrite;
		bool _result;

		private NeedsRewriteVisitor(bool yieldBreakNeedsRewrite) {
			_yieldBreakNeedsRewrite = yieldBreakNeedsRewrite;
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitLabelledStatement(JsLabelledStatement statement, object data) {
			_result = true;
			return statement;
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			if (statement.Value != null || _yieldBreakNeedsRewrite)
				_result = true;
			return statement;
		}

		public static bool Analyze(JsStatement statement, bool yieldBreakNeedsRewrite) {
			var obj = new NeedsRewriteVisitor(yieldBreakNeedsRewrite);
			obj.VisitStatement(statement, null);
			return obj._result;
		}

		public JsStatement VisitGotoStateStatement(JsGotoStateStatement stmt, object data) {
			return stmt;
		}
	}
}