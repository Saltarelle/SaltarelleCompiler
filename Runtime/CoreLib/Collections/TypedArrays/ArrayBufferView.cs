using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	/// <summary>
	/// The ArrayBufferView type holds information shared among
	/// all of the types of views of ArrayBuffers.
	/// </summary>
	[IgnoreNamespace, Imported]
	public class ArrayBufferView {
		internal ArrayBufferView() {
		}

		/// <summary>
		/// The ArrayBuffer that this ArrayBufferView references.
		/// </summary>
		[IntrinsicProperty]
		public ArrayBuffer Buffer { get; private set; }

		/// <summary>
		/// The offset of this ArrayBufferView from the start of
		/// its ArrayBuffer, in bytes, as fixed at construction time.
		/// </summary>
		[IntrinsicProperty]
		public long ByteOffset { get; private set; }

		/// <summary>
		/// The length of the ArrayBufferView in bytes, as fixed at construction time.
		/// </summary>
		[IntrinsicProperty]
		public long ByteLength { get; private set; }
	}
}
