using System;
using System.Collections;
using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ArrayTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(int[]).FullName, "Array", "FullName should be Array");
			Assert.IsTrue(typeof(Array).IsClass, "IsClass should be true");
			object arr = new[] { 1, 2, 3 };
			Assert.IsTrue(arr is Array, "is Array should be true");
			Assert.IsTrue(arr is int[], "is int[] should be true");
			Assert.IsTrue(arr is IList<int>, "is IList<int> should be true");
			Assert.IsTrue(arr is ICollection<int>, "is ICollection<int> should be true");
			Assert.IsTrue(arr is IEnumerable<int>, "is IEnumerable<int> should be true");
		}

		[Test]
		public void ArrayCanBeAssignedToTheCollectionInterfaces() {
			Assert.IsTrue(typeof(IEnumerable<int>).IsAssignableFrom(typeof(int[])));
			Assert.IsTrue(typeof(ICollection<int>).IsAssignableFrom(typeof(int[])));
			Assert.IsTrue(typeof(IList<int>).IsAssignableFrom(typeof(int[])));
		}

		[Test]
		public void LengthWorks() {
			Assert.AreEqual(new int[0].Length, 0);
			Assert.AreEqual(new[] { "x" }.Length, 1);
			Assert.AreEqual(new[] { "x", "y" }.Length, 2);
		}

		[Test]
		public void RankIsOne() {
			Assert.AreEqual(new int[0].Rank, 1);
		}

		[Test]
		public void GetLengthWorks() {
			Assert.AreEqual(new int[0].GetLength(0), 0);
			Assert.AreEqual(new[] { "x" }.GetLength(0), 1);
			Assert.AreEqual(new[] { "x", "y" }.GetLength(0), 2);
		}

		[Test]
		public void GetLowerBound() {
			Assert.AreEqual(new int[0].GetLowerBound(0), 0);
			Assert.AreEqual(new[] { "x" }.GetLowerBound(0), 0);
			Assert.AreEqual(new[] { "x", "y" }.GetLowerBound(0), 0);
		}

		[Test]
		public void GetUpperBoundWorks() {
			Assert.AreEqual(new int[0].GetUpperBound(0), -1);
			Assert.AreEqual(new[] { "x" }.GetUpperBound(0), 0);
			Assert.AreEqual(new[] { "x", "y" }.GetUpperBound(0), 1);
		}

		[Test]
		public void GettingValueByIndexWorks() {
			Assert.AreEqual(new[] { "x", "y"}[0], "x");
			Assert.AreEqual(new[] { "x", "y"}[1], "y");
		}
		
		[Test]
		public void GetValueWorks() {
			Assert.AreEqual(new[] { "x", "y"}.GetValue(0), "x");
			Assert.AreEqual(new[] { "x", "y"}.GetValue(1), "y");
		}

		[Test]
		public void SettingValueByIndexWorks() {
			var arr = new string[2];
			arr[0] = "x";
			arr[1] = "y";
			Assert.AreEqual(arr[0], "x");
			Assert.AreEqual(arr[1], "y");
		}
		
		[Test]
		public void SetValueWorks() {
			var arr = new string[2];
			arr.SetValue("x", 0);
			arr.SetValue("y", 1);
			Assert.AreEqual(arr[0], "x");
			Assert.AreEqual(arr[1], "y");
		}

		[Test]
		public void ForeachWorks() {
			string result = "";
			foreach (var s in new[] { "x", "y" }) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void CloneWorks() {
			var arr = new[] { "x", "y" };
			var arr2 = arr.Clone();
			Assert.IsFalse(arr == arr2);
			Assert.AreEqual(arr, arr2);
		}

		[Test]
		public void ConcatWorks() {
			var arr = new[] { "a", "b" };
			Assert.AreEqual(arr.Concat("c"), new[] { "a", "b", "c" });
			Assert.AreEqual(arr.Concat("c", "d"), new[] { "a", "b", "c", "d" });
			Assert.AreEqual(arr, new[] { "a", "b" });
		}

		[Test]
		public void ContainsWorks() {
			var arr = new[] { "x", "y" };
			Assert.IsTrue(arr.Contains("x"));
			Assert.IsFalse(arr.Contains("z"));
		}

		[Test]
		public void EveryWithArrayItemFilterCallbackWorks() {
			Assert.IsTrue(new[] { 1, 2, 3 }.Every(x => x > 0));
			Assert.IsFalse(new[] { 1, 2, 3 }.Every(x => x > 1));
		}

		[Test]
		public void EveryWithArrayFilterCallbackWorks() {
			var arr = new[] { 1, 2, 3 };
			Assert.IsTrue(arr.Every((x, i, a) => a == arr && x == i + 1));
			Assert.IsFalse(arr.Every((x, i, a) => x > 1));
		}

		[Test]
		public void ExtractWithoutCountWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "d" }.Extract(2), new[] { "c", "d" });
		}

		[Test]
		public void ExtractWithCountWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "d" }.Extract(1, 2), new[] { "b", "c" });
		}

		[Test]
		public void SliceWithoutEndWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "d" }.Slice(2), new[] { "c", "d" });
		}

		[Test]
		public void SliceWithEndWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "d" }.Slice(1, 3), new[] { "b", "c" });
		}

		[Test]
		public void FilterWithArrayItemFilterCallbackWorks() {
			Assert.AreEqual(new[] { 1, 2, 3, 4 }.Filter(x => x > 1 && x < 4), new[] { 2, 3 });
		}

		[Test]
		public void FilterWithArrayFilterCallbackWorks() {
			var arr = new[] { -1, 1, 4, 3 };
			Assert.AreEqual(arr.Filter((x, i, a) => a == arr && x == i), new[] { 1, 3 });
		}

		[Test]
		public void ForeachWithArrayItemCallbackWorks() {
			string result = "";
			new[] { "a", "b", "c" }.ForEach(s => result += s);
			Assert.AreEqual(result, "abc");
		}

		[Test]
		public void ForeachWithArrayCallbackWorks() {
			string result = "";
			new[] { "a", "b", "c" }.ForEach((s, i, a) => result += s + i);
			Assert.AreEqual(result, "a0b1c2");
		}

		[Test]
		public void IndexOfWithoutStartIndexWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.IndexOf("b"), 1);
		}

		[Test]
		public void IndexOfWithStartIndexWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.IndexOf("b", 2), 3);
		}

		[Test]
		public void JoinWithoutDelimiterWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.Join(), "a,b,c,b");
		}

		[Test]
		public void JoinWithDelimiterWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.Join("|"), "a|b|c|b");
		}

		[Test]
		public void MapWithArrayItemMapCallbackWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.Map(s => s + "X" + s), new[] { "aXa", "bXb", "cXc", "bXb" });
		}

		[Test]
		public void MapWithArrayMapCallbackWorks() {
			Assert.AreEqual(new[] { "a", "b", "c", "b" }.Map((s, i, a) => s + i), new[] { "a0", "b1", "c2", "b3" });
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(Array.Parse("[1,2,3]"), new[] { 1, 2, 3 });
		}

		[Test]
		public void ReverseWorks() {
			var arr = new[] { 1, 3, 4, 1, 3, 2 };
			arr.Reverse();
			Assert.AreEqual(arr, new[] { 2, 3, 1, 4, 3, 1 });
		}

		[Test]
		public void SomeWithArrayItemFilterCallbackWorks() {
			Assert.IsTrue(new[] { 1, 2, 3, 4 }.Some(i => i > 1));
			Assert.IsFalse(new[] { 1, 2, 3, 4 }.Some(i => i > 5));
		}

		[Test]
		public void SomeWithArrayFilterCallbackWorks() {
			Assert.IsTrue(new[] { 1, 1, 6, 2 }.Some((x, i, a) => x == i + 1));
			Assert.IsFalse(new[] { 2, 1, 6, 2 }.Some((x, i, a) => x == i + 1));
		}

		[Test]
		public void SortWithDefaultCompareWorks() {
			var arr = new[] { 1, 6, 6, 4, 2 };
			arr.Sort();
			Assert.AreEqual(arr, new[] { 1, 2, 4, 6, 6 });
		}

		[Test]
		public void SortWithCompareCallbackWorks() {
			var arr = new[] { 1, 6, 6, 4, 2 };
			arr.Sort((x, y) => y - x);
			Assert.AreEqual(arr, new[] { 6, 6, 4, 2, 1 });
		}

		[Test]
		public void ToArrayWorks() {
			var other = new JsDictionary();
			other["length"] = 2;
			other["0"] = "a";
			other["1"] = "b";
			var actual = Array.ToArray(other);
			Assert.IsTrue(actual is Array);
			Assert.AreEqual(actual, new[] { "a", "b" });
		}

		[Test]
		public void ForeachWhenCastToIListWorks() {
			IList<string> list = new[] { "x", "y" };
			string result = "";
			foreach (var s in list) {
				result += s;
			}
			Assert.AreEqual(result, "xy");
		}

		[Test]
		public void ICollectionCountWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			Assert.AreEqual(l.Count, 3);
		}

		[Test]
		public void ICollectionAddWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			l.Add("a");
			Assert.AreEqual(l, new[] { "x", "y", "z", "a" });
		}

		[Test]
		public void ICollectionClearWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			l.Clear();
			Assert.AreEqual(l, new string[0]);
		}

		[Test]
		public void ICollectionContainsWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			Assert.IsTrue(l.Contains("y"));
			Assert.IsFalse(l.Contains("a"));
		}

		[Test]
		public void ICollectionRemoveWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			Assert.IsTrue(l.Remove("y"));
			Assert.IsFalse(l.Remove("a"));
			Assert.AreEqual(l, new[] { "x", "z" });
		}

		[Test]
		public void IListIndexingWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			Assert.AreEqual(l[1], "y");
			l[1] = "a";
			Assert.AreEqual(l, new[] { "x", "a", "z" });
		}

		[Test]
		public void IListIndexOfWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			Assert.AreEqual(l.IndexOf("y"), 1);
			Assert.AreEqual(l.IndexOf("a"), -1);
		}

		[Test]
		public void IListInsertWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			l.Insert(1, "a");
			Assert.AreEqual(l, new[] { "x", "a", "y", "z" });
		}

		[Test]
		public void IListRemoveAtWorks() {
			IList<string> l = new[] { "x", "y", "z" };
			l.RemoveAt(1);
			Assert.AreEqual(l, new[] { "x", "z" });
		}
	}
}
