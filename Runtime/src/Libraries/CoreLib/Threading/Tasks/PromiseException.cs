using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Threading.Tasks {
	/// <summary>
	/// This exception is used as the exception for a task created from a promise when the underlying promise fails.
	/// </summary>
	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	public class PromiseException : Exception {
		public PromiseException(object[] args) {
		}

		/// <summary>
		/// Arguments supplied to the promise onError() callback.
		/// </summary>
		public object[] Arguments { get { return null; } }
	}
}
