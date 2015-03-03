using System.Runtime.CompilerServices;

namespace System
{
	[Imported]
	[IgnoreNamespace]
	[ScriptName("console")]
	public static class JsConsole
	{
		[ScriptName("log")]
		[ExpandParams]
		public static void Log(params object[] data)
		{
		}

		[ScriptName("info")]
		[ExpandParams]
		public static void Info(params object[] data)
		{
		}

		[ScriptName("warn")]
		[ExpandParams]
		public static void Warn(params object[] data)
		{
		}

		[ScriptName("error")]
		[ExpandParams]
		public static void Error(params object[] data)
		{
		}

		[ScriptName("dir")]
		public static void Dir(object data)
		{
		}

		[ScriptName("group")]
		public static void Group()
		{
		}

		[ScriptName("groupCollapsed")]
		public static void GroupCollapsed()
		{
		}

		[ScriptName("groupEnd")]
		public static void GroupEnd()
		{
		}

		[ScriptName("count")]
		public static void Count()
		{
		}

		[ScriptName("count")]
		public static void Count(string label)
		{
		}

		[ScriptName("time")]
		public static void Time(string label)
		{
		}

		[ScriptName("timeEnd")]
		public static void TimeEnd(string label)
		{
		}
	}
}