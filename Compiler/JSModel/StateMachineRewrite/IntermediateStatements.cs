using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
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

		public override string ToString() {
			return "gotoState(" + (TargetState != null ? TargetState.Value.StateValue.ToString() : TargetLabel) + ");";
		}
	}

	internal class JsSetNextStateStatement : JsStatement {
		public int TargetStateValue { get; private set; }

		public JsSetNextStateStatement(int targetStateValue) {
			TargetStateValue = targetStateValue;
		}

		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return ((IStateMachineRewriterIntermediateStatementsVisitor<TReturn, TData>)visitor).VisitSetNextStateStatement(this, data);
		}

		public override string ToString() {
			return "setNextState(" + TargetStateValue + ");";
		}
	}
}