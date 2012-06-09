using System;

namespace Saltarelle.Compiler.ScriptSemantics {
    public class FieldScriptSemantics {
        public enum ImplType {
            /// <summary>
            /// The field is a field in script. The Name member is valid.
            /// </summary>
            Field = 0,
            /// <summary>
            /// The field is not usable from script. No other members are valid.
            /// </summary>
            NotUsableFromScript = 1,
        }

        public ImplType Type { get; private set; }
        private string _name;

        public String Name {
            get {
                if (Type != ImplType.Field)
                    throw new InvalidOperationException();
                return _name;
            }
        }

        private FieldScriptSemantics() {
        }

        public static FieldScriptSemantics Field(string name) {
            return new FieldScriptSemantics { Type = ImplType.Field, _name = name };
        }

        public static FieldScriptSemantics NotUsableFromScript() {
            return new FieldScriptSemantics { Type = ImplType.NotUsableFromScript };
        }
    }
}