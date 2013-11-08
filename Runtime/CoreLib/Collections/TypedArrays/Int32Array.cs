using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Int32Array : ArrayBufferView, IList<int> {
		public Int32Array(long length) {}

		public Int32Array(Int32Array array) {}

		public Int32Array(int[] values) {}

		public Int32Array(ArrayBuffer buffer) {}

		public Int32Array(ArrayBuffer buffer, long byteOffset) {}

		public Int32Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public int this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Int32Array array) {}

		public void Set(Int32Array array, long offset) {}

		public void Set(int[] array) {}

		public void Set(int[] array, long offset) {}

		public Int32Array Subarray(long begin) { return null; }

		public Int32Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(int item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<int> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(int item) { return false; }

		int ICollection<int>.Count { get { return 0; } }

		void ICollection<int>.Add(int item) {}

		void ICollection<int>.Clear() {}

		bool ICollection<int>.Remove(int item) { return false; }

		void IList<int>.Insert(int index, int item) {}

		void IList<int>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		int IList<int>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
