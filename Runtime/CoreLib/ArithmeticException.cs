using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class ArithmeticException : Exception {
		public ArithmeticException() {
		}

		public ArithmeticException(string message) {
		}

		public ArithmeticException(string message, Exception innerException) {
		}
	}
}
