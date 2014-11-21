using System;
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
			if (index < 0 || index >= block.Statements.Count)
				throw new ArgumentException("index");
			Block = block;
			Index = index;
			AfterForInitializer = afterForInitializer;
		}

		public string DebugToString() {
			return JsStatement.Block(Block.Statements.Skip(Index)).DebugToString();
		}
	}
}