using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsConstructor : IContainsJsFunctionDefinition, IFreezable {
        /// <summary>
        /// Name of the constructor, or null if the constructor is the unnamed constructor.
        /// </summary>
        public string Name { get; private set; }

        private bool _frozen;

        private JsFunctionDefinitionExpression _definition;
        public JsFunctionDefinitionExpression Definition {
            get { return _definition; }
            set {
                if (_frozen)
                    throw new InvalidOperationException("Object is frozen");
                else if (_definition != null)
                    throw new InvalidOperationException("Can only set definition once");
                _definition = value;
            }
        }

        public JsConstructor(string name) {
            Require.ValidJavaScriptIdentifier(name, "name", allowNull: true);
            Name = name;
        }

        public void Freeze() {
            _frozen = true;
        }
    }
}