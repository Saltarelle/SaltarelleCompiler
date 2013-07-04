using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class NotSupportedException : Exception {
		public NotSupportedException() {
		}

		public NotSupportedException(string message) {
		}

		public NotSupportedException(string message, Exception innerException) {
		}
	}
}
