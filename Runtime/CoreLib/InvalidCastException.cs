using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class InvalidCastException : Exception {
		public InvalidCastException() {
		}

		public InvalidCastException(string message) {
		}

		public InvalidCastException(string message, Exception innerException) {
		}
	}
}
