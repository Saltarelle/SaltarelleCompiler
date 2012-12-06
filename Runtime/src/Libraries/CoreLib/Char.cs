// Char.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The char data type which is mapped to the Number type in Javascript.
    /// </summary>
    [ScriptNamespace("ss")]
    [ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
    public struct Char : IHashable<Char> {
		[InlineCode("0")]
		public Char(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		public static char MinValue { [InlineCode("0")] get { return '\0'; } }

		public static char MaxValue { [InlineCode("65535")] get { return '\0'; } }

        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [InlineCode("{s}.charCodeAt(0)")]
        public static int Parse(string s) {
            return 0;
        }

        /// <summary>
        /// Converts the value to its string representation.
        /// </summary>
        /// <returns>The string representation of the value.</returns>
        [InlineCode("{$System.String}.fromCharCode({this})")]
        [PreserveName]
        public new string ToString() {
            return null;
        }

        /// <summary>
        /// Converts the value to its string representation.
        /// </summary>
        /// <returns>The string representation of the value.</returns>
        [InlineCode("{$System.String}.fromCharCode({this})")]
        [PreserveName]
        public new string ToLocaleString() {
            return null;
        }

	    public bool Equals(char other) {
		    return false;
	    }

		public new int GetHashCode() {
			return 0;
		}
    }
}
