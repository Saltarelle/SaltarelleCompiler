using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler {
    public class EventImplOptions {
        public enum ImplType {
            /// <summary>
            /// The event uses add and remove methods. The AddMethod and RemoveMethod members is valid.
            /// </summary>
            AddAndRemoveMethods = 0,
            /// <summary>
            /// The event is not usable from script. No other members are valid.
            /// </summary>
            NotUsableFromScript = 2,
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

        private MethodImplOptions _addMethod;
        private MethodImplOptions _removeMethod;

        public MethodImplOptions AddMethod {
            get {
                if (Type != ImplType.AddAndRemoveMethods)
                    throw new InvalidOperationException();
                return _addMethod;
            }
        }

        public MethodImplOptions RemoveMethod {
            get {
                if (Type != ImplType.AddAndRemoveMethods)
                    throw new InvalidOperationException();
                return _removeMethod;
            }
        }

        public static EventImplOptions AddAndRemoveMethods(MethodImplOptions addMethod, MethodImplOptions removeMethod) {
            return new EventImplOptions { Type = ImplType.AddAndRemoveMethods, _addMethod = addMethod, _removeMethod = removeMethod };
        }

        public static EventImplOptions NotUsableFromScript() {
            return new EventImplOptions { Type = ImplType.NotUsableFromScript };
        }
    }
}
