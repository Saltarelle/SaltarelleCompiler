// UInt32.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The uint data type which is mapped to the Number type in Javascript.
    /// </summary>
    [ScriptNamespace("ss")]
	[ScriptName("Int32")]
    [Imported(IsRealType = true)]
    public struct UInt32 {

        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptAlias("parseInt")]
		[CLSCompliant(false)]
        public static uint Parse(string s) {
            return 0;
        }

        [ScriptAlias("parseInt")]
		[CLSCompliant(false)]
        public static uint Parse(string s, int radix) {
            return 0;
        }

        /// <summary>
        /// Converts the value to its string representation.
        /// </summary>
        /// <param name="radix">The radix used in the conversion (eg. 10 for decimal, 16 for hexadecimal)</param>
        /// <returns>The string representation of the value.</returns>
        public string ToString(int radix) {
            return null;
        }
    }
}
