using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnum : JsType {
        private IList<JsEnumValue> _values;
        public IList<JsEnumValue> Values { get { return _values; } }

        public JsEnum(ScopedName name, bool isPublic) : base(name, isPublic) {
            _values = new List<JsEnumValue>();
        }

        public override void Freeze() {
            base.Freeze();
            _values = _values.AsReadOnly();
        }
    }
}
