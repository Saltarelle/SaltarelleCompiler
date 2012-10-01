using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	public class AggregateException : Exception {
		public AggregateException() {
		}

		[AlternateSignature]
		public AggregateException(IEnumerable<Exception> innerExceptions) {
		}

		[AlternateSignature]
		public AggregateException(params Exception[] innerExceptions) {
		}

		[AlternateSignature]
		public AggregateException(string message) {
		}

		[AlternateSignature]
		public AggregateException(string message, IEnumerable<Exception> innerExceptions) {
		}

		[AlternateSignature]
		public AggregateException(string message, params Exception[] innerExceptions) {
		}

		public Exception[] InnerExceptions { get { return null; } }
	}
}
