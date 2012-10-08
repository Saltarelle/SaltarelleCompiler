using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.OSModule {
	[Imported]
	[Serializable]
	public class CpuInfo {
		public string Model { get; set; }
		public int Speed { get; set; }
		public CpuTimes Times { get; set; }
	}

	[Imported]
	[Serializable]
	public class CpuTimes {
		public long User { get; set; }
		public long Nice { get; set; }
		public long Sys { get; set; }
		public long Idle { get; set; }
		public long Irq { get; set; }
	}
}
