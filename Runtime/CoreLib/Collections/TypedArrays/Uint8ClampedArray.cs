using System.Runtime.CompilerServices;

namespace System.Collections.TypedArrays {
	[IgnoreNamespace, Imported(ObeysTypeSystem = true)]
	public class Uint8ClampedArray : Uint8Array {
		public Uint8ClampedArray(long length) : base(length) {
		}

		public Uint8ClampedArray(Uint8ClampedArray array) : base(array) {
		}

		public Uint8ClampedArray(Uint8Array array) : base(array) {
		}

		public Uint8ClampedArray(byte[] values) : base(values) {
		}

		public Uint8ClampedArray(ArrayBuffer buffer) : base(buffer) {
		}

		public Uint8ClampedArray(ArrayBuffer buffer, long byteOffset) : base(buffer, byteOffset) {
		}

		public Uint8ClampedArray(ArrayBuffer buffer, long byteOffset, long length) : base(buffer, byteOffset, length) {
		}

		[IntrinsicProperty, ScriptName("BYTES_PER_ELEMENT")]
		public static new int BytesPerElementStatic { get { return 0; } }

		public void Set(Uint8ClampedArray array) {
		}

		public void Set(Uint8ClampedArray array, long offset) {
		}

		public new Uint8ClampedArray Subarray(long begin) {
			return null;
		}

		public new Uint8ClampedArray Subarray(long begin, long end) {
			return null;
		}
	}
}
