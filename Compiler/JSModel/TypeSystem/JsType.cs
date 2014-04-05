using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public abstract class JsType : IFreezable {
		protected bool Frozen { get; private set; }
		public INamedTypeSymbol CSharpTypeDefinition { get; private set; }

		protected JsType(INamedTypeSymbol csharpTypeDefinition) {
			CSharpTypeDefinition = csharpTypeDefinition;
		}

		public virtual void Freeze() {
			Frozen = true;
		}
	}
}
