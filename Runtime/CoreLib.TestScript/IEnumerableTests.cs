using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class IEnumerableTests {
		private class MyEnumerable : IEnumerable<string> {
			public IEnumerator<string> GetEnumerator() {
				yield return "x";
				yield return "y";
				yield return "z";
			}

			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}

		[Test]
		public void ArrayImplementsIEnumerable() {
			Assert.IsTrue((object)new int[1] is IEnumerable<int>);
		}

		[Test]
		public void CustomClassThatShouldImplementIEnumerableDoesSo() {
			Assert.IsTrue((object)new MyEnumerable() is IEnumerable<string>);
		}

		[Test]
		public void ArrayGetEnumeratorMethodWorks() {
			ArrayEnumerator e = new[] { "x", "y", "z" }.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "z");
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void ArrayCastToIEnumerableCanBeEnumerated() {
			IEnumerable<string> enm = new[] { "x", "y", "z" };
			var e = enm.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "z");
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void ClassImplementingIEnumerableCanBeEnumerated() {
			MyEnumerable enm = new MyEnumerable();
			var e = enm.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "z");
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void ClassImplementingIEnumerableCastToIEnumerableCanBeEnumerated() {
			IEnumerable<string> enm = new MyEnumerable();
			var e = enm.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "z");
			Assert.IsFalse(e.MoveNext());
		}
	}
}
