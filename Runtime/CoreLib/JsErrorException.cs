using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class JsErrorException : Exception {
		public JsErrorException(Error error) {
		}

		public JsErrorException(Error error, string message) {
		}

		public JsErrorException(Error error, string message, Exception innerException) {
		}

		[IntrinsicProperty]
		public Error Error { get { return null; } }

		/// <summary>
		/// The stack of the originating Javascript Error, if available.
		/// </summary>
		public override string Stack {
			get { return null; }
		}
	}
}
