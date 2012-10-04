using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[Serializable]
	public class UnixConnectOptions : ConnectOptions {
		public string Path { get; set; }
	}
}
