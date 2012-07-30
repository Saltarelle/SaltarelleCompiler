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
		internal const string ExitLabelName = "$exit";	// Use this label as a name to denote that when control leaves a block through this path, it means that it is leaving the current state machine.

		public string Name { get; private set; }
		public ReadOnlyCollection<JsStatement> Statements { get; private set; }

		public LabelledBlock(string name, IEnumerable<JsStatement> statements) {
			this.Name       = name;
			this.Statements = statements.AsReadOnly();
		}
	}
}
