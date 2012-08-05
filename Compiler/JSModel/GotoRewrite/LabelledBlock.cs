using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	/// <summary>
	/// A labelled block represents a unit of flow, through which control can only flow in at the top.'
	/// All paths out of a labelled block must end with a 'goto' to some other block.
	/// </summary>
	internal class LabelledBlock {
		public string Name { get; private set; }
		public int StateValue { get; private set; }
		public IList<JsStatement> Statements { get; private set; }

		public LabelledBlock(string name, int stateValue, IEnumerable<JsStatement> statements) {
			this.Name       = name;
			this.StateValue = stateValue;
			this.Statements = new List<JsStatement>(statements);
		}

		public void Freeze() {
			if (Statements is List<JsStatement>)
				Statements = ((List<JsStatement>)Statements).AsReadOnly();
		}
	}
}
