using System;

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
        }

        private string _text;

        public string Name {
            get {
                if (Type != ImplType.InstanceMethod && Type != ImplType.StaticMethod && Type != ImplType.InstanceMethodOnFirstArgument)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        public string LiteralCode {
            get {
                if (Type != ImplType.InlineCode)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        public bool IgnoreGenericArguments { get; private set; }

        public ImplType Type { get; private set; }

        public static MethodImplOptions InstanceMethod(string name, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.InstanceMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments };
        }

        public static MethodImplOptions StaticMethod(string name, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.StaticMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments };
        }

        public static MethodImplOptions InlineCode(string literalCode, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.InlineCode, _text = literalCode, IgnoreGenericArguments = ignoreGenericArguments };
        }

        public static MethodImplOptions InstanceMethodOnFirstArgument(string name, bool ignoreGenericArguments = false) {
            return new MethodImplOptions { Type = ImplType.InstanceMethodOnFirstArgument, _text = name, IgnoreGenericArguments = ignoreGenericArguments };
        }
    }
}