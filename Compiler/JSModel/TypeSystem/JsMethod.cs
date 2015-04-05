using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public enum JsMethodKind {
		NormalMethod,
		GetAccessor,
		SetAccessor
	}

	public class JsMethod {
		public ISymbol CSharpMember { get; private set; }
		public string Name { get; private set; }
		public JsMethodKind Kind { get; private set; }
		public ReadOnlyCollection<string> TypeParameterNames { get; private set; }
		public JsFunctionDefinitionExpression Definition { get; private set; }

		public JsMethod(ISymbol csharpMember, string name, JsMethodKind kind, IEnumerable<string> typeParameterNames, JsFunctionDefinitionExpression definition) {
			Require.NotNull(definition, "definition");

			CSharpMember       = csharpMember;
			Kind               = kind;
			Name               = name;
			TypeParameterNames = typeParameterNames.AsReadOnly();
			Definition         = definition;
		}
	}
}
