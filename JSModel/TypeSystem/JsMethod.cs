using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsMethod : IContainsJsFunctionDefinition, IFreezable {
        public string Name { get; private set; }

        private bool _frozen;

        private JsFunctionDefinitionExpression _definition;
        public JsFunctionDefinitionExpression Definition {
            get { return _definition; }
            set {
                if (_frozen)
                    throw new InvalidOperationException("Object is frozen");
                _definition = value;
            }
        }

        public JsMethod(string name) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Name = name;
        }

        public void Freeze() {
            _frozen = true;
        }
    }
}
