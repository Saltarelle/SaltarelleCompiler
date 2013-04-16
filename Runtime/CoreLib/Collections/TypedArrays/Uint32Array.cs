using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Uint32Array : ArrayBufferView, IList<uint> {
		public Uint32Array(long length) {}

		public Uint32Array(Uint32Array array) {}

		public Uint32Array(uint[] values) {}

		public Uint32Array(ArrayBuffer buffer) {}

		public Uint32Array(ArrayBuffer buffer, long byteOffset) {}

		public Uint32Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public uint this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Uint32Array array) {}

		public void Set(Uint32Array array, long offset) {}

		public void Set(uint[] array) {}

		public void Set(uint[] array, long offset) {}

		public Uint32Array Subarray(long begin) { return null; }

		public Uint32Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(uint item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<uint> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(uint item) { return false; }

		int ICollection<uint>.Count { get { return 0; } }

		void ICollection<uint>.Add(uint item) {}

		void ICollection<uint>.Clear() {}

		bool ICollection<uint>.Remove(uint item) { return false; }

		void IList<uint>.Insert(int index, uint item) {}

		void IList<uint>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		uint IList<uint>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
