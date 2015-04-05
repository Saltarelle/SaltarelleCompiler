using System.Runtime.CompilerServices;

namespace System.Threading {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable {
		public bool Equals(CancellationTokenRegistration other) {
			return false;
		}

		public void Dispose() {
		}

		[InlineCode("{$System.Script}.equals({left}, {right})")]
		public static bool operator==(CancellationTokenRegistration left, CancellationTokenRegistration right) {
			return false;
		}

		[InlineCode("!{$System.Script}.equals({left}, {right})")]
		public static bool operator!=(CancellationTokenRegistration left, CancellationTokenRegistration right) {
			return false;
		}
	}
}
