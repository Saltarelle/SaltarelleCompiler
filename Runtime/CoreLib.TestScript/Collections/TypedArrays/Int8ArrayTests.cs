using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.TypedArrays;
using QUnit;

namespace CoreLib.TestScript.Collections.TypedArrays {
	[TestFixture]
	public class Int8ArrayTests {
		private void AssertContent(Int8Array actual, int[] expected, string message) {
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
			Assert.AreEqual(typeof(Int8Array).FullName, "Int8Array", "FullName");

			var interfaces = typeof(Int8Array).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 5, "Interface count should be 5");
			Assert.IsTrue(interfaces.Contains(typeof(IEnumerable<sbyte>)), "Interfaces should contain IEnumerable<sbyte>");
			Assert.IsTrue(interfaces.Contains(typeof(ICollection<sbyte>)), "Interfaces should contain ICollection<sbyte>");
			Assert.IsTrue(interfaces.Contains(typeof(IReadOnlyCollection<sbyte>)), "Interfaces should contain IReadOnlyCollection<sbyte>");
			Assert.IsTrue(interfaces.Contains(typeof(IList<sbyte>)), "Interfaces should contain IList<sbyte>");
			Assert.IsTrue(interfaces.Contains(typeof(IReadOnlyList<sbyte>)), "Interfaces should contain IReadOnlyList<sbyte>");

			object arr = new Int8Array(0);
			Assert.IsTrue(arr is Int8Array, "Is Int8Array");
			Assert.IsTrue(arr is IEnumerable<sbyte>, "Is IEnumerable<sbyte>");
			Assert.IsTrue(arr is ICollection<sbyte>, "Is ICollection<sbyte>");
			Assert.IsTrue(arr is IReadOnlyCollection<sbyte>, "Is IReadOnlyCollection<sbyte>");
			Assert.IsTrue(arr is IList<sbyte>, "Is IList<sbyte>");
			Assert.IsTrue(arr is IReadOnlyList<sbyte>, "Is IReadOnlyList<sbyte>");
		}

		[Test]
		public void LengthConstructorWorks() {
			var arr = new Int8Array(13);
			Assert.IsTrue((object)arr is Int8Array, "is Int8Array");
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void ConstructorFromIntWorks() {
			var source = new sbyte[] { 3, 8, 4 };
			var arr = new Int8Array(source);
			Assert.IsTrue((object)arr != (object)source, "New object");
			Assert.IsTrue((object)arr is Int8Array, "is Int8Array");
			AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void CopyConstructorWorks() {
			 var source = new Int8Array(new sbyte[] { 3, 8, 4 });
			 var arr = new Int8Array(source);
			 Assert.IsTrue(arr != source, "New object");
			 Assert.IsTrue((object)arr is Int8Array, "is Int8Array");
			 AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void ArrayBufferConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Int8Array(buf);
			Assert.IsTrue((object)arr is Int8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 80, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Int8Array(buf, 16);
			Assert.IsTrue((object)arr is Int8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 64, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetAndLengthConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Int8Array(buf, 16, 12);
			Assert.IsTrue((object)arr is Int8Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 12, "length");
		}

		[Test]
		public void InstanceBytesPerElementWorks() {
			Assert.AreEqual(new Int8Array(0).BytesPerElement, 1);
		}

		[Test]
		public void StaticBytesPerElementWorks() {
			Assert.AreEqual(Int8Array.BytesPerElementStatic, 1);
		}

		[Test]
		public void LengthWorks() {
			var arr = new Int8Array(13);
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void IndexingWorks() {
			var arr = new Int8Array(3);
			arr[1] = 42;
			AssertContent(arr, new[] { 0, 42, 0 }, "Content");
			Assert.AreEqual(arr[1], 42, "[1]");
		}

		[Test]
		public void SetInt8ArrayWorks() {
			var arr = new Int8Array(4);
			arr.Set(new Int8Array(new sbyte[] { 3, 6, 7 }));
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetInt8ArrayWithOffsetWorks() {
			var arr = new Int8Array(6);
			arr.Set(new Int8Array(new sbyte[] { 3, 6, 7 }), 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWorks() {
			var arr = new Int8Array(4);
			arr.Set(new sbyte[] { 3, 6, 7 });
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWithOffsetWorks() {
			var arr = new Int8Array(6);
			arr.Set(new sbyte[] { 3, 6, 7 }, 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SubarrayWithBeginWorks() {
			var source = new Int8Array(10);
			var arr = source.Subarray(3);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 3, "ByteOffset should be correct");
		}

		[Test]
		public void SubarrayWithBeginAndEndWorks() {
			var source = new Int8Array(10);
			var arr = source.Subarray(3, 7);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 3, "ByteOffset should be correct");
			Assert.AreEqual(arr.Length, 4, "Length should be correct");
		}

		[Test]
		public void BufferPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Int8Array(buf);
			Assert.IsTrue(arr.Buffer == buf, "Should be correct");
		}

		[Test]
		public void ByteOffsetPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Int8Array(buf, 32);
			Assert.AreEqual(arr.ByteOffset, 32, "Should be correct");
		}

		[Test]
		public void ByteLengthPropertyWorks() {
			var arr = new Int8Array(23);
			Assert.AreEqual(arr.ByteLength, 23, "Should be correct");
		}

		[Test]
		public void IndexOfWorks() {
			var arr = new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(arr.IndexOf(9), 3, "9");
			Assert.AreEqual(arr.IndexOf(1), -1, "1");
		}

		[Test]
		public void ContainsWorks() {
			var arr = new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.IsTrue (arr.Contains(9), "9");
			Assert.IsFalse(arr.Contains(1), "1");
		}

		[Test]
		public void ForeachWorks() {
			var arr = new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			foreach (var i in arr) {
				l.Add(i);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void GetEnumeratorWorks() {
			var arr = new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void IEnumerableGetEnumeratorWorks() {
			var arr = (IEnumerable<sbyte>)new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void ICollectionMethodsWork() {
			var coll = (ICollection<sbyte>)new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
			Assert.Throws(() => coll.Add(2), ex => ex is NotSupportedException, "Add");
			Assert.Throws(() => coll.Clear(), ex => ex is NotSupportedException, "Clear");
			Assert.Throws(() => coll.Remove(2), ex => ex is NotSupportedException, "Remove");
		}

		[Test]
		public void IListMethodsWork() {
			var list = (IList<sbyte>)new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(list.IndexOf(6), 1, "IndexOf(6)");
			Assert.AreEqual(list.IndexOf(1), -1, "IndexOf(1)");
			Assert.AreEqual(list[3], 9, "Get item");
			list[3] = 4;
			Assert.AreEqual(list[3], 4, "Set item");

			Assert.Throws(() => list.Insert(2, 2), ex => ex is NotSupportedException, "Insert");
			Assert.Throws(() => list.RemoveAt(2), ex => ex is NotSupportedException, "RemoveAt");
		}


		[Test]
		public void IReadOnlyCollectionMethodsWork() {
			var coll = (IReadOnlyCollection<sbyte>)new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
		}

		[Test]
		public void IReadOnlyListMethodsWork() {
			var list = (IReadOnlyList<sbyte>)new Int8Array(new sbyte[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(list[3], 9, "Get item");
		}
	}
}
