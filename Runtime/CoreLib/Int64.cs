// Int64.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The long data type which is mapped to the Number type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[ScriptName("Int32")]
	[Imported(ObeysTypeSystem = true)]
	public struct Int64 : IComparable<Int64>, IEquatable<Int64>, IFormattable {
		[InlineCode("0")]
		private Int64(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[Obsolete("This number is not representable in Javascript", true)]
		[NonScriptable]
		public const long MinValue = 0;

		[Obsolete("This number is not representable in Javascript", true)]
		[NonScriptable]
		public const long MaxValue = 0;

		[InlineCode("{$System.Script}.formatNumber({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.localeFormatNumber({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[ScriptAlias("parseInt")]
		public static long Parse(string s) {
			return 0;
		}

		[ScriptAlias("parseInt")]
		public static long Parse(string s, int radix) {
			return 0;
		}

		[InlineCode("{$System.Int32}.tryParse({s}, {result})")]
		public static bool TryParse(string s, out long result) {
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

		[InlineCode("{$System.Script}.netFormatNumber({this}, 'G19')")]
		public new string ToString() {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, {format}, {provider}, 19)")]
		public string ToString(string format, IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.netFormatNumber({this}, 'G19', {provider})")]
		public string ToString(IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.formatNumber({this}, {format}, 19)")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(long other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(long other) {
			return false;
		}
	}
}
