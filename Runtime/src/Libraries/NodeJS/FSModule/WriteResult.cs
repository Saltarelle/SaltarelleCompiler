using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[Serializable]
	public class WriteResult {
		public int Written { get; set; }
		public Buffer Buffer { get; set; }
	}
}