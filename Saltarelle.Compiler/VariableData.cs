using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public class VariableData {
		public string Name { get; private set; }
        public AstNode DeclaringMethod { get; private set; }
		public bool UseByRefSemantics { get; private set; }

		public VariableData(string name, AstNode declaringMethod, bool useByRefSemantics) {
			Name = name;
            DeclaringMethod = declaringMethod;
			UseByRefSemantics = useByRefSemantics;
		}
	}
}
