using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnum : JsType {
        public ReadOnlyCollection<EnumValue> Values { get; private set; }

        public JsEnum(ScopedName name, IEnumerable<EnumValue> values) : base(name) {
            Values = values.AsReadOnly();
        }
    }
}
