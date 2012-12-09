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

        private string _nonVirtualInvocationLiteralCode;
		/// <summary>
        /// Literal code for the method when invoked non-virtually (eg. a base.Method() call), only applicable for type <see cref="ImplType.InlineCode"/>
        /// </summary>
        public string NonVirtualInvocationLiteralCode {
            get {
                if (Type != ImplType.InlineCode)
                    throw new InvalidOperationException();
                return _nonVirtualInvocationLiteralCode;
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
        /// Name of the method that should be generated. If null, no code should be generated for the method
        /// </summary>
        public string GeneratedMethodName { get; private set; }

		/// <summary>
		/// Whether the param array to this method is output to script in expanded form. Methods that use this option can only be invoked in expanded form.
		/// </summary>
		public bool ExpandParams { get; private set; }

		/// <summary>
		/// Whether this method, when used in a foreach, should be treated as being an array enumeration. Only applicable to GetEnumerator() methods.
		/// </summary>
		public bool EnumerateAsArray { get; private set; }

        private MethodScriptSemantics() {
        }

        public static MethodScriptSemantics NormalMethod(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool expandParams = false, bool enumerateAsArray = false) {
            return new MethodScriptSemantics { Type = ImplType.NormalMethod, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GeneratedMethodName = generateCode ? name : null, ExpandParams = expandParams, EnumerateAsArray = enumerateAsArray };
        }

        public static MethodScriptSemantics StaticMethodWithThisAsFirstArgument(string name, bool ignoreGenericArguments = false, bool generateCode = true, bool expandParams = false, bool enumerateAsArray = false) {
            return new MethodScriptSemantics { Type = ImplType.StaticMethodWithThisAsFirstArgument, _text = name, IgnoreGenericArguments = ignoreGenericArguments, GeneratedMethodName = generateCode ? name : null, ExpandParams = expandParams, EnumerateAsArray = enumerateAsArray };
        }

        public static MethodScriptSemantics InlineCode(string literalCode, bool enumerateAsArray = false, string generatedMethodName = null, string nonVirtualInvocationLiteralCode = null) {
            return new MethodScriptSemantics { Type = ImplType.InlineCode, _text = literalCode, IgnoreGenericArguments = true, GeneratedMethodName = generatedMethodName, EnumerateAsArray = enumerateAsArray, _nonVirtualInvocationLiteralCode = nonVirtualInvocationLiteralCode ?? literalCode };
        }

        public static MethodScriptSemantics NativeIndexer() {
            return new MethodScriptSemantics { Type = ImplType.NativeIndexer, GeneratedMethodName = null };
        }

        public static MethodScriptSemantics NativeOperator() {
            return new MethodScriptSemantics { Type = ImplType.NativeOperator, GeneratedMethodName = null};
        }

        public static MethodScriptSemantics NotUsableFromScript() {
            return new MethodScriptSemantics { Type = ImplType.NotUsableFromScript, GeneratedMethodName = null };
        }

		public MethodScriptSemantics WithEnumerateAsArray() {
			return new MethodScriptSemantics { Type = this.Type, _text = this._text, IgnoreGenericArguments = this.IgnoreGenericArguments, GeneratedMethodName = this.GeneratedMethodName, ExpandParams = this.ExpandParams, EnumerateAsArray = true };
		}
    }
}