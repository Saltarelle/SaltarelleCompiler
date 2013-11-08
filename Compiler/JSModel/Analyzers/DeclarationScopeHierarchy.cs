using System.Collections.Generic;

namespace Saltarelle.Compiler.JSModel.Analyzers {
	public class DeclarationScopeHierarchy {
		public JsDeclarationScope ParentScope { get; private set; }
		public IList<JsDeclarationScope> ChildScopes { get; private set; }

		public DeclarationScopeHierarchy(JsDeclarationScope parentScope) {
			ParentScope = parentScope;
			ChildScopes = new List<JsDeclarationScope>();
		}
	}
}
