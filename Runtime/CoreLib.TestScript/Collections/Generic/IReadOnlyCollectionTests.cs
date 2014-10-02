using System.Collections;
using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript.Collections.Generic {
	[TestFixture]
	public class IReadOnlyCollectionTests {
		private class MyCollection : IReadOnlyCollection<string> {
			public MyCollection(string[] items) {
				Items = new List<string>(items);
			}

			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public List<string> Items { get; private set; }

			public IEnumerator<string> GetEnumerator() { return Items.GetEnumerator(); }
			public int Count { get { return Items.Count; } }
			public bool Contains(string item) { return Items.Contains(item); }
		}

		private class C {
			private readonly int _i;

			public C(int i) {
				_i = i;
			}

			public override bool Equals(object o) {
				return o is C && _i == ((C)o)._i;
			}
			public override int GetHashCode() {
				return _i;
			}
		}

		[Test]
		public void ArrayImplementsIReadOnlyCollection()
		{
			Assert.IsTrue((object)new int[1] is IReadOnlyCollection<int>);
		}

		[Test]
		public void CustomClassThatShouldImplementIReadOnlyCollectionDoesSo()
		{
			Assert.IsTrue((object)new MyCollection(new string[0]) is IReadOnlyCollection<string>);
		}

		[Test]
		public void ArrayCastToIReadOnlyCollectionCountWorks()
		{
			Assert.AreEqual(((IReadOnlyCollection<string>)new[] { "x", "y", "z" }).Count, 3);
		}

		[Test]
		public void ClassImplementingICollectionCastToIReadOnlyCollectionCountWorks()
		{
			Assert.AreEqual(((IReadOnlyCollection<string>)new MyCollection(new[] { "x", "y", "z" })).Count, 3);
		}

		[Test]
		public void ArrayCastToIReadOnlyCollectionContainsWorks()
		{
			IReadOnlyCollection<C> arr = new[] { new C(1), new C(2), new C(3) };
			Assert.IsTrue(arr.Contains(new C(2)));
			Assert.IsFalse(arr.Contains(new C(4)));
		}

		[Test]
		public void ClassImplementingICollectionCastToIReadOnlyCollectionContainsWorks()
		{
			IReadOnlyCollection<string> c = new MyCollection(new[] { "x", "y" });
			Assert.IsTrue(c.Contains("x"));
			Assert.IsFalse(c.Contains("z"));
		}
	}
}
