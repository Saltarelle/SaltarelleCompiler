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
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Number")]
	public struct Double : IComparable<Double>, IEquatable<Double>, IFormattable {
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

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Script}._netFormatNumber({this}, {format}, {provider})")]
		public string ToString(string format, IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}._netFormatNumber({this}, 'G', {provider})")]
		public string ToString(IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
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

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(double other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(double other) {
			return false;
		}
	}
}
