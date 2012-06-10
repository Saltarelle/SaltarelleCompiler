using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public abstract class JsType : IFreezable {
        public string Name { get; private set; }

        protected bool Frozen { get; private set; }

        protected JsType(string name) {
            Require.ValidJavaScriptNestedIdentifier(name, "name");
            Name = name;
        }

        public virtual void Freeze() {
            Frozen = true;
        }
    }
}
