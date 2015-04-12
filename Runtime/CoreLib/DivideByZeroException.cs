using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class DivideByZeroException : ArithmeticException {
		public DivideByZeroException() {
		}

		public DivideByZeroException(string message) {
		}

		public DivideByZeroException(string message, Exception innerException) {
		}
	}
}
