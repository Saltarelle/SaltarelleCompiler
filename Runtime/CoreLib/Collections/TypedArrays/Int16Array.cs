using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Int16Array : ArrayBufferView, IList<short>, IReadOnlyList<short> {
		public Int16Array(long length) {}

		public Int16Array(Int16Array array) {}

		public Int16Array(short[] values) {}

		public Int16Array(ArrayBuffer buffer) {}

		public Int16Array(ArrayBuffer buffer, long byteOffset) {}

		public Int16Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public short this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Int16Array array) {}

		public void Set(Int16Array array, long offset) {}

		public void Set(short[] array) {}

		public void Set(short[] array, long offset) {}

		public Int16Array Subarray(long begin) { return null; }

		public Int16Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(short item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<short> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(short item) { return false; }

		int ICollection<short>.Count { get { return 0; } }

		void ICollection<short>.Add(short item) {}

		void ICollection<short>.Clear() {}

		bool ICollection<short>.Remove(short item) { return false; }

		void IList<short>.Insert(int index, short item) {}

		void IList<short>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		short IList<short>.this[int index] {
			get { return 0; }
			set {}
		}

		int IReadOnlyCollection<short>.Count { get { return 0; } }

		short IReadOnlyList<short>.this[int index] {
			get { return 0; }
		}

		bool IReadOnlyCollection<short>.Contains(short item) {
			return false;
		}
	}
}
