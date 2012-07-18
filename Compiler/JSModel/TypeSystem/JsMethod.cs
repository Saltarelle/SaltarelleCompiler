using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsMethod {
		public IMember CSharpMember { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<string> TypeParameterNames { get; private set; }
        public JsFunctionDefinitionExpression Definition { get; private set; }

        public JsMethod(IMember csharpMember, string name, IEnumerable<string> typeParameterNames, JsFunctionDefinitionExpression definition) {
            Require.ValidJavaScriptIdentifier(name, "name");
			CSharpMember       = csharpMember;
            Name               = name;
            TypeParameterNames = typeParameterNames.AsReadOnly();
            Definition         = definition;
        }
    }
}
