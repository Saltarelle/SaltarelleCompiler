using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.OSModule {
	[Imported]
	[Serializable]
	public class NetworkInterfaceInfo {
		public string Address { get; set; }
		public string Family { get; set; }
		public bool Internal { get; set; }
	}
}
