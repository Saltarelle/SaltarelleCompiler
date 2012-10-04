using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.ChildProcessModule {
	[Imported]
	[Serializable]
	public class ExecOptions {
		public string Cwd { get; set; }
		public TypeOption<object[], string> Stdio { get; set; }
		public JsDictionary<string, string> Env { get; set; }
		public Encoding Encoding { get; set; }
		public int Timeout { get; set; }
		public int MaxBuffer { get; set; }
		public string KillSignal { get; set; }
	}
}