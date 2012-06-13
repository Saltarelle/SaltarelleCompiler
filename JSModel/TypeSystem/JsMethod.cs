using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsMethod {
        public string Name { get; private set; }
        public ReadOnlyCollection<string> TypeParameterNames { get; private set; }
        public JsFunctionDefinitionExpression Definition { get; private set; }

        public JsMethod(string name, IEnumerable<string> typeParameterNames, JsFunctionDefinitionExpression definition) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Name               = name;
            TypeParameterNames = typeParameterNames.AsReadOnly();
            Definition         = definition;
        }
    }
}
