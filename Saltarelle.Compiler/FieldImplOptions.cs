using System;

namespace Saltarelle.Compiler {
    public class FieldImplOptions {
        public enum ImplType {
            /// <summary>
            /// The field is an instance field. The Name member is valid.
            /// </summary>
            Instance = 0,
            /// <summary>
            /// The field is a static field. The Name member is valid.
            /// </summary>
            Static = 1,
            /// <summary>
            /// The field is not usable from script. No other members are valid.
            /// </summary>
            NotUsableFromScript = 2,
        }

        public ImplType Type { get; private set; }
        private string _name;

        public String Name {
            get {
                if (Type != ImplType.Instance && Type != ImplType.Static)
                    throw new InvalidOperationException();
                return _name;
            }
        }

        private FieldImplOptions() {
        }

        public static FieldImplOptions Instance(string name) {
            return new FieldImplOptions { Type = ImplType.Instance, _name = name };
        }

        public static FieldImplOptions Static(string name) {
            return new FieldImplOptions { Type = ImplType.Static, _name = name };
        }

        public static FieldImplOptions NotUsableFromScript() {
            return new FieldImplOptions { Type = ImplType.NotUsableFromScript };
        }
    }
}