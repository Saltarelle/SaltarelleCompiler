using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Uint16Array : ArrayBufferView, IList<ushort> {
		public Uint16Array(long length) {}

		public Uint16Array(Uint16Array array) {}

		public Uint16Array(ushort[] values) {}

		public Uint16Array(ArrayBuffer buffer) {}

		public Uint16Array(ArrayBuffer buffer, long byteOffset) {}

		public Uint16Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public ushort this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Uint16Array array) {}

		public void Set(Uint16Array array, long offset) {}

		public void Set(ushort[] array) {}

		public void Set(ushort[] array, long offset) {}

		public Uint16Array Subarray(long begin) { return null; }

		public Uint16Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(ushort item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<ushort> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(ushort item) { return false; }

		int ICollection<ushort>.Count { get { return 0; } }

		void ICollection<ushort>.Add(ushort item) {}

		void ICollection<ushort>.Clear() {}

		bool ICollection<ushort>.Remove(ushort item) { return false; }

		void IList<ushort>.Insert(int index, ushort item) {}

		void IList<ushort>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		ushort IList<ushort>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
