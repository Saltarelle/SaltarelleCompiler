using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.ReplModule {
	[Imported]
	[Serializable]
	public class ReplOptions {
		public string Prompt { get; set; }
		public ReadableStream Input { get; set; }
		public WritableStream Output { get; set; }
		public bool? Terminal { get; set; }
		public Action<string, object, string, Action<Error, object>> Eval { get; set; }
		public bool? UseColors { get; set; }
		public bool? UseGlobal { get; set; }
		public bool? IgnoreUndefined { get; set; }
		public Action<object> Writer { get; set; }
	}
}
