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
	[Imported(ObeysTypeSystem = true)]
	public struct UInt16 : IComparable<UInt16>, IEquatable<UInt16>, IFormattable {
		[InlineCode("0")]
		private UInt16(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineConstant]
		public const ushort MinValue = 0;

		[InlineConstant]
		public const ushort MaxValue = 0xFFFF;

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[ScriptAlias("parseInt")]
		public static ushort Parse(string s) {
			return 0;
		}

		[ScriptAlias("parseInt")]
		public static ushort Parse(string s, int radix) {
			return 0;
		}

		[InlineCode("{$System.Int32}.tryParse({s}, {result}, 0, 32768)")]
		public static bool TryParse(string s, out ushort result) {
			result = 0;
			return false;
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
		public int CompareTo(ushort other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(ushort other) {
			return false;
		}
	}
}
