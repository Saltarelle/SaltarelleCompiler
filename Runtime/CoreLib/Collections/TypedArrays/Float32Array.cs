using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Float32Array : ArrayBufferView, IList<float> {
		public Float32Array(long length) {}

		public Float32Array(Float32Array array) {}

		public Float32Array(float[] values) {}

		public Float32Array(ArrayBuffer buffer) {}

		public Float32Array(ArrayBuffer buffer, long byteOffset) {}

		public Float32Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public float this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Float32Array array) {}

		public void Set(Float32Array array, long offset) {}

		public void Set(float[] array) {}

		public void Set(float[] array, long offset) {}

		public Float32Array Subarray(long begin) { return null; }

		public Float32Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(float item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<float> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(float item) { return false; }

		int ICollection<float>.Count { get { return 0; } }

		void ICollection<float>.Add(float item) {}

		void ICollection<float>.Clear() {}

		bool ICollection<float>.Remove(float item) { return false; }

		void IList<float>.Insert(int index, float item) {}

		void IList<float>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		float IList<float>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
