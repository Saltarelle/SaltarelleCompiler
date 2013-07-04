using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class SystemException : Exception {
		[ScriptName("")]
		public SystemException() {
		}

		[ScriptName("")]
		public SystemException(string message) {
		}

		[ScriptName("")]
		public SystemException(string message, Exception innerException) {
		}
	}
}
