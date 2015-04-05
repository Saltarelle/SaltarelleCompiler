using System.Runtime.CompilerServices;
using System.Threading;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class OperationCanceledException : Exception {
		public OperationCanceledException() {
		}

		[InlineCode("new {$System.OperationCanceledException}(null, {token})")]
		public OperationCanceledException(CancellationToken token) {
		}

		[InlineCode("new {$System.OperationCanceledException}({message}, {$System.Threading.CancellationToken}.none)")]
		public OperationCanceledException(string message) {
		}

		[InlineCode("new {$System.OperationCanceledException}({message}, {$System.Threading.CancellationToken}.none, {innerException})")]
		public OperationCanceledException(string message, Exception innerException) {
		}

		public OperationCanceledException(string message, CancellationToken token) {
		}

		[InlineCode("new {$System.OperationCanceledException}({message}, {token}, {innerException})")]
		public OperationCanceledException(string message, Exception innerException, CancellationToken token) {
		}

		[IntrinsicProperty]
		public CancellationToken CancellationToken { get; private set; }
	}
}
