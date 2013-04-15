using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.TypedArrays;
using QUnit;

namespace CoreLib.TestScript.Collections.TypedArrays {
	[TestFixture]
	public class Float64ArrayTests {
		private void AssertContent(Float64Array actual, int[] expected, string message) {
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
			Assert.AreEqual(typeof(Float64Array).FullName, "Float64Array", "FullName");

			var interfaces = typeof(Float64Array).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3, "Interface count should be 3");
			Assert.IsTrue(interfaces.Contains(typeof(IEnumerable<double>)), "Interfaces should contain IEnumerable<double>");
			Assert.IsTrue(interfaces.Contains(typeof(ICollection<double>)), "Interfaces should contain ICollection<double>");
			Assert.IsTrue(interfaces.Contains(typeof(IList<double>)), "Interfaces should contain IList<double>");

			object arr = new Float64Array(0);
			Assert.IsTrue(arr is Float64Array, "Is Float64Array");
			Assert.IsTrue(arr is IEnumerable<double>, "Is IEnumerable<double>");
			Assert.IsTrue(arr is ICollection<double>, "Is ICollection<double>");
			Assert.IsTrue(arr is IList<double>, "Is IList<double>");
		}

		[Test]
		public void LengthConstructorWorks() {
			var arr = new Float64Array(13);
			Assert.IsTrue((object)arr is Float64Array, "is Float64Array");
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void ConstructorFromIntWorks() {
			var source = new double[] { 3, 8, 4 };
			var arr = new Float64Array(source);
			Assert.IsTrue((object)arr != (object)source, "New object");
			Assert.IsTrue((object)arr is Float64Array, "is Float64Array");
			AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void CopyConstructorWorks() {
			 var source = new Float64Array(new double[] { 3, 8, 4 });
			 var arr = new Float64Array(source);
			 Assert.IsTrue(arr != source, "New object");
			 Assert.IsTrue((object)arr is Float64Array, "is Float64Array");
			 AssertContent(arr, new[] { 3, 8, 4 }, "content");
		}

		[Test]
		public void ArrayBufferConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Float64Array(buf);
			Assert.IsTrue((object)arr is Float64Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 10, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Float64Array(buf, 8);
			Assert.IsTrue((object)arr is Float64Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 9, "length");
		}

		[Test]
		public void ArrayBufferWithOffsetAndLengthConstructorWorks() {
			var buf = new ArrayBuffer(80);
			var arr = new Float64Array(buf, 16, 6);
			Assert.IsTrue((object)arr is Float64Array);
			Assert.IsTrue(arr.Buffer == buf, "buffer");
			Assert.AreEqual(arr.Length, 6, "length");
		}

		[Test]
		public void InstanceBytesPerElementWorks() {
			Assert.AreEqual(new Float64Array(0).BytesPerElement, 8);
		}

		[Test]
		public void StaticBytesPerElementWorks() {
			Assert.AreEqual(Float64Array.BytesPerElementStatic, 8);
		}

		[Test]
		public void LengthWorks() {
			var arr = new Float64Array(13);
			Assert.AreEqual(arr.Length, 13, "Length");
		}

		[Test]
		public void IndexingWorks() {
			var arr = new Float64Array(3);
			arr[1] = 42;
			AssertContent(arr, new[] { 0, 42, 0 }, "Content");
			Assert.AreEqual(arr[1], 42, "[1]");
		}

		[Test]
		public void SetFloat64ArrayWorks() {
			var arr = new Float64Array(4);
			arr.Set(new Float64Array(new double[] { 3, 6, 7 }));
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetFloat64ArrayWithOffsetWorks() {
			var arr = new Float64Array(6);
			arr.Set(new Float64Array(new double[] { 3, 6, 7 }), 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWorks() {
			var arr = new Float64Array(4);
			arr.Set(new double[] { 3, 6, 7 });
			AssertContent(arr, new[] { 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SetNormalArrayWithOffsetWorks() {
			var arr = new Float64Array(6);
			arr.Set(new double[] { 3, 6, 7 }, 2);
			AssertContent(arr, new[] { 0, 0, 3, 6, 7, 0 }, "Content");
		}

		[Test]
		public void SubarrayWithBeginWorks() {
			var source = new Float64Array(10);
			var arr = source.Subarray(3);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 24, "ByteOffset should be correct");
		}

		[Test]
		public void SubarrayWithBeginAndEndWorks() {
			var source = new Float64Array(10);
			var arr = source.Subarray(3, 7);
			Assert.IsFalse(arr == source, "Should be a new array");
			Assert.IsTrue(arr.Buffer == source.Buffer, "Should be the same buffer");
			Assert.AreEqual(arr.ByteOffset, 24, "ByteOffset should be correct");
			Assert.AreEqual(arr.Length, 4, "Length should be correct");
		}

		[Test]
		public void BufferPropertyWorks() {
			var buf = new ArrayBuffer(104);
			var arr = new Float64Array(buf);
			Assert.IsTrue(arr.Buffer == buf, "Should be correct");
		}

		[Test]
		public void ByteOffsetPropertyWorks() {
			var buf = new ArrayBuffer(104);
			var arr = new Float64Array(buf, 56);
			Assert.AreEqual(arr.ByteOffset, 56, "Should be correct");
		}

		[Test]
		public void ByteLengthPropertyWorks() {
			var arr = new Float64Array(23);
			Assert.AreEqual(arr.ByteLength, 184, "Should be correct");
		}

		[Test]
		public void IndexOfWorks() {
			var arr = new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(arr.IndexOf(9), 3, "9");
			Assert.AreEqual(arr.IndexOf(1), -1, "1");
		}

		[Test]
		public void ContainsWorks() {
			var arr = new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			Assert.IsTrue (arr.Contains(9), "9");
			Assert.IsFalse(arr.Contains(1), "1");
		}

		[Test]
		public void ForeachWorks() {
			var arr = new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			var l = new List<double>();
			foreach (var i in arr) {
				l.Add(i);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void GetEnumeratorWorks() {
			var arr = new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			var l = new List<double>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void IEnumerableGetEnumeratorWorks() {
			var arr = (IEnumerable<double>)new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			var l = new List<double>();
			var enm = arr.GetEnumerator();
			while (enm.MoveNext()) {
				l.Add(enm.Current);
			}
			Assert.AreEqual(l, new[] { 3, 6, 2, 9, 5 });
		}

		[Test]
		public void ICollectionMethodsWork() {
			var coll = (ICollection<double>)new Float64Array(new double[] { 3, 6, 2, 9, 5 });
			Assert.AreEqual(coll.Count, 5, "Count");
			Assert.IsTrue(coll.Contains(6), "Contains(6)");
			Assert.IsFalse(coll.Contains(1), "Contains(1)");
			Assert.Throws(() => coll.Add(2), ex => ex is NotSupportedException, "Add");
			Assert.Throws(() => coll.Clear(), ex => ex is NotSupportedException, "Clear");
			Assert.Throws(() => coll.Remove(2), ex => ex is NotSupportedException, "Remove");
		}

		[Test]
		public void IListMethodsWork() {
			var list = (IList<double>)new Float64Array(new double[] { 3, 6, 2, 9, 5 });
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
