// Char.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The char data type which is mapped to the Number type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
	public struct Char : IComparable<Char>, IEquatable<Char>, IFormattable {
		[InlineCode("0")]
		private Char(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineConstant]
		public const char MinValue = '\0';

		[InlineConstant]
		public const char MaxValue = '\xFFFF';

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[InlineCode("{s}.charCodeAt(0)")]
		public static int Parse(string s) {
			return 0;
		}

		[InlineCode("{$System.String}.fromCharCode({ch})")]
		public static explicit operator String(char ch) {
			return null;
		}

		/// <summary>
		/// Converts the value to its string representation.
		/// </summary>
		/// <returns>The string representation of the value.</returns>
		[InlineCode("{$System.String}.fromCharCode({this})")]
		public new string ToString() {
			return null;
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string ToString(string format) {
			return null;
		}

		/// <summary>
		/// Converts the value to its string representation.
		/// </summary>
		/// <returns>The string representation of the value.</returns>
		[InlineCode("{$System.String}.fromCharCode({this})")]
		[PreserveName]
		public new string ToLocaleString() {
			return null;
		}


		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(char other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(char other) {
			return false;
		}

		[InlineCode("{$System.Script}.isLower({ch})")]
		public static bool IsLower(char ch) {
			return false;
		}

		[InlineCode("{$System.Script}.isUpper({ch})")]
		public static bool IsUpper(char ch) {
			return false;
		}
	}
}
