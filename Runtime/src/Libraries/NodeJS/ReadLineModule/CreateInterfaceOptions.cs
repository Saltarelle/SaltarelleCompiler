using System;
using System.Runtime.CompilerServices;

namespace NodeJS.ReadLineModule {
	[Imported]
	[Serializable]
	public class CreateInterfaceOptions {
		public ReadableStream Input { get; set; }
		public WritableStream Output { get; set; }
		public Func<string, object[]> Completer { get; set; }
		public bool? Terminal { get; set; }
	}
}