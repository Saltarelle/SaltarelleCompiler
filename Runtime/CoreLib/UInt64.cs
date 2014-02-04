// UInt64.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The ulong data type which is mapped to the Number type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
	public struct UInt64 : IComparable<UInt64>, IEquatable<UInt64>, IFormattable {
		[InlineCode("0")]
		private UInt64(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineConstant]
		public const ulong MinValue = 0;

		[Obsolete("This number is not representable in Javascript", true)]
		[NonScriptable]
		public const ulong MaxValue = 0;

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[ScriptAlias("parseInt")]
		public static ulong Parse(string s) {
			return 0;
		}

		[ScriptAlias("parseInt")]
		public static ulong Parse(string s, int radix) {
			return 0;
		}

		[InlineCode("{$System.Int32}.tryParse({s}, {result}, 0)")]
		public static bool TryParse(string s, out ulong result) {
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

		[InlineCode("{$System.Script}.netFormatNumber({this}, 'G20')")]
		public new string ToString() {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, {format}, {provider}, 20)")]
		public string ToString(string format, IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, 'G20', {provider})")]
		public string ToString(IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format}, 20)")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(ulong other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(ulong other) {
			return false;
		}
	}
}
