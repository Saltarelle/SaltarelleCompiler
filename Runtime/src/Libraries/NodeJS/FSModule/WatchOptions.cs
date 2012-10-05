using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[Serializable]
	public class WatchOptions {
		public bool Persistent { get; set; }
		public int Interval { get; set; }
	}
}