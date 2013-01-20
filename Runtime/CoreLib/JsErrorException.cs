using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class JsErrorException : Exception {
		public JsErrorException(Error error) {
		}

		public Error Error { get { return null; } }
	}
}
