using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[Serializable]
	public class CreateReadStreamOptions {
		public OpenFlags? Flags { get; set; }
		public Encoding? Encoding { get; set; }
		public int? Fd { get; set; }
		public TypeOption<string, int> Mode { get; set; }
		public int? BufferSize { get; set; }
		public int? Start { get; set; }
		public int? End { get; set; }
	}
}