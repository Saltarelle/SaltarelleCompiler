using System.Runtime.CompilerServices;

namespace System {
	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	public class JsErrorException : Exception {
		public JsErrorException(Error error) {
		}

		public Error Error { get { return null; } }
	}
}
