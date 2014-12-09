using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public class JsMethod {
		public ISymbol CSharpMember { get; private set; }
		public string Name { get; private set; }
		public ReadOnlyCollection<string> TypeParameterNames { get; private set; }
		public JsFunctionDefinitionExpression Definition { get; private set; }

		public JsMethod(ISymbol csharpMember, string name, IEnumerable<string> typeParameterNames, JsFunctionDefinitionExpression definition) {
			Require.NotNull(definition, "definition");

			CSharpMember       = csharpMember;
			Name               = name;
			TypeParameterNames = typeParameterNames.AsReadOnly();
			Definition         = definition;
		}
	}
}
