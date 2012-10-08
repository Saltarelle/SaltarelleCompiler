using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.DgramModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("dgram")]
	public static class Dgram {
		public static Socket CreateSocket(DgramType type) { return null; }
		public static Socket CreateSocket(DgramType type, Action<Buffer, object> messageListener) { return null; }
	}
}
