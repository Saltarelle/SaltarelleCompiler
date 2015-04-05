using System.Runtime.CompilerServices;

namespace System.Threading {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class CancellationTokenSource : IDisposable {
		public CancellationTokenSource() {
		}

		public CancellationTokenSource(int millisecondsDelay) {
		}

		[InlineCode("new {$System.Threading.CancellationTokenSource}({delay}.ticks / 10000)")]
		public CancellationTokenSource(TimeSpan delay) {
		}

		[IntrinsicProperty]
		public bool IsCancellationRequested { get; private set; }

		[IntrinsicProperty]
		public CancellationToken Token { get; private set; }

		public void Cancel() {
		}

		public void Cancel(bool throwOnFirstException) {
		}

		public void CancelAfter(int millisecondsDelay) {
		}

		[InlineCode("{this}.cancelAfter({delay}.ticks / 10000)")]
		public void CancelAfter(TimeSpan delay) {
		}

		public void Dispose() {
		}

		[ScriptName("createLinked")]
		public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2) {
			return null;
		}

		[ScriptName("createLinked")]
		[ExpandParams]
		public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens) {
			return null;
		}
	}
}
