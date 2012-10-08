using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS {
	[Imported]
	[ModuleName(null)]
	[ScriptName("require")]
	public static class Require {
		/// <summary>
		/// Equivalent to 'require(module)' in JS.
		/// </summary>
		[ScriptAlias("require")]
		public static dynamic Load(string module) { return null; }

		public static string Resolve(string module) { return null; }

		[IntrinsicProperty]
		public static JsDictionary<string, dynamic> Cache { get { return null; } }

		[IntrinsicProperty]
		public static JsDictionary<string, Func<Action<dynamic, string>>> Extensions { get { return null; } }
	}
}
