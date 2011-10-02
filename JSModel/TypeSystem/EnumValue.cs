using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class EnumValue {
        public string Name { get; private set; }
        public long Value { get; private set; }

        public EnumValue(string name, long value) {
            Name = name;
            Value = value;
        }
    }
}
