using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.Compiler {
	public class VariableData {
		public string Name { get; private set; }
		public SyntaxNode DeclaringMethod { get; private set; }
		public bool UseByRefSemantics { get; private set; }

		public VariableData(string name, SyntaxNode declaringMethod, bool useByRefSemantics) {
			Name = name;
			DeclaringMethod = declaringMethod;
			UseByRefSemantics = useByRefSemantics;
		}
	}
}
