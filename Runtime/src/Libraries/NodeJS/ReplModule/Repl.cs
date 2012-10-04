using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.ReplModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("repl")]
	public static class Repl {
		public static ReplServer Start(ReplOptions options) { return null; }
	}
}
