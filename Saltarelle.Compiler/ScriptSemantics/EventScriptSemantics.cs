using System;

namespace Saltarelle.Compiler.ScriptSemantics {
    public class EventScriptSemantics {
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

        private MethodScriptSemantics _addMethod;
        private MethodScriptSemantics _removeMethod;

        public MethodScriptSemantics AddMethod {
            get {
                if (Type != ImplType.AddAndRemoveMethods)
                    throw new InvalidOperationException();
                return _addMethod;
            }
        }

        public MethodScriptSemantics RemoveMethod {
            get {
                if (Type != ImplType.AddAndRemoveMethods)
                    throw new InvalidOperationException();
                return _removeMethod;
            }
        }

        public static EventScriptSemantics AddAndRemoveMethods(MethodScriptSemantics addMethod, MethodScriptSemantics removeMethod) {
            return new EventScriptSemantics { Type = ImplType.AddAndRemoveMethods, _addMethod = addMethod, _removeMethod = removeMethod };
        }

        public static EventScriptSemantics NotUsableFromScript() {
            return new EventScriptSemantics { Type = ImplType.NotUsableFromScript };
        }
    }
}
