using System;
using ICSharpCode.NRefactory.Utils;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal struct State : IEquatable<State> {
		public string LoopLabelName { get; private set; }
		public int StateValue { get; private set; }
		public ImmutableStack<Tuple<int, string>> FinallyStack { get; private set; }

		public State(string loopLabelName, int stateValue, ImmutableStack<Tuple<int, string>> finallyStack) : this() {
			LoopLabelName = loopLabelName;
			StateValue = stateValue;
			FinallyStack = finallyStack;
		}

		public bool Equals(State other) {
			return other.StateValue == StateValue;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(State)) return false;
			return Equals((State)obj);
		}

		public override int GetHashCode() {
			return StateValue;
		}
	}
}