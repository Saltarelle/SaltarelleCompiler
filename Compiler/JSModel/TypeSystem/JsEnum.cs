using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public class JsEnum : JsType {
		public JsEnum(INamedTypeSymbol csharpTypeDefinition) : base(csharpTypeDefinition) {
		}
	}
}
