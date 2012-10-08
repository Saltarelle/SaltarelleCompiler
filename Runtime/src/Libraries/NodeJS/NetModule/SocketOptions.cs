using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[NamedValues]
	public enum SocketType {
		Tcp4,
		Tcp6,
		Unix,
	}

	[Imported]
	[Serializable]
	public class SocketOptions {
		public int? Fd { get; set; }
		public SocketType? Type { get; set; }
		public bool? AllowHalfOpen { get; set; }
	}
}
