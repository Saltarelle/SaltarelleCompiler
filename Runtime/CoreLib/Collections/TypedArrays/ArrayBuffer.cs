using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	/// <summary>
	/// The ArrayBuffer type describes a buffer used to store data for the array buffer views.
	/// </summary>
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class ArrayBuffer {
		public ArrayBuffer(long byteLength) {
		}

		/// <summary>
		/// The length of the ArrayBuffer in bytes, as fixed at construction time.
		/// </summary>
		[IntrinsicProperty]
		public long ByteLength { get; private set; }

		/// <summary>
		/// Returns a new ArrayBuffer whose contents are a copy of this
		/// ArrayBuffer's bytes from begin, inclusive, up to end, exclusive.
		/// If either begin or end is negative, it refers to an index from the
		/// end of the array, as opposed to from the beginning.
		/// </summary>
		public ArrayBuffer Slice(long begin) {
			return null;
		}

		/// <summary>
		/// Returns a new ArrayBuffer whose contents are a copy of this
		/// ArrayBuffer's bytes from begin, inclusive, up to end, exclusive.
		/// If either begin or end is negative, it refers to an index from the
		/// end of the array, as opposed to from the beginning.
		/// </summary>
		public ArrayBuffer Slice(long begin, long end) {
			return null;
		}
	}
}
