using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[Serializable]
	public class ReadResult {
		public int BytesRead { get; set; }
		public Buffer Buffer { get; set; }
	}
}