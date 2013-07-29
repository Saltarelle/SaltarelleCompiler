using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class InvalidOperationException : Exception {
		public InvalidOperationException() {
		}

		public InvalidOperationException(string message) {
		}

		public InvalidOperationException(string message, Exception innerException) {
		}
	}
}
