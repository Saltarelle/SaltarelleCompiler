// Byte.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The byte data type which is mapped to the Number type in Javascript.
    /// </summary>
    [ScriptNamespace("ss")]
    [ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
    public struct Byte {
		[InlineCode("0")]
		public Byte(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[CLSCompliant(false)]
		public static byte MinValue { [InlineCode("0")] get { return 0; } }

		[CLSCompliant(false)]
		public static byte MaxValue { [InlineCode("255")] get { return 0; } }

        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptAlias("parseInt")]
        public static byte Parse(string s) {
            return 0;
        }

        [ScriptAlias("parseInt")]
        public static byte Parse(string s, int radix) {
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
