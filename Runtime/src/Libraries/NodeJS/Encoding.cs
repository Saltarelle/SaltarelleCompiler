using System.Runtime.CompilerServices;

namespace NodeJS {
	[NamedValues]
	[Imported]
	public enum Encoding {
		Ascii,
		Utf8,
		Utf16le,
		Base64,
		Binary,
		Hex
	}
}