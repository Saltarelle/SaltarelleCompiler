using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
    public class MethodImplOptions {
        public enum ImplType {
            /// <summary>
            /// The method is a normal method, instance or static depending on how it is declared in C#.
            /// </summary>
            NormalMethod,

            /// <summary>
            /// The method (instance method in C#) is implemented as a static method, with the instance being added as the first argument.
            /// </summary>
            StaticMethodWithThisAsFirstArgument,

            /// <summary>
            /// The method (a static method in C#) is an instance method to be invoked on its first argument. Eg. JQueryDialog.Dialog(this JQuery q, string action) => q.dialog(action). Generic arguments are ignored.
            /// No code will be generated for the method.
            /// </summary>
            InstanceMethodOnFirstArgument,

            /// <summary>
            /// The method is implemented as inline code, eg Debugger.Break() => debugger. Can use the parameters {this} (for instance methods), as well as all typenames and argument names in braces (eg. {arg0}, {TArg0}).
            /// The method must use all of its arguments, or they risk not being evaluated.
            /// No code will be generated for the method.
            /// </summary>
            InlineCode,

            /// <summary>
            /// The method is implemented as a native indexer. It must take exactly one argument (mostly useful for getters/setters for indexed properties).
            /// </summary>
            NativeIndexer,

            /// <summary>
            /// The method can not be used from script. No code is generated, and any usage of it will give an error.
            /// </summary>
            NotUsableFromScript,
        }

        private string _text;

        /// <summary>
        /// Name of the method, where applicable.
        /// </summary>
        public string Name {
            get {
                if (Type != ImplType.NormalMethod && Type != ImplType.StaticMethodWithThisAsFirstArgument && Type != ImplType.InstanceMethodOnFirstArgument)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        /// <summary>
        /// Literal code for the method, only applicable for type <see cref="ImplType.InlineCode"/>
        /// </summary>
        public string LiteralCode {
            get {
                if (Type != ImplType.InlineCode)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

        /// <summary>
        /// Completely ignore any generic (type) arguments to this method.
        /// </summary>
        public bool IgnoreGenericArguments { get; private set; }

        /// <summary>
        /// Whether code should be added for this method. This can be false even though the method is usable from script.
        /// </summary>
        public bool GenerateCode { get; private set; }

        private ReadOnlyCollection<string> _additionalNames;
        public ReadOnlyCollection<string> AdditionalNames {
            get {
                if (Type != ImplType.NormalMethod)
                    throw new InvalidOperationException();
                return _additionalNames;
            }
        }

        private MethodImplOptions() {
        }

        public static MethodImplOptions NormalMethod(string name, bool ignoreGenericArguments = false, bool generateCode = true, IEnumerable<string> additionalNames = null) {
            return new MethodImplOptions { Type = ImplType.NormalMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = generateCode, _additionalNames = additionalNames.AsReadOnly() };
        }

        public static MethodImplOptions StaticMethodWithThisAsFirstArgument(string name, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.StaticMethodWithThisAsFirstArgument, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = false };
        }

        public static MethodImplOptions InstanceMethodOnFirstArgument(string name) {
            return new MethodImplOptions { Type = ImplType.InstanceMethodOnFirstArgument, _text = name, IgnoreGenericArguments = true, GenerateCode = false };
        }

        public static MethodImplOptions InlineCode(string literalCode) {
            return new MethodImplOptions { Type = ImplType.InlineCode, _text = literalCode, IgnoreGenericArguments = true, GenerateCode = false };
        }

        public static MethodImplOptions NativeIndexer() {
            return new MethodImplOptions { Type = ImplType.NativeIndexer, GenerateCode = false };
        }

        public static MethodImplOptions NotUsableFromScript() {
            return new MethodImplOptions { Type = ImplType.NotUsableFromScript, GenerateCode = false };
        }
    }
}