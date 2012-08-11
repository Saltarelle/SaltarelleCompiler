using System;
using ICSharpCode.NRefactory.Utils;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class RemainingBlock {
		public ImmutableStack<StackEntry> Stack { get; private set; }
		public ImmutableStack<Tuple<string, State>> BreakStack { get; private set; }
		public ImmutableStack<Tuple<string, State>> ContinueStack { get; private set; }
		public State StateValue { get; private set; }
		public State ReturnState { get; private set; }

		public RemainingBlock(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State stateValue, State returnState) {
			Stack = stack;
			BreakStack = breakStack;
			ContinueStack = continueStack;
			StateValue = stateValue;
			ReturnState = returnState;
		}
	}
}