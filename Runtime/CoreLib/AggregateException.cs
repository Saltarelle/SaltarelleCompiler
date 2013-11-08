using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class AggregateException : Exception {
		public AggregateException() {
		}

		[InlineCode("new {$System.AggregateException}(null, {innerExceptions})")]
		public AggregateException(IEnumerable<Exception> innerExceptions) {
		}

		[InlineCode("new {$System.AggregateException}(null, {innerExceptions})")]
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

		[IntrinsicProperty]
		public ReadOnlyCollection<Exception> InnerExceptions { get { return null; } }
	}
}
