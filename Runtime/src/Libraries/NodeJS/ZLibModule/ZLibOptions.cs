using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;

namespace NodeJS.ZLibModule {
	[Imported]
	[Serializable]
	public class ZLibOptions {
		public int? ChunkSize { get; set; }
		public int? WindowBits { get; set; }
		public int? Level { get; set; }
		public int? MemLevel { get; set; }
		public int? Strategy { get; set; }
		public Buffer Dictionary { get; set; }
	}
}
