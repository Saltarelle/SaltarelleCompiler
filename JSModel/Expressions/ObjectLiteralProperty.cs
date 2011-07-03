using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.Expressions {
    public class ObjectLiteralProperty {
        public string Name { get; private set; }
        public Expression Value { get; private set; }

        public ObjectLiteralProperty(string name, Expression value) {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            Name  = name;
            Value = value;
        }
    }
}
