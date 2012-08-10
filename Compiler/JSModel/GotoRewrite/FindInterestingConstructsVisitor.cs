using System;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	[Flags]
	internal enum InterestingConstruct {
		None        = 0,
		Label       = 1,
		YieldReturn = 2,
		YieldBreak  = 4,
		Goto        = 8,
	}

	internal class FindInterestingConstructsVisitor : RewriterVisitorBase<object>, IStateMachineRewriterIntermediateStatementsVisitor<JsStatement, object> {
		InterestingConstruct _result;

		private FindInterestingConstructsVisitor() {
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitLabelledStatement(JsLabelledStatement statement, object data) {
			_result |= InterestingConstruct.Label;
			return statement;
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			if (statement.Value != null)
				_result |= InterestingConstruct.YieldReturn;
			else
				_result |= InterestingConstruct.YieldBreak;
			return statement;
		}

		public override JsStatement VisitGotoStatement(JsGotoStatement statement, object data) {
			_result |= InterestingConstruct.Goto;
			return statement;
		}

		public static InterestingConstruct Analyze(JsStatement statement) {
			var obj = new FindInterestingConstructsVisitor();
			obj.VisitStatement(statement, null);
			return obj._result;
		}

		public static bool Analyze(JsStatement statement, InterestingConstruct scanFor) {
			return (Analyze(statement) & scanFor) != InterestingConstruct.None;
		}

		public JsStatement VisitGotoStateStatement(JsGotoStateStatement stmt, object data) {
			return stmt;
		}

		public JsStatement VisitSetNextStateStatement(JsSetNextStateStatement stmt, object data) {
			return stmt;
		}
	}
}