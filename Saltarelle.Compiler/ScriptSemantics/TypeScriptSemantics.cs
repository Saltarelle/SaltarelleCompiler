using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.ScriptSemantics {
	public class TypeScriptSemantics {
        public enum ImplType {
            /// <summary>
            /// Normal type.
            /// </summary>
            NormalType,

            /// <summary>
            /// This type is imported. All members can be used, but no code will be generated.
            /// </summary>
            Imported,

            /// <summary>
            /// This type cannot be used from script. No code is generated, and any usages of it will result in an error.
            /// However, its members might still be used (but care must be taken to specify attributes on the members to ensure that they work even when the type does not exist.
            /// </summary>
            NotUsableFromScript,
        }

        private TypeScriptSemantics() {
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

		private string _name;
		/// <summary>
		/// (Fully qualified) name of the type
		/// </summary>
		public string Name {
			get {
				if (Type != ImplType.NormalType && Type != ImplType.Imported)
					throw new InvalidOperationException();
				return _name;
			}
			private set { _name = value; }
		}

		private bool _ignoreGenericArguments;
		/// <summary>
		/// (Fully qualified) name of the type
		/// </summary>
		public bool IgnoreGenericArguments {
			get {
				if (Type != ImplType.NormalType && Type != ImplType.Imported)
					throw new InvalidOperationException();
				return _ignoreGenericArguments;
			}
			private set { _ignoreGenericArguments = value; }
		}

		public static TypeScriptSemantics NormalType(string name, bool ignoreGenericArguments = false) {
			return new TypeScriptSemantics { Type = ImplType.NormalType, Name = name, IgnoreGenericArguments = ignoreGenericArguments };
		}

		public static TypeScriptSemantics Imported(string name, bool ignoreGenericArguments = false) {
			return new TypeScriptSemantics { Type = ImplType.Imported, Name = name, IgnoreGenericArguments = ignoreGenericArguments };
		}

		public static TypeScriptSemantics NotUsableFromScript() {
			return new TypeScriptSemantics { Type = ImplType.NotUsableFromScript };
		}
	}
}
