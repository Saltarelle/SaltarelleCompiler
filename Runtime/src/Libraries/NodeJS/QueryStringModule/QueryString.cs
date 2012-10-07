using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.QueryStringModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("querystring")]
	public static class QueryString {
		public static string Stringify(JsDictionary obj) { return null; }

		public static string Stringify(JsDictionary obj, string sep) { return null; }

		public static string Stringify(JsDictionary obj, string sep, string eq) { return null; }


		public static JsDictionary Parse(string str) { return null; }

		public static JsDictionary Parse(string str, string sep) { return null; }

		public static JsDictionary Parse(string str, string sep, string eq) { return null; }

		public static JsDictionary Parse(string str, string sep, string eq, ParseOptions options) { return null; }
	}
}
