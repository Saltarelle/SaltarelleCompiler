using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class IndexOutOfRangeException : SystemException {
		[ScriptName("")]
		public IndexOutOfRangeException() {
		}

		[ScriptName("")]
		public IndexOutOfRangeException(string message) {
		}

		[ScriptName("")]
		public IndexOutOfRangeException(string message, Exception innerException) {
		}
	}
}
