using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class NullReferenceException : Exception {
		public NullReferenceException() {
		}

		public NullReferenceException(string message) {
		}

		public NullReferenceException(string message, Exception innerException) {
		}
	}
}
