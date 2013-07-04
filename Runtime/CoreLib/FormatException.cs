using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class FormatException : Exception {
		public FormatException() {
		}

		public FormatException(string message) {
		}

		public FormatException(string message, Exception innerException) {
		}
	}
}
