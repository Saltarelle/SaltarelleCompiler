// UInt16.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The ushort data type which is mapped to the Number type in Javascript.
    /// </summary>
    [ScriptNamespace("ss")]
    [ScriptName("Int32")]
    [Imported(IsRealType = true)]
    public struct UInt16 : IHashable<UInt16> {
		[InlineCode("0")]
		public UInt16(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[CLSCompliant(false)]
		public static ushort MinValue { [InlineCode("0")] get { return 0; } }

		[CLSCompliant(false)]
		public static ushort MaxValue { [InlineCode("65535")] get { return 0; } }

        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptAlias("parseInt")]
		[CLSCompliant(false)]
        public static ushort Parse(string s) {
            return 0;
        }

        [ScriptAlias("parseInt")]
		[CLSCompliant(false)]
        public static ushort Parse(string s, int radix) {
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

	    [CLSCompliant(false)]
	    public bool Equals(ushort other) {
		    return false;
	    }

		public new int GetHashCode() {
			return 0;
		}
    }
}
