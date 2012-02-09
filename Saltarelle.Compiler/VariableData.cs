using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public class VariableData {
		public string Name { get; private set; }
		public bool IsUsedByRef { get; private set; }

		public VariableData(string name, bool isUsedByRef) {
			Name = name;
			IsUsedByRef = isUsedByRef;
		}
	}
}
