// Char.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The char data type which is mapped to the Number type in Javascript.
    /// </summary>
    [IgnoreNamespace]
	[Imported(IsRealType = true)]
    [ScriptName("Int32")]
    public struct Char {
        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptAlias("{$System.String}.charCodeAt(0)")]
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

        [InlineCode("{$System.String}.fromCharCode({ch})")]
        public static implicit operator String(char ch) {
            return null;
        }
    }
}
