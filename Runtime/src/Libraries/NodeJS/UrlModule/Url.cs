using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.UrlModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("url")]
	public static class Url {
		public static UrlObject Parse(string url) { return null; }

		public static UrlObject Parse(string url, bool parseQueryString) { return null; }

		public static UrlObject Parse(string url, bool parseQueryString, bool slashesDenoteHost) { return null; }

		public static string Format(UrlObject url) { return null; }

		public static string Resolve(string from, string to) { return null; }
	}
}
