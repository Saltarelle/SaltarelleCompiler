using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.ChildProcessModule {
	[Imported]
	[Serializable]
	public class SpawnOptions {
		public string Cwd { get; set; }
		public TypeOption<object[], string> Stdio { get; set; }
		public JsDictionary<string, string> Env { get; set; }
		public bool Detached { get; set; }
		public bool Silent { get; set; }
	}
}