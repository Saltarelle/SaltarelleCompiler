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
	}
}
