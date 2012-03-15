using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel {
	public static class Require {
        [DebuggerStepThrough]
        public static void NotNull<T>(T arg, string name) where T : class {
            if (arg == null) throw new ArgumentNullException(name);
        }

        [DebuggerStepThrough]
        public static void ValidJavaScriptIdentifier(string arg, string name, bool allowNull = false) {
            if (!allowNull && arg == null) throw new ArgumentNullException(name);
            if (arg != null && !arg.IsValidJavaScriptIdentifier()) throw new ArgumentException(name);
        }

        [DebuggerStepThrough]
        public static void ValidJavaScriptNestedIdentifier(string arg, string name, bool allowNull = false) {
            if (arg == null) {
                if (!allowNull)
                    throw new ArgumentNullException(name);
                return;
            }
            if (arg == "") throw new ArgumentException(name);
            if (arg.Split('.').Any(p => !p.IsValidJavaScriptIdentifier())) throw new ArgumentException(name);
        }
    }
}
