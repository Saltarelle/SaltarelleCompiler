using System;

namespace Saltarelle.Compiler {
    public class FieldImplOptions {
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

        private FieldImplOptions() {
        }

        public static FieldImplOptions Field(string name) {
            return new FieldImplOptions { Type = ImplType.Field, _name = name };
        }

        public static FieldImplOptions NotUsableFromScript() {
            return new FieldImplOptions { Type = ImplType.NotUsableFromScript };
        }
    }
}