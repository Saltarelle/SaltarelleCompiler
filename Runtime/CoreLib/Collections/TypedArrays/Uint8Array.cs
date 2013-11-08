using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Uint8Array : ArrayBufferView, IList<byte> {
		public Uint8Array(long length) {}

		public Uint8Array(Uint8Array array) {}

		public Uint8Array(byte[] values) {}

		public Uint8Array(ArrayBuffer buffer) {}

		public Uint8Array(ArrayBuffer buffer, long byteOffset) {}

		public Uint8Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public byte this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Uint8Array array) {}

		public void Set(Uint8Array array, long offset) {}

		public void Set(byte[] array) {}

		public void Set(byte[] array, long offset) {}

		public Uint8Array Subarray(long begin) { return null; }

		public Uint8Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(byte item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<byte> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(byte item) { return false; }

		int ICollection<byte>.Count { get { return 0; } }

		void ICollection<byte>.Add(byte item) {}

		void ICollection<byte>.Clear() {}

		bool ICollection<byte>.Remove(byte item) { return false; }

		void IList<byte>.Insert(int index, byte item) {}

		void IList<byte>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		byte IList<byte>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
