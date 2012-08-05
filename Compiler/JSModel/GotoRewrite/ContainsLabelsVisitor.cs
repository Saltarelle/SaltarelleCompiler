using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	internal class ContainsLabelsVisitor : RewriterVisitorBase<object> {
		bool _result;

		private ContainsLabelsVisitor() {
		}

		public override JsStatement VisitTryStatement(JsTryStatement statement, object data) {
			// This is a little hacky. In our state machines, we emit labelled loops, which the LabelledBlockGatherer doesn't like because it doesn't realize that no rewrite is necessary because the labels are only used for break and continue. In the long run, this should probably be fixed, but it will not cause any problems since we never want to go into or out of try/catch/finally anyway.
			return statement;
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
			if (statement.Value != null)
				_result = true;	// 'yield return' contains an implicit label.
			return statement;
		}

		public static bool Analyze(JsStatement statement) {
			var obj = new ContainsLabelsVisitor();
			obj.VisitStatement(statement, null);
			return obj._result;
		}
	}
}