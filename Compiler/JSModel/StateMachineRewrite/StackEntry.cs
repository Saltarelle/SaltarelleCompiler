using System.Diagnostics;
using System.Linq;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	[DebuggerDisplay("{DebugToString()}")]
	internal class StackEntry {
		public JsBlockStatement Block { get; private set; }
		public int Index { get; private set; }
		public bool AfterForInitializer { get; private set; }

		public StackEntry(JsBlockStatement block, int index, bool afterForInitializer = false) {
			Block = block;
			Index = index;
			AfterForInitializer = afterForInitializer;
		}

		public string DebugToString() {
			return new JsBlockStatement(Block.Statements.Skip(Index)).DebugToString();
		}
	}
}