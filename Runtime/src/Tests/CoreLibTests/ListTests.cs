using System;
using System.Collections;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class ListTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(List<int>).FullName, "Array", "FullName should be Array");
			Assert.IsTrue(typeof(List<int>).IsClass, "IsClass should be true");
			object list = new List<int>();
			Assert.IsTrue(list is List<int>, "is int[] should be true");
			Assert.IsTrue(list is IList<int>, "is IList<int> should be true");
			Assert.IsTrue(list is ICollection<int>, "is ICollection<int> should be true");
			Assert.IsTrue(list is IEnumerable<int>, "is IEnumerable<int> should be true");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var l = new List<int>();
			Assert.AreEqual(l.Count, 0);
		}

		[Test]
		public void ConstructorWithCapacityWorks() {
			var l = new List<int>(12);
			Assert.AreEqual(l.Count, 0);
		}

		[Test]
		public void ParamArrayConstructorWorks() {
			var l = new List<int>(1, 4, 7, 8);
			Assert.AreEqual(l, new[] { 1, 4, 7, 8 });
		}

		[Test]
		public void ConstructingFromArrayWorks() {
			var arr = new[] { 1, 4, 7, 8 };
			var l = new List<int>(arr);
			Assert.IsFalse((object)l == (object)arr);
			Assert.AreEqual(l, arr);
		}

		[Test]
		public void ConstructingFromListWorks() {
			var arr = new List<int>(1, 4, 7, 8);
			var l = new List<int>(arr);
			Assert.IsFalse((object)l == (object)arr);
			Assert.AreEqual(l, arr);
		}

		[Test]
		public void ConstructingFromIEnumerableWorks() {
			var enm = (IEnumerable<int>)new List<int>(1, 4, 7, 8);
			var l = new List<int>(enm);
			Assert.IsFalse((object)l == (object)enm);
			Assert.AreEqual(l, new[] { 1, 4, 7, 8 });
		}

		[Test]
		public void CountWorks() {
			Assert.AreEqual(new List<string>().Count, 0);
			Assert.AreEqual(new List<string> { "x" }.Count, 1);
			Assert.AreEqual(new List<string> { "x", "y" }.Count, 2);
		}

		[Test]
		public void IndexingWorks() {
			Assert.AreEqual(new List<string> { "x", "y"}[0], "x");
			Assert.AreEqual(new List<string> { "x", "y"}[1], "y");
		}

		[Test]
		public void ForeachWorks() {
			string result = "";
			foreach (var s in new List<string> { "x", "y" }) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void AddWorks() {
			var l = new List<string> { "x", "y"};
			l.Add("a");
			Assert.AreEqual(l, new[] { "x", "y", "a" });
		}

		[Test]
		public void AddRangeWorks() {
			var l = new List<string> { "x", "y"};
			l.AddRange(new[] { "a", "b", "c" });
			Assert.AreEqual(l, new[] { "x", "y", "a", "b", "c" });
		}

		[Test]
		public void CloneWorks() {
			var l1 = new List<string> { "x", "y" };
			var l2 = l1.Clone();
			Assert.IsFalse(l1 == l2);
			Assert.AreEqual(l1, l2);
		}

		[Test]
		public void ClearWorks() {
			var l = new List<string> { "x", "y"};
			l.Clear();
			Assert.AreEqual(0, l.Count);
		}

		[Test]
		public void ConcatWorks() {
			var list = new List<string> { "a", "b" };
			Assert.AreEqual(list.Concat("c"), new[] { "a", "b", "c" });
			Assert.AreEqual(list.Concat("c", "d"), new[] { "a", "b", "c", "d" });
			Assert.AreEqual(list, new[] { "a", "b" });
		}

		[Test]
		public void ContainsWorks() {
			var list = new List<string> { "x", "y" };
			Assert.IsTrue(list.Contains("x"));
			Assert.IsFalse(list.Contains("z"));
		}

		[Test]
		public void EveryWithListItemFilterCallbackWorks() {
			Assert.IsTrue(new List<int> { 1, 2, 3 }.Every(x => (int)x > 0));
			Assert.IsFalse(new List<int> { 1, 2, 3 }.Every(x => (int)x > 1));
		}

		[Test]
		public void EveryWithListFilterCallbackWorks() {
			var list = new List<int> { 1, 2, 3 };
			Assert.IsTrue(list.Every((x, i, a) => a == list && (int)x == i + 1));
			Assert.IsFalse(list.Every((x, i, a) => (int)x > 1));
		}

		[Test]
		public void ExtractWithoutCountWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "d" }.Extract(2), new[] { "c", "d" });
		}

		[Test]
		public void ExtractWithCountWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "d" }.Extract(1, 2), new[] { "b", "c" });
		}

		[Test]
		public void SliceWithoutEndWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "d" }.Slice(2), new[] { "c", "d" });
		}

		[Test]
		public void SliceWithEndWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "d" }.Slice(1, 3), new[] { "b", "c" });
		}

		[Test]
		public void FilterWithListItemFilterCallbackWorks() {
			Assert.AreEqual(new List<int> { 1, 2, 3, 4 }.Filter(x => (int)x > 1 && (int)x < 4), new[] { 2, 3 });
		}

		[Test]
		public void FilterWithListFilterCallbackWorks() {
			var list = new List<int> { -1, 1, 4, 3 };
			Assert.AreEqual(list.Filter((x, i, a) => a == list && (int)x == i), new[] { 1, 3 });
		}

		[Test]
		public void ForeachWithListItemCallbackWorks() {
			string result = "";
			new List<string> { "a", "b", "c" }.ForEach(s => result += s);
			Assert.AreEqual(result, "abc");
		}

		[Test]
		public void ForeachWithListCallbackWorks() {
			string result = "";
			new List<string> { "a", "b", "c" }.ForEach((s, i, a) => result += (string)s + i);
			Assert.AreEqual(result, "a0b1c2");
		}

		[Test]
		public void IndexOfWithoutStartIndexWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.IndexOf("b"), 1);
		}

		[Test]
		public void IndexOfWithStartIndexWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "b" }.IndexOf("b", 2), 3);
		}

		[Test]
		public void InsertWorks() {
			var l = new List<string> { "x", "y"};
			l.Insert(1, "a");
			Assert.AreEqual(l, new[] { "x", "a", "y" });
		}

		[Test]
		public void InsertRangeWorks() {
			var l = new List<string> { "x", "y"};
			l.InsertRange(1, new[] { "a", "b" });
			Assert.AreEqual(l, new[] { "x", "a", "b", "y" });
		}

		[Test]
		public void JoinWithoutDelimiterWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "b" }.Join(), "a,b,c,b");
		}

		[Test]
		public void JoinWithDelimiterWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "b" }.Join("|"), "a|b|c|b");
		}

		[Test]
		public void MapWithListItemMapCallbackWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "b" }.Map(s => s + "X" + s), new[] { "aXa", "bXb", "cXc", "bXb" });
		}

		[Test]
		public void MapWithListMapCallbackWorks() {
			Assert.AreEqual(new List<string> { "a", "b", "c", "b" }.Map((s, i, a) => (string)s + i), new[] { "a0", "b1", "c2", "b3" });
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(List<int>.Parse("[1,2,3]"), new[] { 1, 2, 3 });
		}

		[Test]
		public void RemoveWorks() {
			var list = new List<string> { "a", "b", "c", "a" };
			list.Remove("a");
			Assert.AreEqual(list, new[] { "b", "c", "a" });
		}

		[Test]
		public void RemoveAtWorks() {
			var list = new List<string> { "a", "b", "c", "a" };
			list.RemoveAt(1);
			Assert.AreEqual(list, new[] { "a", "c", "a" });
		}

		[Test]
		public void RemoveRangeWorks() {
			var list = new List<string> { "a", "b", "c", "d" };
			list.RemoveRange(1, 2);
			Assert.AreEqual(list, new[] { "a", "d" });
		}

		[Test]
		public void ReverseWorks() {
			var list = new List<int> { 1, 3, 4, 1, 3, 2 };
			list.Reverse();
			Assert.AreEqual(list, new[] { 2, 3, 1, 4, 3, 1 });
		}

		[Test]
		public void SomeWithListItemFilterCallbackWorks() {
			Assert.IsTrue(new List<int> { 1, 2, 3, 4 }.Some(i => (int)i > 1));
			Assert.IsFalse(new List<int> { 1, 2, 3, 4 }.Some(i => (int)i > 5));
		}

		[Test]
		public void SomeWithListFilterCallbackWorks() {
			Assert.IsTrue(new List<int> { 1, 1, 6, 2 }.Some((x, i, a) => (int)x == i + 1));
			Assert.IsFalse(new List<int> { 2, 1, 6, 2 }.Some((x, i, a) => (int)x == i + 1));
		}

		[Test]
		public void SortWithDefaultCompareWorks() {
			var list = new List<int> { 1, 6, 6, 4, 2 };
			list.Sort();
			Assert.AreEqual(list, new[] { 1, 2, 4, 6, 6 });
		}

		[Test]
		public void SortWithCompareCallbackWorks() {
			var list = new List<int> { 1, 6, 6, 4, 2 };
			list.Sort((x, y) => (int)y - (int)x);
			Assert.AreEqual(list, new[] { 6, 6, 4, 2, 1 });
		}

		[Test]
		public void ForeachWhenCastToIListWorks() {
			IList<string> list = new List<string> { "x", "y" };
			string result = "";
			foreach (var s in list) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void ICollectionCountWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			Assert.AreEqual(l.Count, 3);
		}

		[Test]
		public void ICollectionAddWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			l.Add("a");
			Assert.AreEqual(l, new[] { "x", "y", "z", "a" });
		}

		[Test]
		public void ICollectionClearWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			l.Clear();
			Assert.AreEqual(l, new string[0]);
		}

		[Test]
		public void ICollectionContainsWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			Assert.IsTrue(l.Contains("y"));
			Assert.IsFalse(l.Contains("a"));
		}

		[Test]
		public void ICollectionRemoveWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			Assert.IsTrue(l.Remove("y"));
			Assert.IsFalse(l.Remove("a"));
			Assert.AreEqual(l, new[] { "x", "z" });
		}

		[Test]
		public void IListIndexingWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			Assert.AreEqual(l[1], "y");
			l[1] = "a";
			Assert.AreEqual(l, new[] { "x", "a", "z" });
		}

		[Test]
		public void IListIndexOfWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			Assert.AreEqual(l.IndexOf("y"), 1);
			Assert.AreEqual(l.IndexOf("a"), -1);
		}

		[Test]
		public void IListInsertWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			l.Insert(1, "a");
			Assert.AreEqual(l, new[] { "x", "a", "y", "z" });
		}

		[Test]
		public void IListRemoveAtWorks() {
			IList<string> l = new List<string> { "x", "y", "z" };
			l.RemoveAt(1);
			Assert.AreEqual(l, new[] { "x", "z" });
		}
	}
}
