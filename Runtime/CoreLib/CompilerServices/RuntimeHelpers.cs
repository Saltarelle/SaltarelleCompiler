using System.ComponentModel;

namespace System.Runtime.CompilerServices {
	[Imported]
	public static class RuntimeHelpers {
		[EditorBrowsable(EditorBrowsableState.Never)]
		[NonScriptable]
		public static void InitializeArray(Array array, RuntimeFieldHandle handle) {
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[NonScriptable]
		public static int OffsetToStringData { get { return 0; } }

		[InlineCode("{$System.Script}.defaultHashCode({obj})")]
		public static int GetHashCode(object obj) { return 0; }
	}
}