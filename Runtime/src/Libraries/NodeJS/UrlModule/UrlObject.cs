using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.UrlModule {
	[Imported]
	[Serializable]
	public class UrlObject {
		public string Href { get; set; }
		public string Protocol { get; set; }
		public string Host { get; set; }
		public string Auth { get; set; }
		public string Hostname { get; set; }
		public string Port { get; set; }
		public string Pathname { get; set; }
		public string Search { get; set; }
		public string Path { get; set; }
		public TypeOption<string, JsDictionary<string, string>> Query { get; set; }
		public string Hash { get; set; }
	}
}
