using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public abstract class JsType {
		public INamedTypeSymbol CSharpTypeDefinition { get; private set; }

		protected JsType(INamedTypeSymbol csharpTypeDefinition) {
			CSharpTypeDefinition = csharpTypeDefinition;
		}
	}
}
