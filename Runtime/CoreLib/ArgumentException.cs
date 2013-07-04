using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class ArgumentException : Exception {
		public ArgumentException() {
		}

		public ArgumentException(string message) {
		}

		[InlineCode("new {$System.ArgumentException}({message}, null, {innerException})")]
		public ArgumentException(string message, Exception innerException) {
		}

		public ArgumentException(string message, string paramName) {
		}

		public ArgumentException(string message, string paramName, Exception innerException) {
		}

		[IntrinsicProperty]
		public string ParamName { get { return null; } }
	}
}
