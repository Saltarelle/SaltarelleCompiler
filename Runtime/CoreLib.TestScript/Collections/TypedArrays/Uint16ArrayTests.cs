using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.TypedArrays;
using QUnit;

namespace CoreLib.TestScript.Collections.TypedArrays {
	[TestFixture]
	public class Uint16ArrayTests {
		private void AssertContent(Uint16Array actual, int[] expected, string message) {
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
			Assert.AreEqual(typeof(Uint16Array).FullName, "Uint16Array", "FullName");

			var interfaces = typeof(Uint16Array).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 5, "Interface count should be 5");
			Assert.IsTrue(interfaces.Contains(typeof(IEnumerable<ushort>)), "Interfaces should contain IEnumerable<ushort>");
			Assert.IsTrue(interfaces.Contains(typeof(ICollection<ushort>)), "Interfaces should contain ICollection<ushort>");
			Assert.IsTrue(interfaces.Contains(typeof(IReadOnlyCollection<ushort>)), "Interfaces should contain IReadOnlyCollection<ushort>");
			Assert.IsTrue(interfaces.Contains(typeof(IList<ushort>)), "Interfaces should contain IList<ushort>");
			Assert.IsTrue(interfaces.Contains(typeof(IReadOnlyList<ushort>)), "Interfaces should contain IReadOnlyList<ushort>");

			object arr = new Uint16Array(0);
			Assert.IsTrue(arr is Uint16Array, "Is Uint16Array");
			Assert.IsTrue(arr is IEnumerable<ushort>, "Is IEnumerable<ushort>");
			Assert.IsTrue(arr is ICollection<ushort>, "Is ICollection<ushort>");
			Assert.IsTrue(arr is IReadOnlyCollection<short>, "Is IReadOnlyCollection<ushort>");
			Assert.IsTrue(arr is IList<ushort>, "Is IList<ushort>");
			Assert.IsTrue(arr is IReadOnlyList<ushort>, "Is IReadOnlyList<ushort>");
		}

		[Test]
		public void LengthConstructorWorks() {
			var arr = new Uint16Array(13);
			Assert.IsTrue((object)arr is Uint16Array, "is Uint16Array");
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void ConstructorFromIntWorks() {
			var source = new ushort[] { 3, 8, 4 };
			var arr = new Uint16Array(source);
			Assert.IsTrue((object)arr != (object)source, "New object");
			Assert.IsTrue((object)arr is Uint16Array, "is Uint16Array");
			AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void CopyConstructorWorks() {
			 var source = new Uint16Array(new ushort[] { 3, 8, 4 });
			 var arr = new Uint16Array(source);
			 Assert.IsTrue(arr != source, "New object");
			 Assert.IsTrue((object)arr is Uint16Array, "is Uint16Array");
			 AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void ArrayBufferConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint16Array(buf);
			Assert.IsTrue((object)arr is Uint16Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 40, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint16Array(buf, 16);
			Assert.IsTrue((object)arr is Uint16Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 32, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetAndLengthConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Uint16Array(buf, 16, 12);
			Assert.IsTrue((object)arr is Uint16Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 12, "length");
		}

		[Test]
		public void InstanceBytesPerElementWorks() {
			Assert.AreEqual(new Uint16Array(0).BytesPerElement, 2);
		}

		[Test]
		public void StaticBytesPerElementWorks() {
			Assert.AreEqual(Uint16Array.BytesPerElementStatic, 2);
		}

		[Test]
		public void LengthWorks() {
			var arr = new Uint16Array(13);
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void IndexingWorks() {
			var arr = new Uint16Array(3);
			arr[1] = 42;
			AssertContent(arr, new[] { 0, 42, 0 }, "Content");
			Assert.AreEqual(arr[1], 42, "[1]");
		}

		[Test]
		public void SetUint16ArrayWorks() {
			var arr = new Uint16Array(4);
			arr.Set(new Uint16Array(new ushort[] { 3, 6, 7 }));
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetUint16ArrayWithOffsetWorks() {
			var arr = new Uint16Array(6);
			arr.Set(new Uint16Array(new ushort[] { 3, 6, 7 }), 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWorks() {
			var arr = new Uint16Array(4);
			arr.Set(new ushort[] { 3, 6, 7 });
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWithOffsetWorks() {
			var arr = new Uint16Array(6);
			arr.Set(new ushort[] { 3, 6, 7 }, 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SubarrayWithBeginWorks() {
			var source = new Uint16Array(10);
			var arr = source.Subarray(3);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 6, "ByteOffset should be correct");
		}

		[Test]
		public void SubarrayWithBeginAndEndWorks() {
			var source = new Uint16Array(10);
			var arr = source.Subarray(3, 7);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 6, "ByteOffset should be correct");
			Assert.AreEqual(arr.Length, 4, "Length should be correct");
		}

		[Test]
		public void BufferPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Uint16Array(buf);
			Assert.IsTrue(arr.Buffer == buf, "Should be correct");
		}

		[Test]
		public void ByteOffsetPropertyWorks() {
			var buf = new ArrayBuffer(100);
			var arr = new Uint16Array(buf, 32);
			Assert.AreEqual(arr.ByteOffset, 32, "Should be correct");
		}

		[Test]
		public void ByteLengthPropertyWorks() {
			var arr = new Uint16Array(23);
			Assert.AreEqual(arr.ByteLength, 46, "Should be correct");
		}

		[Test]
		public void IndexOfWorks() {
			var arr = new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(arr.IndexOf(9), 3, "9");
			Assert.AreEqual(arr.IndexOf(1), -1, "1");
		}

		[Test]
		public void ContainsWorks() {
			var arr = new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			Assert.IsTrue (arr.Contains(9), "9");
			Assert.IsFalse(arr.Contains(1), "1");
		}

		[Test]
		public void ForeachWorks() {
			var arr = new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			foreach (var i in arr) {
				l.Add(i);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void GetEnumeratorWorks() {
			var arr = new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void IEnumerableGetEnumeratorWorks() {
			var arr = (IEnumerable<ushort>)new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			var l = new List<int>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void ICollectionMethodsWork() {
			var coll = (ICollection<ushort>)new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
			Assert.Throws(() => coll.Add(2), ex => ex is NotSupportedException, "Add");
			Assert.Throws(() => coll.Clear(), ex => ex is NotSupportedException, "Clear");
			Assert.Throws(() => coll.Remove(2), ex => ex is NotSupportedException, "Remove");
		}

		[Test]
		public void IListMethodsWork() {
			var list = (IList<ushort>)new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
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
			var coll = (IReadOnlyCollection<ushort>)new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
		}

		[Test]
		public void IReadOnlyListMethodsWork() {
			var list = (IReadOnlyList<ushort>)new Uint16Array(new ushort[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(list[3], 9, "Get item");
		}
	}
}
