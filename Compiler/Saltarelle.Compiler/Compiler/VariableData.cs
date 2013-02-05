using ICSharpCode.NRefactory.CSharp;

namespace Saltarelle.Compiler.Compiler {
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
