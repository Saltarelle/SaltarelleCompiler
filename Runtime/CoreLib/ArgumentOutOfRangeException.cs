using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class ArgumentOutOfRangeException : ArgumentException {
		public ArgumentOutOfRangeException() {
		}

		public ArgumentOutOfRangeException(string paramName) {
		}

		[InlineCode("new {$System.ArgumentOutOfRangeException}(null, {message}, {innerException})")]
		public ArgumentOutOfRangeException(string message, Exception innerException) {
		}

		public ArgumentOutOfRangeException(string paramName, string message) {
		}

		[InlineCode("new {$System.ArgumentOutOfRangeException}({paramName}, {message}, null, {actualValue})")]
		public ArgumentOutOfRangeException(string paramName, object actualValue, string message) {
		}

		[IntrinsicProperty]
		public object ActualValue { get { return null; } }
	}
}
