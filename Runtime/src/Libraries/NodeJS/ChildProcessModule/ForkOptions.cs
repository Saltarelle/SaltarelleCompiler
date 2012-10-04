using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.ChildProcessModule {
	[Imported]
	[Serializable]
	public class ForkOptions {
		public string Cwd { get; set; }
		public JsDictionary<string, string> Env { get; set; }
		public Encoding Encoding { get; set; }
	}
}