// Double.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The double data type which is mapped to the Number type in Javascript.
    /// </summary>
    [IgnoreNamespace]
	[Imported(IsRealType = true)]
    [ScriptName("Number")]
    public struct Double : IHashable<Double> {
		[InlineCode("0")]
		public Double(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

        [ScriptName("MAX_VALUE")]
        public const double MaxValue = 0;

        [ScriptName("MIN_VALUE")]
        public const double MinValue = 0;

        [PreserveCase]
        public const double NaN = 0;

        [ScriptName("NEGATIVE_INFINITY")]
        public const double NegativeInfinity = 0;

        [ScriptName("POSITIVE_INFINITY")]
        public const double PositiveInfinity = 0;

        public string Format(string format) {
            return null;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptAlias("parseFloat")]
        public static double Parse(string s) {
            return 0;
        }

        /// <summary>
        /// Returns a string containing the value represented in exponential notation.
        /// </summary>
        /// <returns>The exponential representation</returns>
        public string ToExponential() {
            return null;
        }

        /// <summary>
        /// Returns a string containing the value represented in exponential notation.
        /// </summary>
        /// <param name="fractionDigits">The number of digits after the decimal point from 0 - 20</param>
        /// <returns>The exponential representation</returns>
        public string ToExponential(int fractionDigits) {
            return null;
        }

        /// <summary>
        /// Returns a string representing the value in fixed-point notation.
        /// </summary>
        /// <returns>The fixed-point notation</returns>
        public string ToFixed() {
            return null;
        }

        /// <summary>
        /// Returns a string representing the value in fixed-point notation.
        /// </summary>
        /// <param name="fractionDigits">The number of digits after the decimal point from 0 - 20</param>
        /// <returns>The fixed-point notation</returns>
        public string ToFixed(int fractionDigits) {
            return null;
        }

        /// <summary>
        /// Returns a string containing the value represented either in exponential or
        /// fixed-point notation with a specified number of digits.
        /// </summary>
        /// <returns>The string representation of the value.</returns>
        public string ToPrecision() {
            return null;
        }

        /// <summary>
        /// Returns a string containing the value represented either in exponential or
        /// fixed-point notation with a specified number of digits.
        /// </summary>
        /// <param name="precision">The number of significant digits (in the range 1 to 21)</param>
        /// <returns>The string representation of the value.</returns>
        public string ToPrecision(int precision) {
            return null;
        }

        [ScriptAlias("isFinite")]
        public static bool IsFinite(double d) {
            return false;
        }

        [ScriptAlias("isNaN")]
        public static bool IsNaN(double d) {
            return false;
        }

	    public bool Equals(double other) {
		    return false;
	    }

		public new int GetHashCode() {
			return 0;
		}
    }
}
