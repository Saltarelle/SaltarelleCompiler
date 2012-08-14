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
	[Imported(IsRealType = true)]
    [ScriptName("Number")]
    public struct Decimal {

        public Decimal(double d) {
        }

        public Decimal(int i) {
        }

        public Decimal(float f) {
        }

        public Decimal(long n) {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale) {
        }

        public string Format(string format) {
            return null;
        }

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

        [ScriptAlias("isFinite")]
        public static bool IsFinite(decimal d) {
            return false;
        }

        [ScriptAlias("isNaN")]
        public static bool IsNaN(decimal d) {
            return false;
        }

        /// <internalonly />
        [ScriptSkip]
        public static implicit operator decimal(byte value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        public static explicit operator short(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        [CLSCompliant(false)]
        public static explicit operator ushort(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        public static explicit operator int(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        [CLSCompliant(false)]
        public static explicit operator uint(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        public static explicit operator long(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        [CLSCompliant(false)]
        public static explicit operator ulong(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        public static explicit operator float(Decimal value) {
            return 0;
        }

        /// <internalonly />
        [ScriptSkip]
        public static explicit operator double(Decimal value) {
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
    }
}
