using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class ArgumentNullException : ArgumentException {
		public ArgumentNullException() {
		}

		public ArgumentNullException(string paramName) {
		}

		public ArgumentNullException(string paramName, string message) {
		}

		[InlineCode("new {$System.ArgumentNullException}(null, {message}, {innerException})")]
		public ArgumentNullException(string message, Exception innerException) {
		}
	}
}
