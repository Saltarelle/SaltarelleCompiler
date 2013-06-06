using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class NullReferenceException : SystemException {
		[ScriptName("")]
		public NullReferenceException() {
		}

		[ScriptName("")]
		public NullReferenceException(string message) {
		}

		[ScriptName("")]
		public NullReferenceException(string message, Exception innerException) {
		}
	}
}
