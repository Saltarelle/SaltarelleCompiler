using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.ScriptSemantics {
    public class ConstructorScriptSemantics {
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
            /// This constructor is implemented as a static method which returns the created instance. Constructors of this type may be chained and the C# types may have an inheritance list, but the JS prototype will not be assigned correctly (ie: no instance methods).
            /// Intended for use with JSON types, sealed types where all members are static methods with "this" as their first argument.
            /// </summary>
            StaticMethod,

            /// <summary>
            /// The constructor is implemented as inline code, eg Debugger.Break() => debugger. Can use the parameters {this} (for instance methods), as well as all typenames and argument names in braces (eg. {arg0}, {TArg0}).
            /// The constructor must use all of its arguments, or they risk not being evaluated.
            /// No code will be generated for the constructor.
            /// </summary>
            InlineCode,

			/// <summary>
			/// The constructor call should be treated as JSON, meaning that no code will be generated, and any code that uses this constructor will use inline object syntax ( { a: b, c : d } ) instead of calling it.
			/// No other members are valid.
			/// </summary>
			Json,

            /// <summary>
            /// The constructor is not usable from script. No code is generated for it, and any usages of it will give an error.
            /// </summary>
            NotUsableFromScript,
        }

        private ConstructorScriptSemantics() {
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

        private string _text;

        /// <summary>
        /// Name of the constructor. Only usable for constructors of type <see cref="ImplType.NamedConstructor"/> and <see cref="ImplType.StaticMethod"/>.
        /// </summary>
        public string Name {
            get {
                if (Type != ImplType.NamedConstructor && Type != ImplType.StaticMethod)
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
        /// Whether code should be generated for this constructor.
        /// </summary>
        public bool GenerateCode { get; private set; }

		/// <summary>
		/// Whether the param array to this method is output to script in expanded form. Methods that use this option can only be invoked in expanded form.
		/// </summary>
		public bool ExpandParams { get; set; }

		private ReadOnlyCollection<IMember> _parameterToMemberMap;
		/// <summary>
		/// For JSON constructor, maps the constructor parameter to members. Each element in this array corresponds to the parameter with the same index, and the members have to be properties with field semantics, or fields.
		/// </summary>
		public ReadOnlyCollection<IMember> ParameterToMemberMap {
			get {
				if (Type != ImplType.Json)
					throw new InvalidOperationException();
				return _parameterToMemberMap;
			}
		}

        public static ConstructorScriptSemantics Unnamed(bool generateCode = true, bool expandParams = false) {
            return new ConstructorScriptSemantics { Type = ImplType.UnnamedConstructor, GenerateCode = generateCode, ExpandParams = expandParams };
        }

        public static ConstructorScriptSemantics Named(string name, bool generateCode = true, bool expandParams = false) {
            return new ConstructorScriptSemantics { Type = ImplType.NamedConstructor, _text = name, GenerateCode = generateCode, ExpandParams = expandParams };
        }

        public static ConstructorScriptSemantics StaticMethod(string name, bool generateCode = true, bool expandParams = false) {
            return new ConstructorScriptSemantics { Type = ImplType.StaticMethod, _text = name, GenerateCode = generateCode, ExpandParams = expandParams };
        }

        public static ConstructorScriptSemantics InlineCode(string literalCode) {
            return new ConstructorScriptSemantics { Type = ImplType.InlineCode, _text = literalCode, GenerateCode = false };
        }

        public static ConstructorScriptSemantics NotUsableFromScript() {
            return new ConstructorScriptSemantics { Type = ImplType.NotUsableFromScript, GenerateCode = false };
        }

		public static ConstructorScriptSemantics Json(IEnumerable<IMember> parameterToMemberMap) {
			return new ConstructorScriptSemantics { Type = ImplType.Json, _parameterToMemberMap = parameterToMemberMap.AsReadOnly(), GenerateCode = false };
		}
    }
}