using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public abstract class JsType : IFreezable {
        public ScopedName Name { get; private set; }

        protected bool Frozen { get; private set; }

        protected JsType(ScopedName name) {
            Require.NotNull(name, "name");
            Name = name;
        }

        public virtual void Freeze() {
            Frozen = true;
        }
    }
}
