using System.Collections.Generic;
using System.Collections.ObjectModel;
using QUnit;

namespace CoreLib.TestScript.Collections.ObjectModel {
	[TestFixture]
	public class ReadOnlyCollectionTests {
		private class C {
			public readonly int i;

			public C(int i) {
				this.i = i;
			}

			public override bool Equals(object o) {
				return o is C && i == ((C)o).i;
			}
			public override int GetHashCode() {
				return i;
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ReadOnlyCollection<int>).FullName, "Array", "FullName should be Array");
			Assert.IsTrue(typeof(ReadOnlyCollection<int>).IsClass, "IsClass should be true");
			object list = new ReadOnlyCollection<int>(new int[0]);
			Assert.IsTrue(list is ReadOnlyCollection<int>, "is ReadOnlyCollection<int> should be true");
			Assert.IsTrue(list is IList<int>, "is IList<int> should be true");
			Assert.IsTrue(list is ICollection<int>, "is ICollection<int> should be true");
			Assert.IsTrue(list is IEnumerable<int>, "is IEnumerable<int> should be true");
		}

		[Test]
		public void ConstructorWorks() {
			var l = new ReadOnlyCollection<int>(new int[] { 41, 42, 43 });
			Assert.AreEqual(l.Count, 3);
			Assert.AreEqual(l[0], 41);
			Assert.AreEqual(l[1], 42);
			Assert.AreEqual(l[2], 43);
		}

		[Test]
		public void CountWorks() {
			Assert.AreEqual(new ReadOnlyCollection<string>(new string[0]).Count, 0);
			Assert.AreEqual(new ReadOnlyCollection<string>(new string[1]).Count, 1);
			Assert.AreEqual(new ReadOnlyCollection<string>(new string[2]).Count, 2);
		}

		[Test]
		public void IndexingWorks() {
			var l = new ReadOnlyCollection<string>(new[] { "x", "y" });
			Assert.AreEqual(l[0], "x");
			Assert.AreEqual(l[1], "y");
		}

		[Test]
		public void ForeachWorks() {
			string result = "";
			foreach (var s in new ReadOnlyCollection<string>(new[] { "x", "y" })) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void GetEnumeratorWorks() {
			var e = new ReadOnlyCollection<string>(new[] { "x", "y" }).GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void ContainsWorks() {
			var l = new ReadOnlyCollection<string>(new[] { "x", "y" });
			Assert.IsTrue(l.Contains("x"));
			Assert.IsFalse(l.Contains("z"));
		}

		[Test]
		public void ContainsUsesEqualsMethod() {
			var l = new ReadOnlyCollection<C>(new[] { new C(1), new C(2), new C(3) });
			Assert.IsTrue(l.Contains(new C(2)));
			Assert.IsFalse(l.Contains(new C(4)));
		}

		[Test]
		public void IndexOfWorks() {
			Assert.AreEqual(new ReadOnlyCollection<string>(new[] { "a", "b", "c", "b" }).IndexOf("b"), 1);
			Assert.AreEqual(new ReadOnlyCollection<C>(new[] { new C(1), new C(2), new C(3), new C(2) }).IndexOf(new C(2)), 1);
		}

		[Test]
		public void ForeachWhenCastToIEnumerableWorks() {
			IEnumerable<string> list = new ReadOnlyCollection<string>(new[] { "x", "y" });
			string result = "";
			foreach (var s in list) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void IEnumerableGetEnumeratorWorks() {
			var l = (IEnumerable<string>)new ReadOnlyCollection<string>(new[] { "x", "y" });
			var e = l.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "x");
			Assert.IsTrue(e.MoveNext());
			Assert.AreEqual(e.Current, "y");
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void ICollectionCountWorks() {
			IList<string> l = new ReadOnlyCollection<string>(new[] { "x", "y", "z" });
			Assert.AreEqual(l.Count, 3);
		}

		[Test]
		public void ICollectionContainsWorks() {
			IList<string> l = new ReadOnlyCollection<string>(new[] { "x", "y", "z" });
			Assert.IsTrue(l.Contains("y"));
			Assert.IsFalse(l.Contains("a"));
		}

		[Test]
		public void ICollectionContainsUsesEqualsMethod() {
			IList<C> l = new ReadOnlyCollection<C>(new[] { new C(1), new C(2), new C(3) });
			Assert.IsTrue(l.Contains(new C(2)));
			Assert.IsFalse(l.Contains(new C(4)));
		}

		[Test]
		public void IListIndexingWorks() {
			IList<string> l = new ReadOnlyCollection<string>(new[] { "x", "y", "z" });
			Assert.AreEqual(l[1], "y");
		}

		[Test]
		public void IListIndexOfWorks() {
			IList<string> l = new ReadOnlyCollection<string>(new[] { "x", "y", "z" });
			Assert.AreEqual(l.IndexOf("y"), 1);
			Assert.AreEqual(l.IndexOf("a"), -1);
		}

		[Test]
		public void IListIndexOfUsesEqualsMethod() {
			IList<C> l = new ReadOnlyCollection<C>(new[] { new C(1), new C(2), new C(3) });
			Assert.AreEqual(l.IndexOf(new C(2)), 1);
			Assert.AreEqual(l.IndexOf(new C(4)), -1);
		}
	}
}
