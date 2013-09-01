using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class AmbiguousMatchException : Exception {
		public AmbiguousMatchException() {
		}

		public AmbiguousMatchException(string message) {
		}

		public AmbiguousMatchException(string message, Exception innerException) {
		}
	}
}
