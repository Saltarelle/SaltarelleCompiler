using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class NotSupportedException : Exception {
		[ScriptName("")]
		public NotSupportedException() {
		}

		[ScriptName("")]
		public NotSupportedException(string message) {
		}

		[ScriptName("")]
		public NotSupportedException(string message, Exception innerException) {
		}
	}
}
