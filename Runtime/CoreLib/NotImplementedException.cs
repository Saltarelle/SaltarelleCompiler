using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class NotImplementedException : Exception {
		public NotImplementedException() {
		}

		public NotImplementedException(string message) {
		}

		public NotImplementedException(string message, Exception innerException) {
		}
	}
}
