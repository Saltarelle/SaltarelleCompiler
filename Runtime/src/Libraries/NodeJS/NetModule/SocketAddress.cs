using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[Serializable]
	public class SocketAddress {
		public int Port { get; set; }
		public string Family { get; set; }
		public string Address { get; set; }

		public SocketAddress(int port, string family, string address) {
		}
	}
}
