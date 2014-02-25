// Decimal.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The decimal data type which is mapped to the Number type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Number")]
    public struct Decimal : IComparable<Decimal>, IEquatable<Decimal>, IFormattable, ILocaleFormattable {
		[Obsolete("This number is not representable in Javascript", true)]
		[NonScriptable]
		public const decimal MaxValue = 79228162514264337593543950335m;

		[Obsolete("This number is not representable in Javascript", true)]
		[NonScriptable]
		public const decimal MinValue = -79228162514264337593543950335m;

		[InlineConstant]
		public const decimal Zero = 0;
		[InlineConstant]
		public const decimal One = 1;
		[InlineConstant]
		public const decimal MinusOne = -1;

		[InlineCode("0")]
		private Decimal(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineCode("{d}")]
		public Decimal(double d) {
		}

		[InlineCode("{i}")]
		public Decimal(int i) {
		}

		[InlineCode("{i}")]
		public Decimal(uint i) {
		}

		[InlineCode("{f}")]
		public Decimal(float f) {
		}

		[InlineCode("{n}")]
		public Decimal(long n) {
		}

		[InlineCode("{n}")]
		public Decimal(ulong n) {
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[NonScriptable]
		public Decimal(int lo, int mid, int hi, bool isNegative, byte scale) {
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		/// <summary>
		/// Converts the value to its string representation.
		/// </summary>
		/// <param name="radix">The radix used in the conversion (eg. 10 for decimal, 16 for hexadecimal)</param>
		/// <returns>The string representation of the value.</returns>
		public string ToString(int radix) {
			return null;
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, {format}, {provider})")]
		public string ToString(string format, IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, 'G', {provider})")]
		public string ToString(IFormatProvider provider) {
			return null;
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
		/// Returns a string containing the number represented either in exponential or
		/// fixed-point notation with a specified number of digits.
		/// </summary>
		/// <returns>The string representation of the value.</returns>
		public string ToPrecision() {
			return null;
		}

		/// <summary>
		/// Returns a string containing the number represented either in exponential or
		/// fixed-point notation with a specified number of digits.
		/// </summary>
		/// <param name="precision">The number of significant digits (in the range 1 to 21)</param>
		/// <returns>The string representation of the value.</returns>
		public string ToPrecision(int precision) {
			return null;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(byte value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(sbyte value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(short value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(ushort value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(char value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(int value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(uint value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(long value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static implicit operator decimal(ulong value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator decimal(float value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator decimal(double value) {
			return 0;
		}


		/// <internalonly />
		[ScriptSkip]
		public static explicit operator byte(decimal value) {
		  return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator sbyte(decimal value) {
		  return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator char(decimal value) {
			return '\0';
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator short(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator ushort(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator int(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator uint(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator long(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator ulong(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator float(decimal value) {
			return 0;
		}

		/// <internalonly />
		[ScriptSkip]
		public static explicit operator double(decimal value) {
			return 0;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator +(decimal d) {
			return d;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator -(decimal d) {
			return d;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator +(decimal d1, decimal d2) {
			return d1;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator -(decimal d1, decimal d2) {
			return d1;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator ++(decimal d) {
			return d;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator --(decimal d) {
			return d;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator *(decimal d1, decimal d2) {
			return d1;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator /(decimal d1, decimal d2) {
			return d1;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static decimal operator %(decimal d1, decimal d2) {
			return d1;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator ==(decimal d1, decimal d2) {
			return false;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator !=(decimal d1, decimal d2) {
			return false;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator >(decimal d1, decimal d2) {
			return false;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator >=(decimal d1, decimal d2) {
			return false;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator <(decimal d1, decimal d2) {
			return false;
		}

		/// <internalonly />
		[IntrinsicOperator]
		public static bool operator <=(decimal d1, decimal d2) {
			return false;
		}

		[InlineCode("{d1} + {d2}")]
		public static decimal Add(decimal d1, decimal d2) {
			return 0;
		}

		[InlineCode("{$System.Math}.ceil({d})")]
		public static decimal Ceiling(decimal d) {
			return 0;
		}

		[InlineCode("{d1} / {d2}")]
		public static decimal Divide(decimal d1, decimal d2) {
			return 0;
		}

		[InlineCode("{$System.Math}.floor({d})")]
		public static decimal Floor(decimal d) {
			return 0;
		}

		[InlineCode("{d1} % {d2}")]
		public static decimal Remainder(decimal d1, decimal d2) {
			return 0;
		}

		[InlineCode("{d1} * {d2}")]
		public static decimal Multiply(decimal d1, decimal d2) {
			return 0;
		}

		[InlineCode("-{d}")]
		public static decimal Negate(decimal d) {
			return 0;
		}

		[InlineCode("{$System.Math}.round({d})")]
		public static decimal Round(decimal d) {
			return 0;
		}

		[InlineCode("{d1} - {d2}")]
		public static decimal Subtract(decimal d1, decimal d2) {
			return 0;
		}


		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(decimal other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(decimal other) {
			return false;
		}
	}
}
