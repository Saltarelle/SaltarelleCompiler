using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnum : JsType {
        private IList<JsEnumValue> _values;
        public IList<JsEnumValue> Values { get { return _values; } }

        public JsEnum(ScopedName name, IEnumerable<JsEnumValue> values) : base(name) {
            _values = values.AsReadOnly();
        }

        public override void Freeze() {
            base.Freeze();
            _values = _values.AsReadOnly();
        }
    }
}
