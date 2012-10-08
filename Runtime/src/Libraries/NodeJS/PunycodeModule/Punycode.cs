using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.PunycodeModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("punycode")]
	public static class Punycode {
		public static string Decode(string text) { return null; }

		public static string Encode(string text) { return null; }

		public static string ToUnicode(string domain) { return null; }

		[ScriptName("toASCII")]
		public static string ToAscii(string domain) { return null; }

		[IntrinsicProperty]
		public static string Version { get; private set; }
	}

	[Imported]
	[ModuleName("punycode")]
	[ScriptName("ucs2")]
	public static class Ucs2 {
		public static int[] Decode(string text) { return null; }

		public static string Encode(int[] codePoints) { return null; }
	}
}
