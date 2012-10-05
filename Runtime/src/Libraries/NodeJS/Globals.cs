using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS {
	[Imported]
	[GlobalMethods]
	[ModuleName(null)]
	public static class Globals {
		public static TimeoutHandle SetTimeout(Action callback, int milliseconds) { return null; }
		public static void ClearTimeout(TimeoutHandle handle) {}

		public static IntervalHandle SetInterval(Action callback, int milliseconds) { return null; }
		public static void ClearInterval(IntervalHandle handle) {}

		[IntrinsicProperty, ScriptName("__filename")] public static string FileName { get { return null; } }
		[IntrinsicProperty, ScriptName("__dirname")] public static string DirName { get { return null; } }

		[IntrinsicProperty]
		public static dynamic Module { get { return null; } }

		[IntrinsicProperty]
		public static dynamic Exports { get { return null; } }
	}

	[Imported]
	public class TimeoutHandle {
		private TimeoutHandle() {}
	}

	[Imported]
	public class IntervalHandle {
		private IntervalHandle() {}
	}
}
