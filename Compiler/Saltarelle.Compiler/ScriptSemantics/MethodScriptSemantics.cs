using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.ScriptSemantics {
    public class MethodScriptSemantics {
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
            /// The method is implemented as inline code, eg Debugger.Break() => debugger. Can use the parameters {this} (for instance methods), as well as all typenames and argument names in braces (eg. {arg0}, {TArg0}).
            /// If a parameter name is preceeded by an @ sign, {@arg0}, that argument must be a literal string during invocation, and the supplied string will be inserted as an identifier into the script (eg '{this}.set_{@arg0}({arg1})' can transform the call 'c.F("MyProp", v)' to 'c.set_MyProp(v)'.
            /// If a parameter name is preceeded by an asterisk {*arg}, that parameter must be a param array, and all invocations of the method must use the expanded invocation form. The actual value supplied for the param array will be inserted into the call.
            /// The format string can also use identifiers starting with a dollar {$Namespace.Name} to construct type references. The name must be the fully qualified type name in this case.
            /// The method must use all of its arguments, or they risk not being evaluated.
            /// No code will be generated for the method.
            /// </summary>
            InlineCode,

            /// <summary>
            /// The method is implemented as a native indexer. It must take exactly one argument (mostly useful for getters/setters for indexed properties).
            /// </summary>
            NativeIndexer,

            /// <summary>
            /// The method is implemented as a native operator. Can only be used with (non-conversion) operator methods.
            /// </summary>
			NativeOperator,

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
                if (Type != ImplType.NormalMethod && Type != ImplType.StaticMethodWithThisAsFirstArgument)
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
		/// Whether the param array to this method is output to script in expanded form. Methods that use this option can only be invoked in expanded form.
		/// </summary>
		public bool ExpandParams { get; set; }

        private MethodScriptSemantics() {
        }

        public static MethodScriptSemantics NormalMethod(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool expandParams = false) {
            return new MethodScriptSemantics { Type = ImplType.NormalMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = generateCode, ExpandParams = expandParams };
        }

        public static MethodScriptSemantics StaticMethodWithThisAsFirstArgument(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool expandParams = false) {
            return new MethodScriptSemantics { Type = ImplType.StaticMethodWithThisAsFirstArgument, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GenerateCode = generateCode, ExpandParams = expandParams };
        }

        public static MethodScriptSemantics InlineCode(string literalCode) {
            return new MethodScriptSemantics { Type = ImplType.InlineCode, _text = literalCode, IgnoreGenericArguments = true, GenerateCode = false };
        }

        public static MethodScriptSemantics NativeIndexer() {
            return new MethodScriptSemantics { Type = ImplType.NativeIndexer, GenerateCode = false };
        }

        public static MethodScriptSemantics NativeOperator() {
            return new MethodScriptSemantics { Type = ImplType.NativeOperator, GenerateCode = false };
        }

        public static MethodScriptSemantics NotUsableFromScript() {
            return new MethodScriptSemantics { Type = ImplType.NotUsableFromScript, GenerateCode = false };
        }
    }
}