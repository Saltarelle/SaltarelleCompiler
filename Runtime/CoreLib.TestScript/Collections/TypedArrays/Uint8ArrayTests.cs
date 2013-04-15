using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.TypedArrays;
using QUnit;

namespace CoreLib.TestScript.Collections.TypedArrays {
	[TestFixture]
	public class Uint8ArrayTests {
		private void AssertContent(Uint8Array actual, int[] expected, string message) {
			if (actual.Length != expected.Length) {
				Assert.Fail(message + ": Expected length " + expected.Length + ", actual: " + actual.Length);
				return;
			}
			for (int i = 0; i < expected.Length; i++) {
				if (actual[i] != expected[i]) {
					Assert.Fail(message + ": Position " + i + ": expected " + expected[i] + ", actual: " + actual[i]);
					return;
				}
			}
			Assert.IsTrue(true, message);
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Uint8Array).FullName, "Uint8Array", "FullName");

			var interfaces = typeof(Uint8Array).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3, "Interface count should be 3");
			Assert.IsTrue(interfaces.Contains(typeof(IEnumerable<byte>)), "Interfaces should contain IEnumerable<byte>");
			Assert.IsTrue(interfaces.Contains(typeof(ICollection<byte>)), "Interfaces should contain ICollection<byte>");
			Assert.IsTrue(interfaces.Contains(typeof(IList<byte>)), "Interfaces should contain IList<byte>");

			object arr = new Uint8Array(0);
			Assert.IsTrue(arr is Uint8Array, "Is Uint8Array");
			Assert.IsTrue(arr is IEnumerable<byte>, "Is IEnumerable<byte>");
			Assert.IsTrue(arr is ICollection<byte>, "Is ICollection<byte>");
			Assert.IsTrue(arr is IList<byte>, "Is IList<byte>");
		}

		[Test]
		public void LengthConstructorWorks() {
			var arr = new Uint8Array(13);
			Assert.IsTrue((object)arr is Uint8Array, "is Uint8Array");
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void ConstructorFromIntWorks() {
			var source = new byte[] { 3, 8, 4 };
			var arr = new Uint8Array(source);
			Assert.IsTrue((object)arr != (object)source, "New object");
			Assert.IsTrue((object)arr is Uint8Array, "is Uint8Array");
			AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void CopyConstructorWorks() {
			 var source = new Uint8Array(new byte[] { 3, 8, 4 });
			 var arr = new Uint8Array(source);
			 Assert.IsTrue(arr != source, "New object");
			 Assert.IsTrue((object)arr is Uint8Array, "is Uint8Array");
			 AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void ArrayBufferConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint8Array(buf);
			Assert.IsTrue((object)arr is Uint8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 80, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint8Array(buf, 16);
			Assert.IsTrue((object)arr is Uint8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 64, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetAndLengthConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint8Array(buf, 16, 12);
			Assert.IsTrue((object)arr is Uint8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 12, "length");
		}

		[Test]
		public void InstanceBytesPerElementWorks() {
			Assert.AreEqual(new Uint8Array(0).BytesPerElement, 1);
		}

		[Test]
		public void StaticBytesPerElementWorks() {
			Assert.AreEqual(Uint8Array.BytesPerElementStatic, 1);
		}

		[Test]
		public void LengthWorks() {
			var arr = new Uint8Array(13);
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void IndexingWorks() {
			var arr = new Uint8Array(3);
			arr[1] = 42;
			AssertContent(arr, new[] { 0, 42, 0 }, "Content");
			Assert.AreEqual(arr[1], 42, "[1]");
		}

		[Test]
		public void SetUint8ArrayWorks() {
			var arr = new Uint8Array(4);
			arr.Set(new Uint8Array(new byte[] { 3, 6, 7 }));
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetUint8ArrayWithOffsetWorks() {
			var arr = new Uint8Array(6);
			arr.Set(new Uint8Array(new byte[] { 3, 6, 7 }), 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWorks() {
			var arr = new Uint8Array(4);
			arr.Set(new byte[] { 3, 6, 7 });
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWithOffsetWorks() {
			var arr = new Uint8Array(6);
			arr.Set(new byte[] { 3, 6, 7 }, 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SubarrayWithBeginWorks() {
			var source = new Uint8Array(10);
			var arr = source.Subarray(3);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 3, "ByteOffset should be correct");
		}

		[Test]
		public void SubarrayWithBeginAndEndWorks() {
			var source = new Uint8Array(10);
			var arr = source.Subarray(3, 7);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 3, "ByteOffset should be correct");
			Assert.AreEqual(arr.Length, 4, "Length should be correct");
		}

		[Test]
		public void BufferPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Uint8Array(buf);
			Assert.IsTrue(arr.Buffer == buf, "Should be correct");
		}

		[Test]
		public void ByteOffsetPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Uint8Array(buf, 32);
			Assert.AreEqual(arr.ByteOffset, 32, "Should be correct");
		}

		[Test]
		public void ByteLengthPropertyWorks() {
			var arr = new Uint8Array(23);
			Assert.AreEqual(arr.ByteLength, 23, "Should be correct");
		}

		[Test]
		public void IndexOfWorks() {
			var arr = new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(arr.IndexOf(9), 3, "9");
			Assert.AreEqual(arr.IndexOf(1), -1, "1");
		}

		[Test]
		public void ContainsWorks() {
			var arr = new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			Assert.IsTrue (arr.Contains(9), "9");
			Assert.IsFalse(arr.Contains(1), "1");
		}

		[Test]
		public void ForeachWorks() {
			var arr = new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			foreach (var i in arr) {
				l.Add(i);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void GetEnumeratorWorks() {
			var arr = new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void IEnumerableGetEnumeratorWorks() {
			var arr = (IEnumerable<byte>)new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void ICollectionMethodsWork() {
			var coll = (ICollection<sbyte>)new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
			Assert.Throws(() => coll.Add(2), ex => ex is NotSupportedException, "Add");
			Assert.Throws(() => coll.Clear(), ex => ex is NotSupportedException, "Clear");
			Assert.Throws(() => coll.Remove(2), ex => ex is NotSupportedException, "Remove");
		}

		[Test]
		public void IListMethodsWork() {
			var list = (IList<sbyte>)new Uint8Array(new byte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(list.IndexOf(6), 1, "IndexOf(6)");
			Assert.AreEqual(list.IndexOf(1), -1, "IndexOf(1)");
			Assert.AreEqual(list[3], 9, "Get item");
			list[3] = 4;
			Assert.AreEqual(list[3], 4, "Set item");

			Assert.Throws(() => list.Insert(2, 2), ex => ex is NotSupportedException, "Insert");
			Assert.Throws(() => list.RemoveAt(2), ex => ex is NotSupportedException, "RemoveAt");
		}
	}
}
