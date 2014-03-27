using System;
using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript.Collections.Generic {
	[TestFixture]
	public class ComparerTests {
		class C : IComparable<C> {
			public int value;

			public C(int value) {
				this.value = value;
			}

			public int CompareTo(C other) {
				return this.value.CompareTo(other.value);
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Comparer<object>).FullName, "ss.Comparer", "FullName should be correct");
			Assert.IsTrue(typeof(Comparer<object>).IsClass, "IsClass should be true");
			Assert.AreStrictEqual(typeof(Comparer<object>).BaseType, typeof(object), "BaseType should be correct");
			object comparer = Comparer<object>.Default;
			Assert.IsTrue(comparer is Comparer<object>, "is Comparer<object> should be true");
			Assert.IsTrue(comparer is IComparer<object>, "is IComparer<object> should be true");
		}

		[Test]
		public void DefaultComparerCanOrderNumbers() {
			Assert.AreEqual(Comparer<int>.Default.Compare(3, 8), -1, "Compare(3, 8) should be -1");
			Assert.AreEqual(Comparer<int>.Default.Compare(3, 3), 0, "Compare(3, 3) should be 0");
			Assert.AreEqual(Comparer<int>.Default.Compare(8, 3), 1, "Compare(8, 3) should be 1");
		}

		[Test]
		public void DefaultComparerCanOrderNullValues() {
			Assert.AreEqual(Comparer<int?>.Default.Compare(0, null), 1, "Compare(0, null) should be 1");
			Assert.AreEqual(Comparer<int?>.Default.Compare(null, 0), -1, "Compare(null, 0) should be -1");
			Assert.AreEqual(Comparer<int?>.Default.Compare(null, null), 0, "Compare(null, null) should be 0");
		}

		[Test]
		public void DefaultComparerUsesCompareMethodIfClassImplementsIComparable() {
			Assert.AreEqual(Comparer<C>.Default.Compare(new C(3), new C(8)), -1, "Compare(3, 8) should be -1");
			Assert.AreEqual(Comparer<C>.Default.Compare(new C(3), new C(3)), 0, "Compare(3, 3) should be 0");
			Assert.AreEqual(Comparer<C>.Default.Compare(new C(8), new C(3)), 1, "Compare(8, 3) should be 1");
		}

		[Test]
		public void CreateWorks() {
			var comparer = Comparer<int>.Create((x, y) => {
				Assert.AreEqual(x, 8, "x should be 8");
				Assert.AreEqual(y, 3, "y should be 3");
				return 42;
			});
			Assert.AreEqual(comparer.Compare(8, 3), 42, "The result should be 42");
		}
	}
}
