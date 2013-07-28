using System.Runtime.CompilerServices;

namespace System.Diagnostics {
	[Imported]
	public static class Debugger {
		[InlineCode("debugger")]
		public static void Break() {
		}
	}
}
