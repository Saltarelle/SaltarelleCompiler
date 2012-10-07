using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS {
	[Imported]
	[ModuleName(null)]
	[IgnoreNamespace]
	[ScriptName("console")]
	public static class Console {
		[ExpandParams] public static void Log(params object[] data) {}
		[ExpandParams] public static void Log(string format, params object[] data) {}

		[ExpandParams] public static void Info(params object[] data) {}
		[ExpandParams] public static void Info(string format, params object[] data) {}

		[ExpandParams] public static void Error(params object[] data) {}
		[ExpandParams] public static void Error(string format, params object[] data) {}

		[ExpandParams] public static void Warn(params object[] data) {}
		[ExpandParams] public static void Warn(string format, params object[] data) {}

		public static void Dir(object obj) {}

		public static void Time(string label) {}

		public static void TimeEnd(string label) {}

		public static void Trace(string label) {}

		public static void Assert(bool expression) {}
		public static void Assert(bool expression, string message) {}
	}
}
