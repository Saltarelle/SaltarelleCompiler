using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.PathModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("path")]
	public static class Path {
		public static string Normalize(string path) { return null; }

		[ExpandParams]
		public static string Join(params string[] paths) { return null; }

		[ExpandParams]
		public static string Resolve(params string[] paths) { return null; }

		public static string Relative(string from, string to) { return null; }

		[ScriptName("dirname")]
		public static string DirName(string path) { return null; }

		[ScriptName("basename")]
		public static string BaseName(string path) { return null; }

		[ScriptName("extname")]
		public static string ExtName(string path) { return null; }

		[IntrinsicProperty]
		public static string Sep { get { return null; } }
	}
}
