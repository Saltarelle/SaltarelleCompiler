using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class KeyNotFoundException : Exception {
		public KeyNotFoundException() {
		}

		public KeyNotFoundException(string message) {
		}

		public KeyNotFoundException(string message, Exception innerException) {
		}
	}
}
