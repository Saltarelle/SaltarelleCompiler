using System;
using System.Runtime.CompilerServices;

namespace NodeJS.DnsModule {
	[Imported]
	[Serializable]
	public class LookupResult {
		public int Family { get; set; }
		public string Address { get; set; }
	}
}