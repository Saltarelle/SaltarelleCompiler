using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.Expressions {
    public class JsObjectLiteralProperty {
        public string Name { get; private set; }
        public JsExpression Value { get; private set; }

        public JsObjectLiteralProperty(string name, JsExpression value) {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            Name  = name;
            Value = value;
        }
    }
}
