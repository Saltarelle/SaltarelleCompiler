// Math.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// Equivalent to the Math object in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported]
    public static class Math {
        [PreserveCase]
        public static readonly double E;

        [PreserveCase]
        public static readonly double LN2;

        [PreserveCase]
        public static readonly double LN10;

        [PreserveCase]
        public static readonly double LOG2E;

        [PreserveCase]
        public static readonly double LOG10E;

        [PreserveCase]
        public static readonly double PI;

        [PreserveCase]
        public static readonly double SQRT1_2;

        [PreserveCase]
        public static readonly double SQRT2;

        public static double Abs(double d) {
            return 0;
        }

        public static int Abs(int i) {
            return 0;
        }

        public static int Abs(long l) {
            return 0;
        }

        public static double Acos(double d) {
            return 0;
        }

        public static double Asin(double d) {
            return 0;
        }

        public static double Atan(double d) {
            return 0;
        }

        public static double Atan2(double x, double y) {
            return 0;
        }

        [ScriptName("ceil")]
		public static double Ceiling(double d) {
            return 0;
        }

        public static double Cos(double d) {
            return 0;
        }

        public static double Exp(double d) {
            return 0;
        }

        public static double Floor(double d) {
            return 0;
        }

        public static double Log(double d) {
            return 0;
        }

        [ExpandParams]
		public static double Max(params double[] numbers) {
            return 0;
        }

        [ExpandParams]
		public static int Max(params int[] numbers) {
            return 0;
        }

        [ExpandParams]
		public static long Max(params long[] numbers) {
            return 0;
        }

        [ExpandParams]
        public static double Min(params double[] numbers) {
            return 0;
        }

        [ExpandParams]
        public static int Min(params int[] numbers) {
            return 0;
        }

        [ExpandParams]
        public static int Min(params long[] numbers) {
            return 0;
        }

        public static double Pow(double baseNumber, double exponent) {
            return 0;
        }

        public static double Random() {
            return 0;
        }

        public static int Round(double d) {
            return 0;
        }

        public static double Sin(double d) {
            return 0;
        }

        public static double Sqrt(double d) {
            return 0;
        }

        public static double Tan(double d) {
            return 0;
        }

        [InlineCode("{d} | 0")]
        public static int Truncate(double d) {
            return 0;
        }
    }
}
