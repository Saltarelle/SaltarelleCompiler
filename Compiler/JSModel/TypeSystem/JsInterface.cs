using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	/// <summary>
	/// An interface.
	/// </summary>
	public class JsInterface : JsType {
		public JsInterface(INamedTypeSymbol csharpTypeDefinition) : base(csharpTypeDefinition) {
		}
	}
}
