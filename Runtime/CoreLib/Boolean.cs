// Boolean.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// Equivalent to the Boolean type in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported(ObeysTypeSystem = true)]
    public struct Boolean : IComparable<Boolean>, IEquatable<Boolean> {
		[InlineCode("false")]
		public Boolean(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

        /// <summary>
        /// Enables you to parse a string representation of a boolean value.
        /// </summary>
        /// <param name="s">The string to be parsed.</param>
        /// <returns>The resulting boolean value.</returns>
        public static bool Parse(string s) {
            return false;
        }

	    [InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(bool other) {
		    return 0;
	    }

	    [InlineCode("{$System.Script}.equalsT({this}, {other})")]
	    public bool Equals(bool other) {
		    return false;
	    }
    }
}
