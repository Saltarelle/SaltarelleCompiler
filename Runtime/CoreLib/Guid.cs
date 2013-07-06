
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The Guid data type which is mapped to the string type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[ScriptName("Guid")]
	[Imported(ObeysTypeSystem = true)]
	public struct Guid : IEquatable<Guid>, IComparable<Guid> {
		[InlineCode("{$System.Guid}.empty")]
		private Guid(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[InlineCode("{$System.Guid}.parse({uuid})")]
		public Guid(string uuid) { 
		}

		[InlineCode("{$System.Guid}.fromBytes({b})")]
		public Guid(byte[] b) {
		}

		[InlineCode("{$System.Guid}.fromBytes([({a} >> 24) & 0xff, ({a} >> 16) & 0xff, ({a} >> 8) & 0xff, {a} & 0xff, ({b} >> 8) & 0xff, {b} & 0xff, ({c} >> 8) & 0xff, {c} & 0xff].concat({d}))")]
		public Guid(int a, short b, short c, byte[] d) {
		}

		[InlineCode("{$System.Guid}.fromBytes([({a} >> 24) & 0xff, ({a} >> 16) & 0xff, ({a} >> 8) & 0xff, {a} & 0xff, ({b} >> 8) & 0xff, {b} & 0xff, ({c} >> 8) & 0xff, {c} & 0xff, {d} & 0xff, {e} & 0xff, {f} & 0xff, {g} & 0xff, {h} & 0xff, {i} & 0xff, {j} & 0xff, {k} & 0xff])")]
		public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) {
		}

		[InlineCode("{$System.Guid}.fromBytes([({a} >> 24) & 0xff, ({a} >> 16) & 0xff, ({a} >> 8) & 0xff, {a} & 0xff, ({b} >> 8) & 0xff, {b} & 0xff, ({c} >> 8) & 0xff, {c} & 0xff, {d} & 0xff, {e} & 0xff, {f} & 0xff, {g} & 0xff, {h} & 0xff, {i} & 0xff, {j} & 0xff, {k} & 0xff])")]
		public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) {
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(Guid other) {
			return false;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(Guid other) {
			return 0;
		}

		[InlineCode("{$System.Guid}.format({this}, {format})")]
		public string ToString(string format) {
			return null;
		}

		[InlineCode("{$System.Guid}.getBytes({this})")]
		public byte[] ToByteArray() {
			return null;
		}

		public static readonly Guid Empty = new Guid();

		public static Guid Parse(string input) {
			return default(Guid);
		}

		[ScriptName("parse")]
		public static Guid ParseExact(string input, string format) {
			return default(Guid);
		}

		[InlineCode("{$System.Guid}.tryParse({input}, null, {result})")]
		public static bool TryParse(string input, out Guid result) {
			result = default(Guid);
			return false;
		}

		[ScriptName("tryParse")]
		public static bool TryParseExact(string input, string format, out Guid result) {
			result = default(Guid);
			return false;
		}

		public static Guid NewGuid() {
			return default(Guid);
		}

		[IntrinsicOperator]
		public static bool operator==(Guid a, Guid b) {
			return false;
		}

		[IntrinsicOperator]
		public static bool operator!=(Guid a, Guid b) {
			return false;
		}

/*
		[InlineCode("{$System.Guid}.newGuid()")]
		public static Guid NewGuid() {
			return default(Guid);
		}

		[InlineCode("{$System.Guid}.parse({uuid})")]
		public static Guid Parse(string uuid) {
			return Empty;
		}

		[InlineCode("{$System.Guid}.uuid)")]
		public string ToString(string format) {
			return null;
		}*/
	}
}
