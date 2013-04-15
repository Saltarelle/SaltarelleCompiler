using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Float64Array : ArrayBufferView, IList<double> {
		public Float64Array(long length) {}

		public Float64Array(Float64Array array) {}

		public Float64Array(double[] values) {}

		public Float64Array(ArrayBuffer buffer) {}

		public Float64Array(ArrayBuffer buffer, long byteOffset) {}

		public Float64Array(ArrayBuffer buffer, long byteOffset, long length) {}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static int BytesPerElementStatic { get { return 0; } }

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public int BytesPerElement { get { return 0; } }

		[IntrinsicProperty]
		public long Length { get; private set; }

		[IntrinsicProperty]
		public double this[long index] {
			get { return 0; }
			set { }
		}

		public void Set(Float64Array array) {}

		public void Set(Float64Array array, long offset) {}

		public void Set(double[] array) {}

		public void Set(double[] array, long offset) {}

		public Float64Array Subarray(long begin) { return null; }

		public Float64Array Subarray(long begin, long end) { return null; }

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(double item) { return 0; }

		[EnumerateAsArray, InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<double> GetEnumerator() { return null; }

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(double item) { return false; }

		int ICollection<double>.Count { get { return 0; } }

		void ICollection<double>.Add(double item) {}

		void ICollection<double>.Clear() {}

		bool ICollection<double>.Remove(double item) { return false; }

		void IList<double>.Insert(int index, double item) {}

		void IList<double>.RemoveAt(int index) {}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		double IList<double>.this[int index] {
			get { return 0; }
			set {}
		}
	}
}
