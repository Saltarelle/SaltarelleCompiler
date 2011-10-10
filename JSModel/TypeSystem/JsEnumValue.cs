using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnumValue {
        public string Name { get; private set; }
        public long Value { get; private set; }

        public JsEnumValue(string name, long value) {
            Name = name;
            Value = value;
        }
    }
}
