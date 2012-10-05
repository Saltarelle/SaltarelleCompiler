using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.OSModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("os")]
	public static class OS {
		public static string TmpDir { [ScriptName("tmpDir")] get { return null; } }

		public static string Hostname { [ScriptName("hostname")] get { return null; } }

		public static string Type { [ScriptName("type")] get { return null; } }

		public static string Platform { [ScriptName("platform")] get { return null; } }

		public static string Arch { [ScriptName("arch")] get { return null; } }

		public static string Release { [ScriptName("release")] get { return null; } }

		public static double Uptime { [ScriptName("uptime")] get { return 0; } }

		public static double[] LoadAvg { [ScriptName("loadavg")] get { return null; } }

		public static long TotalMem { [ScriptName("totalmem")] get { return 0; } }

		public static long FreeMem { [ScriptName("freemem")] get { return 0; } }

		public static CpuInfo[] Cpus { [ScriptName("cpus")] get { return null; } }

		public static NetworkInterfaceInfo[] NetworkInterfaces { [ScriptName("networkInterfaces")] get { return null; } }

		public static string Eol { [ScriptName("EOL")] get { return null; } }
	}
}
