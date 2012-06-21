using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsNamedConstructor {
        public string Name { get; private set; }
        public JsFunctionDefinitionExpression Definition { get; private set; }

        public JsNamedConstructor(string name, JsFunctionDefinitionExpression definition) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Name       = name;
            Definition = definition;
        }
    }
}
