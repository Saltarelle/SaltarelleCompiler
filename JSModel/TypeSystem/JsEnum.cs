using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnum : JsType {
        private IList<EnumValue> _values;
        public IList<EnumValue> Values { get { return _values; } }

        public JsEnum(ScopedName name) : base(name) {
            _values = new List<EnumValue>();
        }

        public override void Freeze() {
            base.Freeze();
            _values = _values.AsReadOnly();
        }
    }
}
