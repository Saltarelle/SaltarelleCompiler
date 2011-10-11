using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public abstract class JsType {
        public ScopedName Name { get; private set; }
        [Obsolete("Remove, replace with extension method used by naming convention")]
        public bool IsPublic { get; private set; }

        protected JsType(ScopedName name, bool isPublic) {
            Require.NotNull(name, "name");
            Name = name;
            IsPublic = isPublic;
        }

        public virtual void Freeze() {
        }
    }
}
