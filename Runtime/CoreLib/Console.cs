// Console.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System
{
	[Imported(ObeysTypeSystem = true)]
	[IgnoreNamespace]
	[ScriptName("console")]
	public static class Console
	{
		[ScriptName("log")]
		public static void WriteLine(string message) {
		}

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

		[ScriptName("trace")]
		public static void Trace()
		{
		}
	}
}
