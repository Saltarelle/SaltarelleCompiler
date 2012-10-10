using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.UtilModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("util")]
	public static class Url {
		[ExpandParams]
		public static string Format(string format, params object[] args) { return null; }

		public static void Debug(string message) {}

		[ExpandParams]
		public static void Error(params object[] messages) {}

		[ExpandParams]
		public static void Puts(params object[] messages) {}

		[ExpandParams]
		public static void Print(params object[] messages) {}

		public static void Log(string message) {}

		public static void Inspect(object obj) {}

		public static void Inspect(object obj, bool showHidden) {}

		public static void Inspect(object obj, bool showHidden, double? depth) {}

		public static void Inspect(object obj, bool showHidden, double? depth, bool colors) {}

		public static bool IsArray(object value) { return false; }

		public static bool IsRegExp(object value) { return false; }

		public static bool IsDate(object value) { return false; }

		public static bool IsError(object value) { return false; }
	}
}
