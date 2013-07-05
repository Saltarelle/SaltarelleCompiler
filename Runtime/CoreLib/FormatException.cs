using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class FormatException : Exception {
		[ScriptName("")]
		public FormatException() {
		}

		[ScriptName("")]
		public FormatException(string message) {
		}

		[ScriptName("")]
        public FormatException(string message, Exception innerException)
        {
		}
	}
}
