using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsEnum : JsType {
        private IList<JsEnumValue> _values;
        public IList<JsEnumValue> Values { get { return _values; } }

        public JsEnum(ITypeDefinition csharpTypeDefinition, string name, IEnumerable<JsEnumValue> values) : base(csharpTypeDefinition, name) {
            _values = values.AsReadOnly();
        }

        public override void Freeze() {
            base.Freeze();
            _values = _values.AsReadOnly();
        }
    }
}
