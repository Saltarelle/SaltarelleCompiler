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

		public static int Abs(short l)
		{
			return 0;
		}

		public static int Abs(sbyte l)
		{
			return 0;
		}

		public static int Abs(float l)
		{
			return 0;
		}

		public static int Abs(decimal l)
		{
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
		public static double Ceiling(decimal d)
		{
			return 0;
		}

		[ScriptName("ceil")]
		public static double Ceiling(double d) {
			return 0;
		}

		public static double Cos(double d) {
			return 0;
		}

		[InlineCode("(Math.exp({d}) + Math.exp({d} * -1))/2")]
		public static double Cosh(double d)
		{
			return 0;
		}

		[InlineCode("(Math.exp({d}) - Math.exp({d} * -1))/2")]
		public static double Sinh(double d)
		{
			return 0;
		}

		[InlineCode("(Math.exp({d}) - Math.exp({d} * -1)) / (Math.exp({d}) + Math.exp({d} * -1))")]
		public static double Tanh(double d)
		{
			return 0;
		}

		public static double Exp(double d) {
			return 0;
		}

		public static double Floor(decimal d)
		{
			return 0;
		}

		public static double Floor(double d) {
			return 0;
		}

		public static double Log(double d) {
			return 0;
		}

		[InlineCode("Math.log({d}) / Math.log({newBase})")]
		public static double Log(double d, double newBase)
		{
			return 0;
		}

		[InlineCode("Math.log({d}) / Math.log(10)")]
		public static double Log10(double d)
		{
			return 0;
		}

		public static int Max(byte a, byte b)
		{
			return 0;
		}

		public static int Max(decimal a, decimal b)
		{
			return 0;
		}

		public static double Max(double a, double b) {
			return 0;
		}

		public static int Max(short a, short b)
		{
			return 0;
		}

		public static int Max(int a, int b) {
			return 0;
		}

		public static long Max(long a, long b) {
			return 0;
		}

		public static int Max(sbyte a, sbyte b)
		{
			return 0;
		}

		public static int Max(float a, float b)
		{
			return 0;
		}

		public static int Max(ushort a, ushort b)
		{
			return 0;
		}

		public static int Max(uint a, uint b)
		{
			return 0;
		}

		public static int Max(ulong a, ulong b)
		{
			return 0;
		}

		public static int Min(byte a, byte b)
		{
			return 0;
		}

		public static int Min(decimal a, decimal b)
		{
			return 0;
		}

		public static double Min(double a, double b)
		{
			return 0;
		}

		public static int Min(short a, short b)
		{
			return 0;
		}

		public static int Min(int a, int b)
		{
			return 0;
		}

		public static long Min(long a, long b)
		{
			return 0;
		}

		public static int Min(sbyte a, sbyte b)
		{
			return 0;
		}

		public static int Min(float a, float b)
		{
			return 0;
		}

		public static int Min(ushort a, ushort b)
		{
			return 0;
		}

		public static int Min(uint a, uint b)
		{
			return 0;
		}

		public static int Min(ulong a, ulong b)
		{
			return 0;
		}

		public static double Pow(double baseNumber, double exponent) {
			return 0;
		}

		public static double Random() {
			return 0;
		}

		public static decimal Round(decimal d)
		{
			return 0;
		}

		public static double Round(double d) {
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigits({d}, {digits})")]
		public static decimal Round(decimal d, int digits)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigits({d}, {digits})")]
		public static double Round(double d, int digits)
		{
			return 0;
		}

		[InlineCode("{x} - ({y} * Math.round({x} / {y}))")]
		public static double IEEERemainder(double x, double y)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigitsAndMidpoint({d}, 0, {method})")]
		public static decimal Round(decimal d, MidpointRounding method)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigitsAndMidpoint({d}, 0, {method})")]
		public static double Round(double d, MidpointRounding method)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigitsAndMidpoint({d}, {digits}, {method})")]
		public static decimal Round(decimal d, int digits, MidpointRounding method)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.roundWithDigitsAndMidpoint({d}, {digits}, {method})")]
		public static double Round(double d, int digits, MidpointRounding method)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.divRem({a}, {b}, {result})")]
		public static int DivRem(int a, int b, out int result)
		{
			result = 0;
			return 0;
		}

		[InlineCode("{$System.Script}.divRem({a}, {b}, {result})")]
		public static long DivRem(long a, long b, out long result)
		{
			result = 0;
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(decimal value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(double value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(short value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(int value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(long value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(sbyte value)
		{
			return 0;
		}

		[InlineCode("{value} > 0 ? 1 : {value} < 0 ? -1 : 0")]
		public static int Sign(float value)
		{
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

		[InlineCode("{d} | 0")]
		public static int Truncate(decimal d)
		{
			return 0;
		}
		
		[InlineCode("{a} * {b}")]
		public static long BigMul(int a, int b)
		{
			return 0;
		}
	}
}
