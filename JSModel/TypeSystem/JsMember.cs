using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsMember {
        public string Name { get; private set; }
        public JsExpression Initializer { get; private set; }

        public JsMember(string name, JsExpression initializer) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Require.NotNull(initializer, "initializer");
            Name = name;
            Initializer = initializer;
        }
    }
}
