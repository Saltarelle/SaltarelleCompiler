using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[Serializable]
	public class TcpConnectOptions : ConnectOptions {
		public int Port { get; set; }
		public string Host { get; set; }
		public string LocalAddress { get; set; }
	}
}
