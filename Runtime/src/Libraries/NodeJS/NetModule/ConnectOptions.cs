using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[Serializable]
	public class ConnectOptions {
		public bool? AllowHalfOpen { get; set; }
	}
}
