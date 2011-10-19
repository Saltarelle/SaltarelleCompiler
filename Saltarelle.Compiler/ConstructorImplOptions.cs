using System;

namespace Saltarelle.Compiler {
    public class ConstructorImplOptions {
        public enum ImplType {
            /// <summary>
            /// This is the unnamed constructor.
            /// </summary>
            UnnamedConstructor,
            /// <summary>
            /// This is a named constructor.
            /// </summary>
            NamedConstructor,
            /// <summary>
            /// This constructor is implemented as a static method which returns the created instance.
            /// </summary>
            StaticMethod,
            /// <summary>
            /// The constructor is not usable from script. No code is generated for it, and any usages of it will give an error.
            /// </summary>
            NotUsableFromScript,
        }

        private ConstructorImplOptions() {
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

        private string _name;

        /// <summary>
        /// Name of the constructor. Only usable for constructors of type <see cref="ImplType.NamedConstructor"/> and <see cref="ImplType.StaticMethod"/>.
        /// </summary>
        public string Name {
            get {
                if (Type != ImplType.NamedConstructor && Type != ImplType.StaticMethod)
                    throw new InvalidOperationException();
                return _name;
            }
        }

        /// <summary>
        /// Whether code should be generated for this constructor.
        /// </summary>
        public bool GenerateCode { get; private set; }

        public static ConstructorImplOptions Unnamed(bool generateCode = true) {
            return new ConstructorImplOptions { Type = ImplType.UnnamedConstructor, GenerateCode = generateCode };
        }

        public static ConstructorImplOptions Named(string name, bool generateCode = true) {
            return new ConstructorImplOptions { Type = ImplType.NamedConstructor, _name = name, GenerateCode = generateCode };
        }

        public static ConstructorImplOptions StaticMethod(string name, bool generateCode = true) {
            return new ConstructorImplOptions { Type = ImplType.StaticMethod, _name = name, GenerateCode = generateCode };
        }

        public static ConstructorImplOptions NotUsableFromScript() {
            return new ConstructorImplOptions { Type = ImplType.NotUsableFromScript, GenerateCode = false };
        }
    }
}