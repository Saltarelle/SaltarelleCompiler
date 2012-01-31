using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsField : IFreezable {
        public string Name { get; private set; }

        private JsExpression _initializer;
        private bool _frozen;

        /// <summary>
        /// Initializer for the field. Must be the default value for the field's type if no initializer was given.
        /// </summary>
        public JsExpression Initializer {
            get { return _initializer; }
            set {
                if (_frozen)
                    throw new InvalidOperationException("Object is frozen");
                _initializer = value;
            }
        }

        public JsField(string name, JsExpression initializer = null) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Name = name;
            Initializer = initializer;
        }

        public void Freeze() {
            _frozen = true;
        }
    }
}
