using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite
{
	internal interface IStateMachineRewriterIntermediateStatementsVisitor<TReturn, TData> {
		TReturn VisitGotoStateStatement(JsGotoStateStatement stmt, TData data);
		TReturn VisitSetNextStateStatement(JsSetNextStateStatement stmt, TData data);
	}

	internal class JsGotoStateStatement : JsStatement {
		public string TargetLabel { get; private set; }
		public State? TargetState { get; private set; }
		public State CurrentState { get; private set; }

		public JsGotoStateStatement(string targetLabel, State currentState) {
			TargetLabel = targetLabel;
			CurrentState = currentState;
		}

		public JsGotoStateStatement(State targetState, State currentState) {
			TargetState = targetState;
			CurrentState = currentState;
		}

		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return ((IStateMachineRewriterIntermediateStatementsVisitor<TReturn, TData>)visitor).VisitGotoStateStatement(this, data);
		}
	}

	internal class JsSetNextStateStatement : JsStatement {
		public State TargetState { get; private set; }

		public JsSetNextStateStatement(State targetState) {
			TargetState = targetState;
		}

		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return ((IStateMachineRewriterIntermediateStatementsVisitor<TReturn, TData>)visitor).VisitSetNextStateStatement(this, data);
		}
	}
}