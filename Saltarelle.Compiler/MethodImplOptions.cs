using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
    public class MethodImplOptions {
        public enum ImplType {
            /// <summary>
            /// The method is a normal instance method.
            /// </summary>
            InstanceMethod,

            /// <summary>
            /// The method is a normal static method.
            /// </summary>
            StaticMethod,

            /// <summary>
            /// The method is implemented as inline code, eg Debugger.Break() => debugger. Can use the parameters {this} (for static methods), as well as all typenames and argument names in braces (eg. {arg0}, {TArg0}).
            /// The method must use all of its arguments, or they risk not being evaluated.
            /// No code will be generated for the method.
            /// </summary>
            InlineCode,

            /// <summary>
            /// The method is an instance method to be invoked on its first argument. Eg. JQueryDialog.Dialog(this JQuery q, string action) => q.dialog(action).
            /// No code will be generated for the method.
            /// </summary>
            InstanceMethodOnFirstArgument,

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
                if (Type != ImplType.InstanceMethod && Type != ImplType.StaticMethod && Type != ImplType.InstanceMethodOnFirstArgument)
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

        /// <summary>
        /// If true, "this" (the object on which the method is being invoked) will be added as the first argument to the method.
        /// </summary>
        public bool AddThisAsFirstArgument { get; private set; }

        private ReadOnlyCollection<string> _additionalNames;
        public ReadOnlyCollection<string> AdditionalNames {
            get {
                if (Type != ImplType.InstanceMethod && Type != ImplType.StaticMethod)
                    throw new InvalidOperationException();
                return _additionalNames;
            }
        }

        private MethodImplOptions() {
        }

        public static MethodImplOptions InstanceMethod(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool addThisAsFirstArgument = false, IEnumerable<string> additionalNames = null) {
            return new MethodImplOptions { Type = ImplType.InstanceMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = generateCode, AddThisAsFirstArgument = addThisAsFirstArgument, _additionalNames = additionalNames.AsReadOnly() };
        }

        public static MethodImplOptions StaticMethod(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool addThisAsFirstArgument = false, IEnumerable<string> additionalNames = null) {
            return new MethodImplOptions { Type = ImplType.StaticMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = generateCode, AddThisAsFirstArgument = addThisAsFirstArgument, _additionalNames = additionalNames.AsReadOnly() };
        }

        public static MethodImplOptions InlineCode(string literalCode, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.InlineCode, _text = literalCode, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = false };
        }

        public static MethodImplOptions InstanceMethodOnFirstArgument(string name, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.InstanceMethodOnFirstArgument, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = false };
        }

        public static MethodImplOptions NotUsableFromScript() {
            return new MethodImplOptions { Type = ImplType.NotUsableFromScript, GenerateCode = false };
        }
    }
}