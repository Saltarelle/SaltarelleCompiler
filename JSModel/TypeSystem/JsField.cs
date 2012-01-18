using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsField {
        public string Name { get; private set; }

        /// <summary>
        /// Initializer for the field. Must be the default value for the field's type if no initializer was given.
        /// </summary>
        public JsExpression Initializer { get; private set; }

        public JsField(string name, JsExpression initializer) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Require.NotNull(initializer, "initializer");
            Name = name;
            Initializer = initializer;
        }
    }
}
