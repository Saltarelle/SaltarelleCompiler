using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class OverflowException : ArithmeticException {
		public OverflowException() {
		}

		public OverflowException(string message) {
		}

		public OverflowException(string message, Exception innerException) {
		}
	}
}
