using System.Runtime.CompilerServices;

namespace System.Threading {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public struct CancellationToken {
		public CancellationToken(bool canceled) {
		}

		[IntrinsicProperty]
		public static CancellationToken None { get { return default(CancellationToken); } }

		public bool CanBeCanceled { get { return false; } }

		public bool IsCancellationRequested { get { return false; } }

		public void ThrowIfCancellationRequested() {}

		public CancellationTokenRegistration Register(Action callback) {
			return default(CancellationTokenRegistration);
		}

		[InlineCode("{this}.register({callback})")]
		public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext) {
			return default(CancellationTokenRegistration);
		}

		public CancellationTokenRegistration Register(Action<object> callback, object state) {
			return default(CancellationTokenRegistration);
		}

		[InlineCode("{this}.register({callback}, {state})")]
		public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext) {
			return default(CancellationTokenRegistration);
		}
	}
}
