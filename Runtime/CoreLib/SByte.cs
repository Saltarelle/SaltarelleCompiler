// SByte.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The signed byte data type which is mapped to the Number type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
	public struct SByte : IComparable<SByte>, IEquatable<SByte>, IFormattable {
		[InlineCode("0")]
		public SByte(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineConstant]
		public const sbyte MinValue = -128;

		[InlineConstant]
		public const sbyte MaxValue = 127;

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[ScriptAlias("parseInt")]
		public static sbyte Parse(string s) {
			return 0;
		}

		[ScriptAlias("parseInt")]
		public static sbyte Parse(string s, int radix) {
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

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(sbyte other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(sbyte other) {
			return false;
		}
	}
}
