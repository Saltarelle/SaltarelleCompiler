using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Int8Array : ArrayBufferView, IList<sbyte>, IReadOnlyList<sbyte> {
		public Int8Array(long length) {}

		public Int8Array(Int8Array array) {}

		public Int8Array(sbyte[] values) {}

		public Int8Array(ArrayBuffer buffer) {}

		public Int8Array(ArrayBuffer buffer, long byteOffset) {}

		public Int8Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public sbyte this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Int8Array array) {}

		public void Set(Int8Array array, long offset) {}

		public void Set(sbyte[] array) {}

		public void Set(sbyte[] array, long offset) {}

		public Int8Array Subarray(long begin) { return null; }

		public Int8Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(sbyte item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<sbyte> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(sbyte item) { return false; }

		int ICollection<sbyte>.Count { get { return 0; } }

		void ICollection<sbyte>.Add(sbyte item) {}

		void ICollection<sbyte>.Clear() {}

		bool ICollection<sbyte>.Remove(sbyte item) { return false; }

		void IList<sbyte>.Insert(int index, sbyte item) {}

		void IList<sbyte>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		sbyte IList<sbyte>.this[int index] {
			get { return 0; }
			set {}
		}

		int IReadOnlyCollection<sbyte>.Count { get { return 0; } }

		sbyte IReadOnlyList<sbyte>.this[int index] {
			get { return 0; }
		}

		bool IReadOnlyCollection<sbyte>.Contains(sbyte item) {
			return false;
		}
	}
}
