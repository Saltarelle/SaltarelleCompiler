using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[Serializable]
	public class CreateWriteStreamOptions {
		public OpenFlags? Flags { get; set; }
		public Encoding? Encoding { get; set; }
		public TypeOption<string, int> Mode { get; set; }
		public int? Start { get; set; }
	}
}