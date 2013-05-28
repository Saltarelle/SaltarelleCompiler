using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class ArgumentNullException : Exception {
		[ScriptName("")]
		public ArgumentNullException() {
		}

		[ScriptName("")]
		public ArgumentNullException(string paramName) {
		}

        [ScriptName("")]
        public ArgumentNullException(string paramName, string message)
        {
        }

		[ScriptName("")]
        public ArgumentNullException(string message, Exception innerException)
        {
		}
	}
}
