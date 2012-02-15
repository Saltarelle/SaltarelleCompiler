using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public class NestedFunctionData {
		public bool UsesThis { get; private set; }
		public ISet<IVariable> UsedByRefVariables { get; private set; }

		public NestedFunctionData(bool usesThis, ISet<IVariable> usedByRefVariables) {
			UsesThis = usesThis;
			UsedByRefVariables = new ReadOnlySet<IVariable>(usedByRefVariables);
		}
	}
}
