using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.HttpModule {
	[Imported]
	[ModuleName("http")]
	[IgnoreNamespace]
	public class Agent {
		private Agent() {}

		public int MaxSockets { get; set; }
	}
}
