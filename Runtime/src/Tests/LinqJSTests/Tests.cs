using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QUnit;
using System.Text;
using System.Text.RegularExpressions;

namespace LinqJSTests {
	[TestFixture]
	public class Tests {
		private class MyEnumerator : IEnumerator<int> {
			private int _from, _count, _current;
			private MyEnumerable _enumerable;

			public MyEnumerator(MyEnumerable enumerable, int from, int count) {
				_enumerable = enumerable;
				_from  = from;
				_count = count;
			}

			public int Current { get; private set; }
			object IEnumerator.Current { get { return Current; } }

			public bool MoveNext() {
				if (_current == _enumerable.ThrowOnIndex)
					throw new Exception("error");

				if (_current < _count) {
					Current = _from + _current++;
					_enumerable.LastReturnedValue = Current;
					_enumerable.NumMoveNextCalls++;
					return true;
				}
				else {
					return false;
				}
			}

			public void Reset() {
				_current = 0;
			}

			public void Dispose() {
				_enumerable.EnumeratorDisposed = true;
			}
		}

		private class MyEnumerable : IEnumerable<int> {
			private int _from, _count;
			public bool EnumeratorDisposed { get; set; }
			public int LastReturnedValue { get; set; }
			public int NumMoveNextCalls { get; set; }
			public int ThrowOnIndex { get; set; }

			public MyEnumerable(int from, int count) {
				_from  = from;
				_count = count;
				ThrowOnIndex = -1;
			}

			public IEnumerator<int> GetEnumerator() {
				return new MyEnumerator(this, _from, _count);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return null;
			}
		}

#region Misc tests
		[Test(Category = "Misc")]
		public void QueryExpressionsWork() {
			string[] data = new[] { "4", "5", "7" };
			var result = (from a in data let b = int.Parse(a) let c = b + 1 select a + b.ToString() + c.ToString()).ToArray();

			Assert.AreEqual(result, new[] { "445", "556", "778" });
		}

		[Test(Category = "Misc")]
		public void EnumeratorIsIDisposable() {
			var enm = Enumerable.Range(1, 5).GetEnumerator();
			Assert.IsTrue(enm is IDisposable);
		}

		[Test(Category = "Misc")]
		public void EnumerableIsIEnumerable() {
			var enm = Enumerable.Range(1, 5);
			Assert.IsTrue(enm is IEnumerable<int>);
		}

		[Test(Category = "Misc")]
		public void CanSelectFromGrouping() {
			var grp = new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i.ToString()).First();
			Assert.AreEqual(grp.Select(x => x).ToArray(), new[] { "1", "3", "5" });
		}

		[Test(Category = "Misc")]
		public void GroupingIsIEnumerable() {
			var grp = new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i.ToString()).First();
			Assert.IsTrue(grp is IEnumerable<string>);
		}

		[Test(Category = "Misc")]
		public void CanForeachOverGrouping() {
			var grp = new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i.ToString()).First();
			var result = "";
			foreach (var x in grp)
				result += x;
			Assert.AreEqual(result, "135");
		}

		[Test(Category = "Misc")]
		public void CanReadGroupingKey() {
			var grp = new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i.ToString()).First();
			Assert.AreEqual(grp.Key, 1);
		}

		[Test(Category = "Misc")]
		public void CanSelectFromLookup() {
			var actual = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]).Select(x => x.Key + " : " + x.ToArray());
			Assert.AreEqual(actual.ToArray(), new[] { "xls : temp1", "pdf : temp2,temp4", "jpg : temp3" });
		}

		[Test(Category = "Misc")]
		public void CanForeachOverLookup() {
			var actual = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			string result = "";
			foreach (var x in actual)
				result += x.Key + " : " + x.ToArray() + "\n";
			Assert.AreEqual(result, "xls : temp1\npdf : temp2,temp4\njpg : temp3\n");
		}

		[Test(Category = "Misc")]
		public void LookupImplementsIEnumerableOfGrouping() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.IsTrue(lu is IEnumerable<Grouping<string, string>>);
		}

		[Test(Category = "Misc")]
		public void LookupCountMemberWorks() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.AreEqual(lu.Count, 3);
		}

		[Test(Category = "Misc")]
		public void LookupContainsMemberWorks() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.IsTrue(lu.Contains("xls"));
			Assert.IsFalse(lu.Contains("XLS"));
		}

		[Test(Category = "Misc")]
		public void LookupIndexingWorks() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "temp2", "temp4" });
		}


		[Test(Category = "Misc")]
		public void CanSelectFromDictionary() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value).Select(x => new { x.Key, x.Value });
			Assert.AreEqual(actual.ToArray(), new[] { new { Key = "id_0", Value = 1 },  new { Key = "id_1", Value = 2 },  new { Key = "id_2", Value = 3 },  new { Key = "id_3", Value = 4 },  new { Key = "id_4", Value = 5 } });
		}

		[Test(Category = "Misc")]
		public void CanForeachOverDictionary() {
			string result = "";
			foreach (var kvp in Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value)) {
				result += kvp.Key + "=" + kvp.Value + " ";
			}
			Assert.AreEqual(result, "id_0=1 id_1=2 id_2=3 id_3=4 id_4=5 ");
		}

		[Test(Category = "Misc")]
		public void DictionaryImplementsIEnumerableOfKeyValuePair() {
			var d = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.IsTrue(d is IEnumerable<KeyValuePair<string, int>>);
		}

		[Test(Category = "Misc")]
		public void CanGetDictionaryElement() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.AreEqual(actual["id_0"], 1);
			Assert.Throws(() => { int i = actual["x"]; });
		}

		[Test(Category = "Misc")]
		public void CanSetDictionaryElementWithIndexer() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			actual["id_0"] = 98;
			actual["x"] = 99;
			Assert.AreEqual(actual["id_0"], 98);
			Assert.AreEqual(actual["x"], 99);
		}

		[Test(Category = "Misc")]
		public void CanAddDictionaryElement() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			actual.Add("x", 99);
			Assert.AreEqual(actual["x"], 99);
			Assert.Throws(() => actual.Add("id_0", 1));
		}

		[Test(Category = "Misc")]
		public void CanGetDictionaryCount() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.AreEqual(actual.Count, 5);
		}

		[Test(Category = "Misc")]
		public void DictionaryContainsKeyWorks() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.IsTrue(actual.ContainsKey("id_0"));
			Assert.IsFalse(actual.ContainsKey("x"));
		}

		[Test(Category = "Misc")]
		public void DictionaryKeysWorks() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value).Keys;
			Assert.AreEqual(actual.OrderBy().ToArray(), new[] { "id_0", "id_1", "id_2", "id_3", "id_4" });
		}

		[Test(Category = "Misc")]
		public void DictionaryValuesWorks() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value).Values;
			Assert.AreEqual(actual.OrderBy().ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Misc")]
		public void DictionaryRemoveWorks() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.IsTrue(actual.Remove("id_0"), "Remove id_0 should return true");
			Assert.IsFalse(actual.Remove("x"), "Remove x should return false");
			Assert.AreEqual(actual.Values.OrderBy().ToArray(), new[] { 2, 3, 4, 5 });
		}

		[Test(Category = "Misc")]
		public void DictionaryTryGetValueWorks() {
			var actual = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			int i;
			Assert.IsTrue(actual.TryGetValue("id_0", out i), "TryGetValue id_0 should return true");
			Assert.AreEqual(i, 1, "id_0 should be 1");
			Assert.IsFalse(actual.TryGetValue("x", out i), "TryGetValue x should return false");
			Assert.AreEqual(i, 0, "x should be 0");
		}

#endregion

#region Generator methods
		[Test(Category = "Generators")]
		public void ChoiceWorks() {
			var enm = Enumerable.Choice("a", "b", "c", "d");
			int count = 0;
			foreach (var x in enm) {
				Assert.IsTrue(x == "a" || x == "b" || x == "c" || x == "d", "Value should be one of the choices");
				if (count++ > 10)
					break;
			}
		}

		[Test(Category = "Generators")]
		public void CycleWorks() {
			var enm = Enumerable.Cycle("a", "b", "c");
			var result = new List<string>();
			foreach (var x in enm) {
				result.Add(x);
				if (result.Count >= 10)
					break;
			}
			Assert.AreEqual(result, new[] { "a", "b", "c", "a", "b", "c", "a", "b", "c", "a" });
		}

		[Test(Category = "Generators", ExpectedAssertionCount = 0)]
		public void EmptyWorks() {
			foreach (var x in Enumerable.Empty<int>()) {
				Assert.Fail("Enumerator should be empty");
			}
		}

		[Test(Category = "Generators")]
		public void FromWorksForArray() {
			Assert.AreEqual(Enumerable.From(new[] { 1, 2, 3, 4, 5 }).ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Generators")]
		public void FromWorksForSaltarelleEnumerable() {
			Assert.AreEqual(Enumerable.From(new MyEnumerable(1, 5)).ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Generators")]
		public void FromWorksForArrayLikeObject() {
			var d = new JsDictionary<string, object>();
			d["length"] = 5;
			d["0"] = "A";
			d["1"] = "B";
			d["2"] = "C";
			d["3"] = "D";
			d["4"] = "E";
			Assert.AreEqual(Enumerable.From(d).ToArray(), new[] { "A", "B", "C", "D", "E" });
		}

		[Test(Category = "Generators")]
		public void FromWorksForString() {
			var result = new List<string>();
			foreach (var s in Enumerable.From("12345"))
				result.Add(s);
			Assert.AreEqual(result, new[] { "1", "2", "3", "4", "5" });
		}

		[Test(Category = "Generators")]
		public void MakeWorks() {
			Assert.AreEqual(Enumerable.Make(123).ToArray(), new[] { 123 });
		}

		[Test(Category = "Generators")]
		public void MatchesWithRegexArgWorks() {
			Assert.AreEqual(Enumerable.Matches("xbcyBCzbc", new Regex("(.)bc", "i")).Select(m => new { index = m.Index, all = m[0], capture = m[1] }).ToArray(), new[] { new { index = 0, all = "xbc", capture = "x" }, new { index = 3, all = "yBC", capture = "y" }, new { index = 6, all = "zbc", capture = "z" } });
		}

		[Test(Category = "Generators")]
		public void MatchesWithStringArgWorks() {
			Assert.AreEqual(Enumerable.Matches("xbcyBCzbc", "(.)bc").Select(m => new { index = m.Index, all = m[0], capture = m[1] }).ToArray(), new[] { new { index = 0, all = "xbc", capture = "x" }, new { index = 6, all = "zbc", capture = "z" } });
		}

		[Test(Category = "Generators")]
		public void MatchesWithStringAndFlagsArgWorks() {
			Assert.AreEqual(Enumerable.Matches("xbcyBCzbc", "(.)bc", "i").Select(m => new { index = m.Index, all = m[0], capture = m[1] }).ToArray(), new[] { new { index = 0, all = "xbc", capture = "x" }, new { index = 3, all = "yBC", capture = "y" }, new { index = 6, all = "zbc", capture = "z" } });
		}

		[Test(Category = "Generators")]
		public void RangeWorks() {
			Assert.AreEqual(Enumerable.Range(4, 3).ToArray(), new[] { 4, 5, 6 });
		}

		[Test(Category = "Generators")]
		public void RangeWithStepWorks() {
			Assert.AreEqual(Enumerable.Range(4, 3, 2).ToArray(), new[] { 4, 6, 8 });
		}

		[Test(Category = "Generators")]
		public void RangeDownWorks() {
			Assert.AreEqual(Enumerable.RangeDown(10, 5).ToArray(), new[] { 10, 9, 8, 7, 6 });
		}

		[Test(Category = "Generators")]
		public void RangeDownWithStepWorks() {
			Assert.AreEqual(Enumerable.RangeDown(10, 5, 3).ToArray(), new[] { 10, 7, 4, 1, -2 });
		}

		[Test(Category = "Generators")]
		public void RangeToWorks() {
			Assert.AreEqual(Enumerable.RangeTo(10, 13).ToArray(), new[] { 10, 11, 12, 13 });
		}

		[Test(Category = "Generators")]
		public void RangeToWithStepWorks() {
			Assert.AreEqual(Enumerable.RangeTo(1, 9, 3).ToArray(), new[] { 1, 4, 7 });
		}

		[Test(Category = "Generators")]
		public void RepeatWorks() {
			var result = new List<string>();
			foreach (var enm in Enumerable.Repeat("x")) {
				result.Add(enm);
				if (result.Count == 3)
					break;
			}
			Assert.AreEqual(result, new[] { "x", "x", "x" });
		}

		[Test(Category = "Generators")]
		public void RepeatWithCountWorks() {
			Assert.AreEqual(Enumerable.Repeat("foo", 3).ToArray(), new[] { "foo", "foo", "foo" });
		}

		[Test(Category = "Generators")]
		public void RepeatWithFinalizeWorks() {
			bool finalized = false;
			var enm = Enumerable.RepeatWithFinalize(() => "foo", s => { Assert.AreEqual(s, "foo", "The correct arg should be passed to finalizer"); finalized = true; });
			var result = new List<string>();
			foreach (var s in enm) {
				result.Add(s);
				if (result.Count == 3)
					break;
			}
			Assert.AreEqual(result, new[] { "foo", "foo", "foo" }, "Result should be correct");
			Assert.IsTrue(finalized, "Finalizer should have been called");
		}

		[Test(Category = "Generators")]
		public void GenerateWorks() {
			int i = 1;
			var result = new List<int>();
			foreach (var enm in Enumerable.Generate(() => i *= 2)) {
				result.Add(enm);
				if (result.Count == 3)
					break;
			}
			Assert.AreEqual(result, new[] { 2, 4, 8 });
		}

		[Test(Category = "Generators")]
		public void GenerateWithCountWorks() {
			int i = 1;
			Assert.AreEqual(Enumerable.Generate(() => i *= 2, 4).ToArray(), new[] { 2, 4, 8, 16 });
		}

		[Test(Category = "Generators")]
		public void ToInfinityWorks() {
			Assert.AreEqual(Enumerable.ToInfinity().Take(5).ToArray(), new[] { 0, 1, 2, 3, 4 });
		}

		[Test(Category = "Generators")]
		public void ToInfinityWithStartWorks() {
			Assert.AreEqual(Enumerable.ToInfinity(10).Take(5).ToArray(), new[] { 10, 11, 12, 13, 14 });
		}

		[Test(Category = "Generators")]
		public void ToInfinityWithStartAndStepWorks() {
			Assert.AreEqual(Enumerable.ToInfinity(10, 2).Take(5).ToArray(), new[] { 10, 12, 14, 16, 18 });
		}

		[Test(Category = "Generators")]
		public void ToNegativeInfinityWorks() {
			Assert.AreEqual(Enumerable.ToNegativeInfinity().Take(5).ToArray(), new[] { 0, -1, -2, -3, -4 });
		}

		[Test(Category = "Generators")]
		public void ToNegativeInfinityWithStartWorks() {
			Assert.AreEqual(Enumerable.ToNegativeInfinity(10).Take(5).ToArray(), new[] { 10, 9, 8, 7, 6 });
		}

		[Test(Category = "Generators")]
		public void ToNegativeInfinityWithStartAndStepWorks() {
			Assert.AreEqual(Enumerable.ToNegativeInfinity(10, 2).Take(5).ToArray(), new[] { 10, 8, 6, 4, 2 });
		}
#endregion

#region Projection / filtering
		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseBreadthFirst(x => new[] { x + x }).Take(5).ToArray(), new[] { 1, 2, 4, 8, 16 }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWithResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseBreadthFirst(x => new[] { x + x }, x => x * x).Take(5).ToArray(), new[] { 1, 4, 16, 64, 256 }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWithResultSelectorWithIndexArgWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseBreadthFirst(x => new[] { x + x }, (x, level) => new { x, level }).Take(5).ToArray(), new[] { new { x = 1, level = 0 }, new { x = 2, level = 1 }, new { x = 4, level = 2 }, new { x = 8, level = 3 }, new { x = 16, level = 4 } }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseBreadthFirst(x => new[] { x + x }).Take(5).ToArray(), new[] { 1, 2, 4, 8, 16 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWithResultSelectorWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseBreadthFirst(x => new[] { x + x }, x => x * x).Take(5).ToArray(), new[] { 1, 4, 16, 64, 256 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseBreadthFirstWithResultSelectorWithIndexArgWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseBreadthFirst(x => new[] { x + x }, (x, level) => new { x, level }).Take(5).ToArray(), new[] { new { x = 1, level = 0 }, new { x = 2, level = 1 }, new { x = 4, level = 2 }, new { x = 8, level = 3 }, new { x = 16, level = 4 } }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}


		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseDepthFirst(x => new[] { x + x }).Take(5).ToArray(), new[] { 1, 2, 4, 8, 16 }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWithResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseDepthFirst(x => new[] { x + x }, x => x * x).Take(5).ToArray(), new[] { 1, 4, 16, 64, 256 }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWithResultSelectorWithIndexArgWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Make(1).TraverseDepthFirst(x => new[] { x + x }, (x, level) => new { x, level }).Take(5).ToArray(), new[] { new { x = 1, level = 0 }, new { x = 2, level = 1 }, new { x = 4, level = 2 }, new { x = 8, level = 3 }, new { x = 16, level = 4 } }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseDepthFirst(x => new[] { x + x }).Take(5).ToArray(), new[] { 1, 2, 4, 8, 16 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWithResultSelectorWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseDepthFirst(x => new[] { x + x }, x => x * x).Take(5).ToArray(), new[] { 1, 4, 16, 64, 256 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void TraverseDepthFirstWithResultSelectorWithIndexArgWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1);
			Assert.AreEqual(enumerable.TraverseDepthFirst(x => new[] { x + x }, (x, level) => new { x, level }).Take(5).ToArray(), new[] { new { x = 1, level = 0 }, new { x = 2, level = 1 }, new { x = 4, level = 2 }, new { x = 8, level = 3 }, new { x = 16, level = 4 } }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}
		
		
		[Test(Category = "Projection / filtering")]
		public void FlattenWorksForSaltarelleEnumerable() {
			var arr = new object[] { 1, new object[] { 234, 2, new object[] { 62, 3 } }, new object[] { 234, 5 }, 3 };
			Assert.AreEqual(arr.Flatten().ToArray(), new[] { 1, 234, 2, 62, 3, 234, 5, 3 });
		}

		[Test(Category = "Projection / filtering")]
		public void FlattenWorksForLinqJSEnumerable() {
			var arr = new object[] { 1, new object[] { 234, 2, new object[] { 62, 3 } }, new object[] { 234, 5 }, 3 };
			Assert.AreEqual(arr.Select(x => x).Flatten().ToArray(), new[] { 1, 234, 2, 62, 3, 234, 5, 3 });
		}

		
		[Test(Category = "Projection / filtering")]
		public void PairwiseWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Pairwise((p, n) => p + n).ToArray(), new[] { 3, 5, 7, 9 });
		}

		[Test(Category = "Projection / filtering")]
		public void PairwiseWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(x => x).Pairwise((p, n) => p + n).ToArray(), new[] { 3, 5, 7, 9 });
		}


		[Test(Category = "Projection / filtering")]
		public void ScanWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Scan((a, b) => a + b).ToArray(), new[] { 1, 3, 6, 10, 15 });
		}

		[Test(Category = "Projection / filtering")]
		public void ScanWithSeedWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Scan("", (a, b) => a + " " + b).ToArray(), new[] { "", " 1", " 1 2", " 1 2 3", " 1 2 3 4", " 1 2 3 4 5" });
		}

		[Test(Category = "Projection / filtering")]
		public void ScanWithSeedAndResultSelectorWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Scan("", (a, b) => a + " " + b, s => s.Substr(1)).ToArray(), new[] { "", "1", "1 2", "1 2 3", "1 2 3 4", "1 2 3 4 5" });
		}

		[Test(Category = "Projection / filtering")]
		public void ScanWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(x => x).Scan((a, b) => a + b).ToArray(), new[] { 1, 3, 6, 10, 15 });
		}

		[Test(Category = "Projection / filtering")]
		public void ScanWithSeedWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(x => x).Scan("", (a, b) => a + " " + b).ToArray(), new[] { "", " 1", " 1 2", " 1 2 3", " 1 2 3 4", " 1 2 3 4 5" });
		}

		[Test(Category = "Projection / filtering")]
		public void ScanWithSeedAndResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(x => x).Scan("", (a, b) => a + " " + b, s => s.Substr(1)).ToArray(), new[] { "", "1", "1 2", "1 2 3", "1 2 3 4", "1 2 3 4 5" });
		}


		[Test(Category = "Projection / filtering")]
		public void SelectFromArrayWorks() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select(i => i * i).ToArray(), new[] { 1, 4, 9 });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectWithIndexFromArrayWorks() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select((i, n) => i * i + n).ToArray(), new[] { 1, 5, 11 });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectFromSaltarelleEnumerableWorks() {
			var enumerable = new MyEnumerable(1, 3);
			Assert.AreEqual(enumerable.Select(i => i * i).ToArray(), new[] { 1, 4, 9 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void SelectWithIndexFromSaltarelleEnumerableWorks() {
			var enumerable = new MyEnumerable(1, 3);
			Assert.AreEqual(enumerable.Select((i, n) => i * i + n).ToArray(), new[] { 1, 5, 11 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void ChainingSelectWorks() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select(i => i * i).Select(i => i * 2).ToArray(), new[] { 2, 8, 18 }, "Result should be correct");
		}

		[Test(Category = "Projection / filtering")]
		public void ChainingSelectWithIndexWorks() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select(i => i * i).Select((i, idx) => i * 2 + idx).ToArray(), new[] { 2, 9, 20 }, "Result should be correct");
		}


		[Test(Category = "Projection / filtering")]
		public void SelectManyWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.SelectMany(i => Enumerable.Repeat('a' + i, i)).ToArray(), new[] { 'b', 'c', 'c', 'd', 'd', 'd' });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.SelectMany((i, idx) => Enumerable.Repeat('a' + idx, i)).ToArray(), new[] { 'a', 'b', 'b', 'c', 'c', 'c' });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithResultSelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.SelectMany(i => Enumerable.Repeat('a' + i, i), (i, c) => new string((char)c, 1) + "x" + i).ToArray(), new[] { "bx1", "cx2", "cx2", "dx3", "dx3", "dx3" });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithIndexAndResultSelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.SelectMany((i, idx) => Enumerable.Repeat('a' + idx, i), (i, c) => new string((char)c, 1) + "x" + i).ToArray(), new[] { "ax1", "bx2", "bx2", "cx3", "cx3", "cx3" });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).SelectMany(i => Enumerable.Repeat('a' + i, i)).ToArray(), new[] { 'b', 'c', 'c', 'd', 'd', 'd' });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).SelectMany((i, idx) => Enumerable.Repeat('a' + idx, i)).ToArray(), new[] { 'a', 'b', 'b', 'c', 'c', 'c' });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).SelectMany(i => Enumerable.Repeat('a' + i, i), (i, c) => new string((char)c, 1) + "x" + i).ToArray(), new[] { "bx1", "cx2", "cx2", "dx3", "dx3", "dx3" });
		}

		[Test(Category = "Projection / filtering")]
		public void SelectManyWithIndexAndResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).SelectMany((i, idx) => Enumerable.Repeat('a' + idx, i), (i, c) => new string((char)c, 1) + "x" + i).ToArray(), new[] { "ax1", "bx2", "bx2", "cx3", "cx3", "cx3" });
		}


		[Test(Category = "Projection / filtering")]
		public void CastWorksForArray() {
			Assert.AreEqual(new object[] { "X", null, "Y" }.Cast<string>().ToArray(), new[] { "X", null, "Y" });
			Assert.Throws(() => new object[] { "X", 1, "Y" }.Cast<string>().ToArray(), "Invalid cast should throw");
		}

		[Test(Category = "Projection / filtering")]
		public void CastWorksForLinqJSEnumerable() {
			Assert.AreEqual(new object[] { "X", null, "Y" }.Select(x => x).Cast<string>().ToArray(), new[] { "X", null, "Y" });
			Assert.Throws(() => new object[] { "X", 1, "Y" }.Select(x => x).Cast<string>().ToArray(), "Invalid cast should throw");
		}


		[Test(Category = "Projection / filtering")]
		public void OfTypeWorksForArray() {
			Assert.AreEqual(new object[] { "X", null, 1, "Y" }.OfType<string>().ToArray(), new[] { "X", "Y" });
		}

		[Test(Category = "Projection / filtering")]
		public void OfTypeWorksForLinqJSEnumerable() {
			Assert.AreEqual(new object[] { "X", null, 1, "Y" }.Select(x => x).OfType<string>().ToArray(), new[] { "X", "Y" });
		}


		[Test(Category = "Projection / filtering")]
		public void ZipWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Zip(new[] { "a", "b", "c" }, (a, b) => a + b).ToArray(), new[] { "1a", "2b", "3c" });
		}

		[Test(Category = "Projection / filtering")]
		public void ZipWithIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Zip(new[] { "a", "b", "c" }, (a, b, i) => a + b + i).ToArray(), new[] { "1a0", "2b1", "3c2" });
		}

		[Test(Category = "Projection / filtering")]
		public void ZipWorksForJSLinqEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Zip(new[] { "a", "b", "c" }, (a, b) => a + b).ToArray(), new[] { "1a", "2b", "3c" });
		}

		[Test(Category = "Projection / filtering")]
		public void ZipWithIndexWorksForJSLinqEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Zip(new[] { "a", "b", "c" }, (a, b, i) => a + b + i).ToArray(), new[] { "1a0", "2b1", "3c2" });
		}


		[Test(Category = "Projection / filtering")]
		public void WhereWithoutIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(i => i).Where(i => i > 3).ToArray(), new[] { 4, 5 });
		}

		[Test(Category = "Projection / filtering")]
		public void WhereWithoutIndexWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 5);
			Assert.AreEqual(enumerable.Where(i => i > 3).ToArray(), new[] { 4, 5 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Projection / filtering")]
		public void WhereWithIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Select(i => i).Where((i, idx) => idx > 2).ToArray(), new[] { 4, 5 });
		}

		[Test(Category = "Projection / filtering")]
		public void WhereWithIndexWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 5);
			Assert.AreEqual(enumerable.Where((i, idx) => idx > 2).ToArray(), new[] { 4, 5 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}
#endregion

#region Join methods

		[Test(Category = "Join")]
		public void JoinWorksForArray() {
			Assert.AreEqual(new[] { Tuple.Create(1, "Outer 1"), Tuple.Create(2, "Outer 2"), Tuple.Create(3, "Outer 3"), Tuple.Create(4, "Outer 4"), Tuple.Create(5, "Outer 5") }.Join(Enumerable.Range(3, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1, x => x.Item2, (t1, t2) => t1.Item2 + ":" + t2.Item1).ToArray(), new[] { "Outer 3:Inner 3", "Outer 4:Inner 4", "Outer 5:Inner 5" });
		}

		[Test(Category = "Join")]
		public void JoinWithCompareSelectorWorksForArray() {
			Assert.AreEqual(new[] { Tuple.Create(1, "Outer 1"), Tuple.Create(2, "Outer 2"), Tuple.Create(3, "Outer 3"), Tuple.Create(4, "Outer 4"), Tuple.Create(5, "Outer 5") }.Join(Enumerable.Range(3, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1, x => x.Item2, (t1, t2) => t1.Item2 + ":" + t2.Item1, a => a % 3).ToArray(), new[] { "Outer 1:Inner 4", "Outer 1:Inner 7", "Outer 2:Inner 5", "Outer 3:Inner 3", "Outer 3:Inner 6", "Outer 4:Inner 4", "Outer 4:Inner 7", "Outer 5:Inner 5" });
		}

		[Test(Category = "Join")]
		public void JoinWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => Tuple.Create(i, "Outer " + i)).Join(Enumerable.Range(3, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1, x => x.Item2, (t1, t2) => t1.Item2 + ":" + t2.Item1).ToArray(), new[] { "Outer 3:Inner 3", "Outer 4:Inner 4", "Outer 5:Inner 5" });
		}

		[Test(Category = "Join")]
		public void JoinWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => Tuple.Create(i, "Outer " + i)).Join(Enumerable.Range(3, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1, x => x.Item2, (t1, t2) => t1.Item2 + ":" + t2.Item1, a => a % 3).ToArray(), new[] { "Outer 1:Inner 4", "Outer 1:Inner 7", "Outer 2:Inner 5", "Outer 3:Inner 3", "Outer 3:Inner 6", "Outer 4:Inner 4", "Outer 4:Inner 7", "Outer 5:Inner 5" });
		}


		[Test(Category = "Join")]
		public void GroupJoinWorksForArray() {
			Assert.AreEqual(new[] { Tuple.Create(1, "Outer 1"), Tuple.Create(2, "Outer 2"), Tuple.Create(3, "Outer 3") }.GroupJoin(Enumerable.Range(1, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1 % 2, x => x.Item2 % 3, (t1, t2) => t1.Item2 + ":" + t2.Select(x => x.Item1).ToArray()).ToArray(), new[] { "Outer 1:Inner 1,Inner 4", "Outer 2:Inner 3", "Outer 3:Inner 1,Inner 4" });
		}

		[Test(Category = "Join")]
		public void GroupJoinWithCompareSelectorWorksForArray() {
			Assert.AreEqual(new[] { Tuple.Create(1, "Outer 1"), Tuple.Create(2, "Outer 2"), Tuple.Create(3, "Outer 3") }.GroupJoin(Enumerable.Range(1, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1 % 2, x => x.Item2 % 3, (t1, t2) => t1.Item2 + ":" + t2.Select(x => x.Item1).ToArray(), a => a % 2).ToArray(), new[] { "Outer 1:Inner 1,Inner 4", "Outer 2:Inner 2,Inner 3,Inner 5", "Outer 3:Inner 1,Inner 4" });
		}


		[Test(Category = "Join")]
		public void GroupJoinWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Select(i => Tuple.Create(i, "Outer " + i)).GroupJoin(Enumerable.Range(1, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1 % 2, x => x.Item2 % 3, (t1, t2) => t1.Item2 + ":" + t2.Select(x => x.Item1).ToArray()).ToArray(), new[] { "Outer 1:Inner 1,Inner 4", "Outer 2:Inner 3", "Outer 3:Inner 1,Inner 4" });
		}

		[Test(Category = "Join")]
		public void GroupJoinWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Select(i => Tuple.Create(i, "Outer " + i)).GroupJoin(Enumerable.Range(1, 5).Select(i => Tuple.Create("Inner " + i, i)), x => x.Item1 % 2, x => x.Item2 % 3, (t1, t2) => t1.Item2 + ":" + t2.Select(x => x.Item1).ToArray(), a => a % 2).ToArray(), new[] { "Outer 1:Inner 1,Inner 4", "Outer 2:Inner 2,Inner 3,Inner 5", "Outer 3:Inner 1,Inner 4" });
		}

#endregion

#region Set methods

		[Test(Category = "Set methods")]
		public void AllWorksForArray() {
			Assert.IsTrue(new[] { 1, 2, 3 }.All(x => x > 0));
			Assert.IsFalse(new[] { 0, 1, 2 }.All(x => x > 0));
		}

		[Test(Category = "Set methods")]
		public void AllWorksForLinqJSEnumerable() {
			Assert.IsTrue(Enumerable.Repeat(1, 3).All(x => x > 0));
			Assert.IsFalse(Enumerable.Repeat(0, 3).All(x => x > 0));
		}


		[Test(Category = "Set methods")]
		public void AnyWorksForArray() {
			Assert.IsFalse(new int[0].Any());
			Assert.IsTrue(new[] { 1 }.Any());
		}

		[Test(Category = "Set methods")]
		public void AnyWorksForLinqJSEnumerable() {
			Assert.IsTrue(Enumerable.Repeat(0, 3).Any());
			Assert.IsFalse(Enumerable.Empty<int>().Any());
		}

		[Test(Category = "Set methods")]
		public void AnyWithPredicateWorksForArray() {
			Assert.IsTrue(new[] { 0, 1, 2 }.Any(x => x == 0));
			Assert.IsFalse(new[] { 1, 2, 3 }.Any(x => x == 0));
		}

		[Test(Category = "Set methods")]
		public void AnyWithPredicateWorksForLinqJSEnumerable() {
			Assert.IsTrue(Enumerable.Repeat(0, 3).Any(x => x == 0));
			Assert.IsFalse(Enumerable.Repeat(1, 3).Any(x => x == 0));
		}


		[Test(Category = "Set methods")]
		public void ConcatWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 3).Concat(new[] { 4, 5, 6 }).ToArray(), new[] { 1, 2, 3, 4, 5, 6 });
		}

		[Test(Category = "Set methods")]
		public void ConcatWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Concat(new[] { 4, 5, 6 }).ToArray(), new[] { 1, 2, 3, 4, 5, 6 });
		}


		[Test(Category = "Set methods")]
		public void InsertWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 3).Insert(1, new[] { 4, 5, 6 }).ToArray(), new[] { 1, 4, 5, 6, 2, 3 });
		}

		[Test(Category = "Set methods")]
		public void InsertWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).Insert(1, new[] { 4, 5, 6 }).ToArray(), new[] { 1, 4, 5, 6, 2, 3 });
		}


		[Test(Category = "Set methods")]
		public void AlternateWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 5).Alternate(-1).ToArray(), new[] { 1, -1, 2, -1, 3, -1, 4, -1, 5 });
		}

		[Test(Category = "Set methods")]
		public void AlternateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Alternate(-1).ToArray(), new[] { 1, -1, 2, -1, 3, -1, 4, -1, 5 });
		}


		[Test(Category = "Set methods")]
		public void ContainsWorksForSaltarelleEnumerable() {
			Assert.IsFalse(new MyEnumerable(1, 3).Contains(0));
			Assert.IsTrue(new MyEnumerable(1, 3).Contains(1));
		}

		[Test(Category = "Set methods")]
		public void ContainsWithCompareSelectorWorksForSaltarelleEnumerable() {
			Assert.IsFalse(new MyEnumerable(10, 3).Contains(4, i => i % 5));
			Assert.IsTrue(new MyEnumerable(10, 3).Contains(2, i => i % 5));
		}

		[Test(Category = "Set methods")]
		public void ContainsWorksForLinqJSEnumerable() {
			Assert.IsFalse(Enumerable.Range(1, 3).Contains(0));
			Assert.IsTrue(Enumerable.Range(1, 3).Contains(1));
		}

		[Test(Category = "Set methods")]
		public void ContainsWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.IsFalse(Enumerable.Range(10, 3).Contains(4, i => i % 5));
			Assert.IsTrue(Enumerable.Range(10, 3).Contains(2, i => i % 5));
		}


		[Test(Category = "Set methods")]
		public void DefaultIfEmptyWithoutArgumentWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 3).DefaultIfEmpty().ToArray(), new[] { 1, 2, 3 });
			Assert.AreEqual(new MyEnumerable(1, 0).DefaultIfEmpty().ToArray(), new[] { 0 });
		}

		[Test(Category = "Set methods")]
		public void DefaultIfEmptyWithArgumentWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 3).DefaultIfEmpty(8).ToArray(), new[] { 1, 2, 3 });
			Assert.AreEqual(new MyEnumerable(1, 0).DefaultIfEmpty(8).ToArray(), new[] { 8 });
		}

		[Test(Category = "Set methods")]
		public void DefaultIfEmptyWithoutArgumentWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).DefaultIfEmpty().ToArray(), new[] { 1, 2, 3 });
			Assert.AreEqual(Enumerable.Range(1, 0).DefaultIfEmpty().ToArray(), new[] { 0 });
		}

		[Test(Category = "Set methods")]
		public void DefaultIfEmptyWithArgumentWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 3).DefaultIfEmpty(8).ToArray(), new[] { 1, 2, 3 });
			Assert.AreEqual(Enumerable.Range(1, 0).DefaultIfEmpty(8).ToArray(), new[] { 8 });
		}


		[Test(Category = "Set methods")]
		public void DistinctWorksForArray() {
			Assert.AreEqual(new[] { 1, 4, 1, 3, 7, 1, 4, 3 }.Distinct().ToArray(), new[] { 1, 4, 3, 7 });
		}

		[Test(Category = "Set methods")]
		public void DistinctWithCompareSelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Distinct(i => i % 3).ToArray(), new[] { 1, 2, 3 });
		}

		[Test(Category = "Set methods")]
		public void DistinctWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 4, 1, 3, 7, 1, 4, 3 }.Select(x => x).Distinct().ToArray(), new[] { 1, 4, 3, 7 });
		}

		[Test(Category = "Set methods")]
		public void DistinctWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 10).Distinct(i => i % 3).ToArray(), new[] { 1, 2, 3 });
		}


		[Test(Category = "Set methods")]
		public void ExceptWorksForArray() {
			Assert.AreEqual(Enumerable.Range(1, 5).Except(Enumerable.Range(3, 7)).ToArray(), new[] { 1, 2 });
		}

		[Test(Category = "Set methods")]
		public void ExceptWithCompareSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i = 1 }, new { i = 2}, new { i = 3 }, new { i = 4 }, new { i = 5 } }.Except(new[] { new { i = 3 }, new { i = 4 }, new { i = 5 }, new { i = 6 }, new { i = 7 } }, x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 } });
		}

		[Test(Category = "Set methods")]
		public void ExceptWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Except(Enumerable.Range(3, 7)).ToArray(), new[] { 1, 2 });
		}

		[Test(Category = "Set methods")]
		public void ExceptWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i }).Except(Enumerable.Range(3, 7).Select(i => new { i }), x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 } });
		}

		
		[Test(Category = "Set methods")]
		public void SequenceEqualWorksForArray() {
			Assert.IsTrue(new[] { 1, 2, 3, 4, 5 }.SequenceEqual(new[] { 1, 2, 3, 4, 5 }));
			Assert.IsFalse(new[] { 1, 2, 3, 4, 5 }.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }));
		}

		[Test(Category = "Set methods")]
		public void SequenceEqualWithCompareSelectorWorksForArray() {
			Assert.IsTrue(new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } }.SequenceEqual(new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } }, x => x.i));
			Assert.IsFalse(new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } }.SequenceEqual(new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 }, new { i = 6 } }, x => x.i));
		}

		[Test(Category = "Set methods")]
		public void SequenceEqualWorksForLinqJSEnumerable() {
			Assert.IsTrue(Enumerable.Range(1, 5).SequenceEqual(Enumerable.Range(1, 5)));
			Assert.IsFalse(Enumerable.Range(1, 5).SequenceEqual(Enumerable.Range(1, 6)));
		}

		[Test(Category = "Set methods")]
		public void SequenceEqualWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.IsTrue(Enumerable.Range(1, 5).Select(i => new { i }).SequenceEqual(Enumerable.Range(1, 5).Select(i => new { i }), x => x.i));
			Assert.IsFalse(Enumerable.Range(1, 5).Select(i => new { i }).SequenceEqual(Enumerable.Range(1, 6).Select(i => new { i }), x => x.i));
		}


		[Test(Category = "Set methods")]
		public void UnionWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4 }.Union(new[] { 2, 3, 4, 5 }).ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Set methods")]
		public void UnionWithCompareSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new {i = 4 } }.Union(new[] { new { i = 2 }, new { i = 3 }, new {i = 4 },  new { i = 5 } }, x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new {i = 4 }, new { i = 5 } });
		}

		[Test(Category = "Set methods")]
		public void UnionWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 4).Union(Enumerable.Range(2, 4)).ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Set methods")]
		public void UnionWithCompareSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 4).Select(i => new { i }).Union(Enumerable.Range(2, 4).Select(i => new { i }), x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } });
		}
#endregion

#region Ordering methods
		[Test(Category = "Ordering methods")]
		public void OrderByWithoutSelectorWorksForArray() {
			Assert.AreEqual(new[] { 5, 4, 2, 1, 3 }.OrderBy().ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i = 5 }, new { i = 4 }, new { i = 2 }, new { i = 1 }, new { i = 3 } }.OrderBy(x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByWithoutSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 5, 4, 2, 1, 3 }.Select(x => x).OrderBy().ToArray(), new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { new { i = 5 }, new { i = 4 }, new { i = 2 }, new { i = 1 }, new { i = 3 } }.Select(x => x).OrderBy(x => x.i).ToArray(), new[] { new { i = 1 }, new { i = 2 }, new { i = 3 }, new { i = 4 }, new { i = 5 } });
		}
		

		[Test(Category = "Ordering methods")]
		public void OrderByDescendingWithoutSelectorWorksForArray() {
			Assert.AreEqual(new[] { 5, 4, 2, 1, 3 }.OrderByDescending().ToArray(), new[] { 5, 4, 3, 2, 1 });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByDescendningWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i = 5 }, new { i = 4 }, new { i = 2 }, new { i = 1 }, new { i = 3 } }.OrderByDescending(x => x.i).ToArray(), new[] { new { i = 5 }, new { i = 4 }, new { i = 3 }, new { i = 2 }, new { i = 1 } });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByDescendingWithoutSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 5, 4, 2, 1, 3 }.Select(x => x).OrderByDescending().ToArray(), new[] { 5, 4, 3, 2, 1 });
		}

		[Test(Category = "Ordering methods")]
		public void OrderByDescendingWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { new { i = 5 }, new { i = 4 }, new { i = 2 }, new { i = 1 }, new { i = 3 } }.Select(x => x).OrderByDescending(x => x.i).ToArray(), new[] { new { i = 5 }, new { i = 4 }, new { i = 3 }, new { i = 2 }, new { i = 1 } });
		}


		[Test(Category = "Ordering methdos")]
		public void ThenByWorks() {
			var arr = new[] { new { a = 2, b = 4, c = 1 }, new { a = 2, b = 3, c = 7 }, new { a = 2, b = 3, c = 3 }, new { a = 4, b = 7, c = 5 }, new { a = 7, b = 3, c = 2 }, new { a = 4, b = 1, c = 5 } };
			var result = arr.OrderBy(x => x.a).ThenBy(x => x.c).ThenBy(x => x.b).ToArray();
			Assert.AreEqual(result, new[] { new { a = 2, b = 4, c = 1 }, new { a = 2, b = 3, c = 3 }, new { a = 2, b = 3, c = 7 }, new { a = 4, b = 1, c = 5 }, new { a = 4, b = 7, c = 5 }, new { a = 7, b = 3, c = 2 } });
		}


		[Test(Category = "Ordering methdos")]
		public void ThenByDescendingWorks() {
			var arr = new[] { new { a = 2, b = 4, c = 1 }, new { a = 2, b = 3, c = 7 }, new { a = 2, b = 3, c = 3 }, new { a = 4, b = 7, c = 5 }, new { a = 7, b = 3, c = 2 }, new { a = 4, b = 1, c = 5 } };
			var result = arr.OrderBy(x => x.a).ThenByDescending(x => x.c).ThenByDescending(x => x.b).ToArray();
			Assert.AreEqual(result, new[] { new { a = 2, b = 3, c = 7 }, new { a = 2, b = 3, c = 3 }, new { a = 2, b = 4, c = 1 }, new { a = 4, b = 7, c = 5 }, new { a = 4, b = 1, c = 5 }, new { a = 7, b = 3, c = 2 } });
		}


		[Test(Category = "Ordering methdos")]
		public void ReverseWorksForSaltarelleEnumerable() {
			Assert.AreEqual(new MyEnumerable(1, 5).Reverse().ToArray(), new[] { 5, 4, 3, 2, 1 });
		}

		[Test(Category = "Ordering methdos")]
		public void ReverseWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Reverse().ToArray(), new[] { 5, 4, 3, 2, 1 });
		}


		[Test(Category = "Ordering methdos")]
		public void ShuffleWorksForSaltarelleEnumerable() {
			var result = new MyEnumerable(1, 5).Shuffle().ToArray();
			Assert.IsTrue(result.Contains(1) && result.Contains(2) && result.Contains(3) && result.Contains(4) && result.Contains(5));
		}

		[Test(Category = "Ordering methdos")]
		public void ShuffleWorksForLinqJSEnumerable() {
			var result = Enumerable.Range(1, 5).Shuffle().ToArray();
			Assert.IsTrue(result.Contains(1) && result.Contains(2) && result.Contains(3) && result.Contains(4) && result.Contains(5));
		}
#endregion

#region Grouping methods
		[Test(Category = "Grouping methods")]
		public void GroupByWithOnlyKeySelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 1, 3, 5 } }, new { key = 0, value = new[] { 2, 4 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithOnlyKeySelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1,5).GroupBy(i => i % 2).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 1, 3, 5 } }, new { key = 0, value = new[] { 2, 4 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i * 10).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 0, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementSelectorsForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1,5).GroupBy(i => i % 2, i => i * 10).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 0, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementAndResultSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i % 2, i => i * 10, (key, value) => new { key, value = value.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 0, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementAndResultSelectorsWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1,5).GroupBy(i => i % 2, i => i * 10, (key, value) => new { key, value = value.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 0, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementAndResultAndCompareSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.GroupBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }, i => i % 2).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 2, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void GroupByWithKeyAndElementAndResultAndCompareSelectorsWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1,5).GroupBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }, i => i % 2).ToArray(), new[] { new { key = 1, value = new[] { 10, 30, 50 } }, new { key = 2, value = new[] { 20, 40 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithOnlyKeySelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.PartitionBy(i => i).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 1 } }, new { key = 2, value = new[] { 2, 2 } }, new { key = 3, value = new[] { 3, 3 } }, new { key = 2, value = new[] { 2 } }, new { key = 1, value = new[] { 1, 1 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithOnlyKeySelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.Select(x => x).PartitionBy(i => i).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 1 } }, new { key = 2, value = new[] { 2, 2 } }, new { key = 3, value = new[] { 3, 3 } }, new { key = 2, value = new[] { 2 } }, new { key = 1, value = new[] { 1, 1 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.PartitionBy(i => i, i => i * 10).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20 } }, new { key = 3, value = new[] { 30, 30 } }, new { key = 2, value = new[] { 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementSelectorsWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.Select(x => x).PartitionBy(i => i, i => i * 10).Select(g => new { key = g.Key, value = g.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20 } }, new { key = 3, value = new[] { 30, 30 } }, new { key = 2, value = new[] { 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementAndResultSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.PartitionBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20 } }, new { key = 3, value = new[] { 30, 30 } }, new { key = 2, value = new[] { 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementAndResultSelectorsWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.Select(x => x).PartitionBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20 } }, new { key = 3, value = new[] { 30, 30 } }, new { key = 2, value = new[] { 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementAndResultAndCompareSelectorsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.PartitionBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }, i => Math.Min(i, 2)).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20, 30, 30, 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}

		[Test(Category = "Grouping methods")]
		public void PartitionByWithKeyAndElementAndResultAndCompareSelectorsWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 2, 3, 3, 2, 1, 1 }.Select(x => x).PartitionBy(i => i, i => i * 10, (key, value) => new { key, value = value.ToArray() }, i => Math.Min(i, 2)).ToArray(), new[] { new { key = 1, value = new[] { 10 } }, new { key = 2, value = new[] { 20, 20, 30, 30, 20 } }, new { key = 1, value = new[] { 10, 10 } } });
		}


		[Test(Category = "Grouping methods")]
		public void BufferWithCountWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Buffer(4).ToArray(), new[] { new[] { 1, 2, 3, 4 }, new[] { 5, 6, 7, 8 }, new[] { 9, 10 } });
		}

		[Test(Category = "Grouping methods")]
		public void BufferWithCountWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 10).Buffer(4).ToArray(), new[] { new[] { 1, 2, 3, 4 }, new[] { 5, 6, 7, 8 }, new[] { 9, 10 } });
		}

#endregion

#region Aggregate methods
		[Test(Category = "Aggregate")]
		public void AggregateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5}.Aggregate((a, b) => a * b), 120);
		}

		[Test(Category = "Aggregate")]
		public void AggregateWithSeedWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5}.Aggregate("", (a, b) => a + b), "12345");
		}

		[Test(Category = "Aggregate")]
		public void AggregateWithSeedAndResultSelectorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5}.Aggregate("", (a, b) => a + b, s => s + "X"), "12345X");
		}

		[Test(Category = "Aggregate")]
		public void AggregateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Aggregate((a, b) => a * b), 120);
		}

		[Test(Category = "Aggregate")]
		public void AggregateWithSeedWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Aggregate("", (a, b) => a + b), "12345");
		}

		[Test(Category = "Aggregate")]
		public void AggregateWithSeedAndResultSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Aggregate("", (a, b) => a + b, s => s + "X"), "12345X");
		}


		[Test(Category = "Aggregate")]
		public void AverageWorksForArray() {
			Assert.AreEqual(new int[]     { 1, 2, 3, 4, 5 }.Average(), 3);
			Assert.AreEqual(new long[]    { 1, 2, 3, 4, 5 }.Average(), 3);
			Assert.AreEqual(new double[]  { 1, 2, 3, 4, 5 }.Average(), 3);
			Assert.AreEqual(new float[]   { 1, 2, 3, 4, 5 }.Average(), 3);
			Assert.AreEqual(new decimal[] { 1, 2, 3, 4, 5 }.Average(), 3);
		}

		[Test(Category = "Aggregate")]
		public void AverageWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Average(), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>    (long)i).Average(), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>   (float)i).Average(), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>  (double)i).Average(), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => (decimal)i).Average(), 3);
		}

		[Test(Category = "Aggregate")]
		public void AverageWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.Average(x => x.i), 3);
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.Average(x => x.i), 3);
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.Average(x => x.i), 3);
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.Average(x => x.i), 3);
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.Average(x => x.i), 3);
		}

		[Test(Category = "Aggregate")]
		public void AverageWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).Average(x => x.i), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).Average(x => x.i), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).Average(x => x.i), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).Average(x => x.i), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).Average(x => x.i), 3);
		}


		[Test(Category = "Aggregate")]
		public void CountWithoutPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Count(), 5);
		}

		[Test(Category = "Aggregate")]
		public void CountWithPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Count(i => i > 3), 2);
		}

		[Test(Category = "Aggregate")]
		public void CountWithoutPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Count(), 5);
		}

		[Test(Category = "Aggregate")]
		public void CountWithPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Count(i => i > 3), 2);
		}
		

		[Test(Category = "Aggregate")]
		public void MaxWorksForArray() {
			Assert.AreEqual(new int[]     { 1, 2, 3, 4, 5 }.Max(), 5);
			Assert.AreEqual(new long[]    { 1, 2, 3, 4, 5 }.Max(), 5);
			Assert.AreEqual(new double[]  { 1, 2, 3, 4, 5 }.Max(), 5);
			Assert.AreEqual(new float[]   { 1, 2, 3, 4, 5 }.Max(), 5);
			Assert.AreEqual(new decimal[] { 1, 2, 3, 4, 5 }.Max(), 5);
		}

		[Test(Category = "Aggregate")]
		public void MaxWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Max(), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>    (long)i).Max(), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>   (float)i).Max(), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>  (double)i).Max(), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => (decimal)i).Max(), 5);
		}

		[Test(Category = "Aggregate")]
		public void MaxWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.Max(x => x.i), 5);
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.Max(x => x.i), 5);
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.Max(x => x.i), 5);
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.Max(x => x.i), 5);
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.Max(x => x.i), 5);
		}

		[Test(Category = "Aggregate")]
		public void MaxWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).Max(x => x.i), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).Max(x => x.i), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).Max(x => x.i), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).Max(x => x.i), 5);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).Max(x => x.i), 5);
		}


		[Test(Category = "Aggregate")]
		public void MinWorksForArray() {
			Assert.AreEqual(new int[]     { 1, 2, 3, 4, 5 }.Min(), 1);
			Assert.AreEqual(new long[]    { 1, 2, 3, 4, 5 }.Min(), 1);
			Assert.AreEqual(new double[]  { 1, 2, 3, 4, 5 }.Min(), 1);
			Assert.AreEqual(new float[]   { 1, 2, 3, 4, 5 }.Min(), 1);
			Assert.AreEqual(new decimal[] { 1, 2, 3, 4, 5 }.Min(), 1);
		}

		[Test(Category = "Aggregate")]
		public void MinWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Min(), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>    (long)i).Min(), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>   (float)i).Min(), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>  (double)i).Min(), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => (decimal)i).Min(), 1);
		}

		[Test(Category = "Aggregate")]
		public void MinWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.Min(x => x.i), 1);
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.Min(x => x.i), 1);
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.Min(x => x.i), 1);
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.Min(x => x.i), 1);
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.Min(x => x.i), 1);
		}

		[Test(Category = "Aggregate")]
		public void MinWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).Min(x => x.i), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).Min(x => x.i), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).Min(x => x.i), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).Min(x => x.i), 1);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).Min(x => x.i), 1);
		}


		[Test(Category = "Aggregate")]
		public void MaxByWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.MaxBy(x => x.i), new { i = 5 });
		}

		[Test(Category = "Aggregate")]
		public void MaxByWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).MaxBy(x => x.i), new { i = 5 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).MaxBy(x => x.i), new { i = 5 });
		}


		[Test(Category = "Aggregate")]
		public void MinByWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.MinBy(x => x.i), new { i = 1 });
		}

		[Test(Category = "Aggregate")]
		public void MinByWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).MinBy(x => x.i), new { i = 1 });
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).MinBy(x => x.i), new { i = 1 });
		}


		[Test(Category = "Aggregate")]
		public void SumWorksForArray() {
			Assert.AreEqual(new int[]     { 1, 2, 3, 4, 5 }.Sum(), 15);
			Assert.AreEqual(new long[]    { 1, 2, 3, 4, 5 }.Sum(), 15);
			Assert.AreEqual(new double[]  { 1, 2, 3, 4, 5 }.Sum(), 15);
			Assert.AreEqual(new float[]   { 1, 2, 3, 4, 5 }.Sum(), 15);
			Assert.AreEqual(new decimal[] { 1, 2, 3, 4, 5 }.Sum(), 15);
		}

		[Test(Category = "Aggregate")]
		public void SumWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Sum(), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>    (long)i).Sum(), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>   (float)i).Sum(), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i =>  (double)i).Sum(), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => (decimal)i).Sum(), 15);
		}

		[Test(Category = "Aggregate")]
		public void SumWithSelectorWorksForArray() {
			Assert.AreEqual(new[] { new { i =     (int)1 }, new { i =     (int)2 }, new { i =     (int)3 }, new { i =     (int)4 }, new { i =     (int)5 } }.Sum(x => x.i), 15);
			Assert.AreEqual(new[] { new { i =    (long)1 }, new { i =    (long)2 }, new { i =    (long)3 }, new { i =    (long)4 }, new { i =    (long)5 } }.Sum(x => x.i), 15);
			Assert.AreEqual(new[] { new { i =   (float)1 }, new { i =   (float)2 }, new { i =   (float)3 }, new { i =   (float)4 }, new { i =   (float)5 } }.Sum(x => x.i), 15);
			Assert.AreEqual(new[] { new { i =  (double)1 }, new { i =  (double)2 }, new { i =  (double)3 }, new { i =  (double)4 }, new { i =  (double)5 } }.Sum(x => x.i), 15);
			Assert.AreEqual(new[] { new { i = (decimal)1 }, new { i = (decimal)2 }, new { i = (decimal)3 }, new { i = (decimal)4 }, new { i = (decimal)5 } }.Sum(x => x.i), 15);
		}

		[Test(Category = "Aggregate")]
		public void SumWithSelectorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =     (int)i }).Sum(x => x.i), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =    (long)i }).Sum(x => x.i), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =   (float)i }).Sum(x => x.i), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i =  (double)i }).Sum(x => x.i), 15);
			Assert.AreEqual(Enumerable.Range(1, 5).Select(i => new { i = (decimal)i }).Sum(x => x.i), 15);
		}
#endregion

#region Paging methods
		[Test(Category = "Paging")]
		public void ElementAtWorksForLinqArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ElementAt(2), 3);
		}

		[Test(Category = "Paging")]
		public void ElementAtWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ElementAt(2), 3);
		}


		[Test(Category = "Paging")]
		public void ElementAtOrDefaultWithoutDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ElementAtOrDefault(2), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ElementAtOrDefault(10), 0);
		}

		[Test(Category = "Paging")]
		public void ElementAtOrDefaultWithDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ElementAtOrDefault(2, -1), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ElementAtOrDefault(10, -1), -1);
		}

		[Test(Category = "Paging")]
		public void ElementAtOrDefaultWithoutDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ElementAtOrDefault(2), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).ElementAtOrDefault(10), 0);
		}

		[Test(Category = "Paging")]
		public void ElementAtOrDefaultWithDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ElementAtOrDefault(2, -1), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).ElementAtOrDefault(10, -1), -1);
		}


		[Test(Category = "Paging")]
		public void FirstWithoutPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.First(), 1);
		}

		[Test(Category = "Paging")]
		public void FirstWithPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.First(i => i > 2), 3);
		}

		[Test(Category = "Paging")]
		public void FirstWithoutPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).First(), 1);
		}

		[Test(Category = "Paging")]
		public void FirstWithPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).First(i => i > 2), 3);
		}


		[Test(Category = "Paging")]
		public void FirstOrDefaultWithoutPredicateOrDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(), 1);
			Assert.AreEqual(new int[0].FirstOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithoutPredicateWithDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(-1), 1);
			Assert.AreEqual(new int[0].FirstOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithPredicateWithoutDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(i => i > 2), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(i => i > 5), 0);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithPredicateAndDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(i => i > 2, -1), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.FirstOrDefault(i => i > 5, -1), -1);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithoutPredicateOrDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).FirstOrDefault(), 1);
			Assert.AreEqual(Enumerable.Range(1, 0).FirstOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithoutPredicateWithDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).FirstOrDefault(-1), 1);
			Assert.AreEqual(Enumerable.Range(1, 0).FirstOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithPredicateWithoutDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).FirstOrDefault(i => i > 2), 3);
			Assert.AreEqual(Enumerable.Range(1, 2).FirstOrDefault(i => i > 2), 0);
		}

		[Test(Category = "Paging")]
		public void FirstOrDefaultWithPredicateAndDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).FirstOrDefault(i => i > 2, -1), 3);
			Assert.AreEqual(Enumerable.Range(1, 2).FirstOrDefault(i => i > 2, -1), -1);
		}


		[Test(Category = "Paging")]
		public void LastWithoutPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Last(), 5);
		}

		[Test(Category = "Paging")]
		public void LastWithPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Last(i => i < 3), 2);
		}

		[Test(Category = "Paging")]
		public void LastWithoutPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Last(), 5);
		}

		[Test(Category = "Paging")]
		public void LastWithPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Last(i => i < 3), 2);
		}


		[Test(Category = "Paging")]
		public void LastOrDefaultWithoutPredicateOrDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(), 5);
			Assert.AreEqual(new int[0].LastOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithoutPredicateWithDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(-1), 5);
			Assert.AreEqual(new int[0].LastOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithPredicateWithoutDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(i => i < 3), 2);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(i => i < 0), 0);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithPredicateAndDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(i => i < 3, -1), 2);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LastOrDefault(i => i < 0, -1), -1);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithoutPredicateOrDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).LastOrDefault(), 5);
			Assert.AreEqual(Enumerable.Range(1, 0).LastOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithoutPredicateWithDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).LastOrDefault(-1), 5);
			Assert.AreEqual(Enumerable.Range(1, 0).LastOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithPredicateWithoutDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).LastOrDefault(i => i < 3), 2);
			Assert.AreEqual(Enumerable.Range(1, 2).LastOrDefault(i => i < 0), 0);
		}

		[Test(Category = "Paging")]
		public void LastOrDefaultWithPredicateAndDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).LastOrDefault(i => i < 3, -1), 2);
			Assert.AreEqual(Enumerable.Range(1, 2).LastOrDefault(i => i < 0, -1), -1);
		}


		[Test(Category = "Paging")]
		public void SingleWithoutPredicateWorksForArray() {
			Assert.AreEqual(new[] { 2 }.Single(), 2);
		}

		[Test(Category = "Paging")]
		public void SingleWithPredicateWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Single(i => i == 3), 3);
		}

		[Test(Category = "Paging")]
		public void SingleWithoutPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(2, 1).Single(), 2);
		}

		[Test(Category = "Paging")]
		public void SingleWithPredicateWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Single(i => i == 3), 3);
		}


		[Test(Category = "Paging")]
		public void SingleOrDefaultWithoutPredicateOrDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 2 }.SingleOrDefault(), 2);
			Assert.AreEqual(new int[0].SingleOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithoutPredicateWithDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 2 }.SingleOrDefault(-1), 2);
			Assert.AreEqual(new int[0].SingleOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithPredicateWithoutDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SingleOrDefault(i => i == 3), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SingleOrDefault(i => i == 9), 0);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithPredicateAndDefaultValueWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SingleOrDefault(i => i == 3, -1), 3);
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SingleOrDefault(i => i == 9, -1), -1);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithoutPredicateOrDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(2, 1).SingleOrDefault(), 2);
			Assert.AreEqual(Enumerable.Range(1, 0).SingleOrDefault(), 0);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithoutPredicateWithDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(2, 1).SingleOrDefault(-1), 2);
			Assert.AreEqual(Enumerable.Range(1, 0).SingleOrDefault(-1), -1);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithPredicateWithoutDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).SingleOrDefault(i => i == 3), 3);
			Assert.AreEqual(Enumerable.Range(1, 5).SingleOrDefault(i => i == 9), 0);
		}

		[Test(Category = "Paging")]
		public void SingleOrDefaultWithPredicateAndDefaultValueWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).SingleOrDefault(i => i == 3, -1), 3);
			Assert.AreEqual(Enumerable.Range(1, 2).SingleOrDefault(i => i == 9, -1), -1);
		}


		[Test(Category = "Paging")]
		public void SkipWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.Skip(2).ToArray(), new[] { 3, 4, 5 });
		}

		[Test(Category = "Paging")]
		public void SkipWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Skip(2).ToArray(), new[] { 3, 4, 5 });
		}


		[Test(Category = "Paging")]
		public void SkipWhileWithoutIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SkipWhile(i => i <= 2).ToArray(), new[] { 3, 4, 5 });
		}

		[Test(Category = "Paging")]
		public void SkipWhileWithIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.SkipWhile((i, idx) => (i + idx) <= 3).ToArray(), new[] { 3, 4, 5 });
		}

		[Test(Category = "Paging")]
		public void SkipWhileWithoutIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).SkipWhile(i => i <= 2).ToArray(), new[] { 3, 4, 5 });
		}

		[Test(Category = "Paging")]
		public void SkipWhileWithIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).SkipWhile((i, idx) => (i + idx) <= 3).ToArray(), new[] { 3, 4, 5 });
		}


		[Test(Category = "Paging")]
		public void TakeWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 1000).Take(3).ToArray(), new[] { 1, 2, 3 }, "Result should be correct");
		}

		[Test(Category = "Paging")]
		public void TakeWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 1000);
			Assert.AreEqual(enumerable.Take(3).ToArray(), new[] { 1, 2, 3 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}


		[Test(Category = "Paging")]
		public void TakeWhileWithoutIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.TakeWhile(i => i <= 2).ToArray(), new[] { 1, 2 });
		}

		[Test(Category = "Paging")]
		public void TakeWhileWithIndexWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.TakeWhile((i, idx) => (i + idx) <= 3).ToArray(), new[] { 1, 2 });
		}

		[Test(Category = "Paging")]
		public void TakeWhileWithoutIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).TakeWhile(i => i <= 2).ToArray(), new[] { 1, 2 });
		}

		[Test(Category = "Paging")]
		public void TakeWhileWithIndexWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).TakeWhile((i, idx) => (i + idx) <= 3).ToArray(), new[] { 1, 2 });
		}


		[Test(Category = "Paging")]
		public void TakeExceptLastWithoutArgumentWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.TakeExceptLast().ToArray(), new[] { 1, 2, 3, 4 });
		}

		[Test(Category = "Paging")]
		public void TakeExceptLastWithArgumentWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.TakeExceptLast(2).ToArray(), new[] { 1, 2, 3 });
		}

		[Test(Category = "Paging")]
		public void TakeExceptLastWithoutArgumentWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).TakeExceptLast().ToArray(), new[] { 1, 2, 3, 4 });
		}

		[Test(Category = "Paging")]
		public void TakeExceptLastWithArgumentWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).TakeExceptLast(2).ToArray(), new[] { 1, 2, 3 });
		}


		[Test(Category = "Paging")]
		public void TakeFromLastWithoutArgumentWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.TakeFromLast(2).ToArray(), new[] { 4, 5 });
		}

		[Test(Category = "Paging")]
		public void TakeFromLastWithoutArgumentWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).TakeFromLast(2).ToArray(), new[] { 4, 5 });
		}


		[Test(Category = "Paging")]
		public void IndexOfWorksForArray() {
			Assert.AreEqual(((IEnumerable<int>)new[] { 1, 2, 3, 4, 3, 2, 1 }).IndexOf(3), 2);
		}

		[Test(Category = "Paging")]
		public void IndexOfWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 3, 2, 1 }.Select(x => x).IndexOf(3), 2);
		}


		[Test(Category = "Paging")]
		public void LastIndexOfWorksForArray() {
			Assert.AreEqual(((IEnumerable<int>)new[] { 1, 2, 3, 4, 3, 2, 1 }).LastIndexOf(3), 4);
		}

		[Test(Category = "Paging")]
		public void LastIndexOfWorksForLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 3, 2, 1 }.Select(x => x).LastIndexOf(3), 4);
		}

#endregion

#region Convert methods
		[Test(Category = "Convert")]
		public void ToArrayWorksFromLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select(i => i * i).ToArray(), new[] { 1, 4, 9 });
		}

		[Test(Category = "Convert")]
		public void ToArrayWorksFromSatarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 3);
			Assert.AreEqual(enumerable.ToArray(), new[] { 1, 2, 3 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}


		[Test(Category = "Convert")]
		public void ToListWorksFromLinqJSEnumerable() {
			Assert.AreEqual(new[] { 1, 2, 3 }.Select(i => i * i).ToList(), new[] { 1, 4, 9 });
		}

		[Test(Category = "Convert")]
		public void ToListWorksFromSatarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 3);
			Assert.AreEqual(enumerable.ToList(), new[] { 1, 2, 3 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}

		[Test(Category = "Convert")]
		public void CanForeachOverLinqJSEnumerable() {
			var enumerable = new MyEnumerable(1, 3);
			var result = new List<int>();
			foreach (var i in enumerable.Select(i => i * i)) {
				result.Add(i);
			}
			Assert.AreEqual(result, new[] { 1, 4, 9 }, "Result should be correct");
			Assert.IsTrue(enumerable.EnumeratorDisposed, "Enumerator should be disposed");
		}


		[Test(Category = "Convert")]
		public void ToLookupWithOnlyKeySelectorWorksForArray() {
			var lu = new[] { "temp.xls", "temp.pdf", "temp.jpg", "temp2.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1]);
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "temp.xls" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "temp.pdf", "temp2.pdf" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "temp.jpg" });
		}

		[Test(Category = "Convert")]
		public void ToLookupWithOnlyKeySelectorWorksForLinqJSEnumerable() {
			var lu = new[] { "temp.xls", "temp.pdf", "temp.jpg", "temp2.pdf" }.Select(x => x).ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1]);
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "temp.xls" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "temp.pdf", "temp2.pdf" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "temp.jpg" });
		}

		[Test(Category = "Convert")]
		public void ToLookupWithKeyAndValueSelectorsWorksForArray() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "temp1" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "temp2", "temp4" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "temp3" });
		}

		[Test(Category = "Convert")]
		public void ToLookupWithKeyAndValueSelectorsWorksForLinqJSEnumerable() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.pdf" }.Select(x => x).ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Match(new Regex("^(.+)\\."))[1]);
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "temp1" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "temp2", "temp4" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "temp3" });
		}

		[Test(Category = "Convert")]
		public void ToLookupWithKeyAndValueAndCompareSelectorsWorksForArray() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.PDF" }.ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Substr(1), s => s.ToLower());
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "emp1.xls" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "emp2.pdf", "emp4.PDF" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "emp3.jpg" });
		}

		[Test(Category = "Convert")]
		public void ToLookupWithKeyAndValueAndCompareSelectorsWorksForLinqJSEnumerable() {
			var lu = new[] { "temp1.xls", "temp2.pdf", "temp3.jpg", "temp4.PDF" }.Select(x => x).ToLookup(s => s.Match(new Regex("\\.(.+$)"))[1], s => s.Substr(1), s => s.ToLower());
			Assert.AreEqual(lu.Count, 3);
			Assert.AreEqual(lu["xls"].ToArray(), new[] { "emp1.xls" });
			Assert.AreEqual(lu["pdf"].ToArray(), new[] { "emp2.pdf", "emp4.PDF" });
			Assert.AreEqual(lu["jpg"].ToArray(), new[] { "emp3.jpg" });
		}


		[Test(Category = "Convert")]
		public void ToObjectWorksForArray() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select((value, index) => new {id = "id_" + index, value}).ToArray().ToObject(item => item.id, item => item.value), new { id_0 = 1, id_1 = 2, id_2 = 3, id_3 = 4, id_4 = 5 });
		}

		[Test(Category = "Convert")]
		public void ToObjectWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).Select((value, index) => new {id = "id_" + index, value}).ToObject(item => item.id, item => item.value), new { id_0 = 1, id_1 = 2, id_2 = 3, id_3 = 4, id_4 = 5 });
		}


		[Test(Category = "Convert")]
		public void ToDictionaryWithOnlyKeySelectorWorksForArray() {
			var d = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToArray().ToDictionary(item => item.id);
			Assert.AreEqual(d.Count, 5);
			Assert.AreEqual(d["id_0"], new { id = "id_0", value = 1 });
			Assert.AreEqual(d["id_1"], new { id = "id_1", value = 2 });
			Assert.AreEqual(d["id_2"], new { id = "id_2", value = 3 });
			Assert.AreEqual(d["id_3"], new { id = "id_3", value = 4 });
			Assert.AreEqual(d["id_4"], new { id = "id_4", value = 5 });
		}

		[Test(Category = "Convert")]
		public void ToDictionaryWithOnlyKeySelectorWorksForLinqJSEnumerable() {
			var d = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id);
			Assert.AreEqual(d.Count, 5);
			Assert.AreEqual(d["id_0"], new { id = "id_0", value = 1 });
			Assert.AreEqual(d["id_1"], new { id = "id_1", value = 2 });
			Assert.AreEqual(d["id_2"], new { id = "id_2", value = 3 });
			Assert.AreEqual(d["id_3"], new { id = "id_3", value = 4 });
			Assert.AreEqual(d["id_4"], new { id = "id_4", value = 5 });
		}

		[Test(Category = "Convert")]
		public void ToDictionaryWithKeyAndValueSelectorsWorksForArray() {
			var d = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToArray().ToDictionary(item => item.id, item => item.value);
			Assert.AreEqual(d.Count, 5);
			Assert.AreEqual(d["id_0"], 1);
			Assert.AreEqual(d["id_1"], 2);
			Assert.AreEqual(d["id_2"], 3);
			Assert.AreEqual(d["id_3"], 4);
			Assert.AreEqual(d["id_4"], 5);
		}

		[Test(Category = "Convert")]
		public void ToDictionaryWithKeyAndValueSelectorsWorksForLinqJSEnumerable() {
			var d = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToDictionary(item => item.id, item => item.value);
			Assert.AreEqual(d.Count, 5);
			Assert.AreEqual(d["id_0"], 1);
			Assert.AreEqual(d["id_1"], 2);
			Assert.AreEqual(d["id_2"], 3);
			Assert.AreEqual(d["id_3"], 4);
			Assert.AreEqual(d["id_4"], 5);
		}

		[Test(Category = "Convert")]
		public void ToDictionaryWithKeyAndValueAndCompareSelectorsWorksForArray() {
			var items = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value }).ToArray();
			var d = items.ToDictionary(item => item, item => item.value, item => item.id);
			Assert.AreEqual(d.ToArray(), items.Select(x => new { key = x, x.value }).ToArray());
		}

		[Test(Category = "Convert")]
		public void ToDictionaryWithKeyAndValueAndCompareSelectorsWorksForLinqJSEnumerable() {
			var items = Enumerable.Range(1, 5).Select((value, index) => new { id = "id_" + index, value });
			var d = items.ToDictionary(item => item, item => item.value, item => item.id);
			Assert.AreEqual(d.ToArray(), items.Select(x => new { key = x, x.value }).ToArray());
		}


		[Test(Category = "Convert")]
		public void ToJoinedStringWithoutArgumentsWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ToJoinedString(), "12345");
		}

		[Test(Category = "Convert")]
		public void ToJoinedStringWithSeparatorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ToJoinedString(","), "1,2,3,4,5");
		}

		[Test(Category = "Convert")]
		public void ToJoinedStringWithSelectorAndSeparatorWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.ToJoinedString(",", i => (i + 1).ToString()), "2,3,4,5,6");
		}

		[Test(Category = "Convert")]
		public void ToJoinedStringWithoutArgumentsWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ToJoinedString(), "12345");
		}

		[Test(Category = "Convert")]
		public void ToJoinedStringWithSeparatorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ToJoinedString(","), "1,2,3,4,5");
		}

		[Test(Category = "Convert")]
		public void ToJoinedStringWithSelectorAndSeparatorWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1, 5).ToJoinedString(",", i => (i + 1).ToString()), "2,3,4,5,6");
		}
#endregion

#region Action methods
		[Test(Category = "Action")]
		public void DoActionWorksForArray() {
			var result = new List<int>();
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.DoAction(x => result.Add(x)).ToArray(), new[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Action")]
		public void DoActionWithIndexWorksForArray() {
			var result = new List<int>();
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.DoAction((x, idx) => { result.Add(x); result.Add(idx); }).ToArray(), new[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2, 4, 3, 5, 4 });
		}

		[Test(Category = "Action")]
		public void DoActionWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Assert.AreEqual(Enumerable.Range(1, 5).DoAction(x => result.Add(x)).ToArray(), new[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Action")]
		public void DoActionWithIndexWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Assert.AreEqual(Enumerable.Range(1, 5).DoAction((x, idx) => { result.Add(x); result.Add(idx); }).ToArray(), new[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2, 4, 3, 5, 4 });
		}


		[Test(Category = "Action")]
		public void ForEachWithSingleParameterWithoutReturnValueWorksForSaltarelleEnumerable() {
			var result = new List<int>();
			new MyEnumerable(1, 5).ForEach(i => result.Add(i));
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Action")]
		public void ForEachWithSingleParameterWithReturnValueWorksForSaltarelleEnumerable() {
			var result = new List<int>();
			new MyEnumerable(1, 5).ForEach(i => { result.Add(i); return i < 3; });
			Assert.AreEqual(result, new[] { 1, 2, 3 });
		}

		[Test(Category = "Action")]
		public void ForEachWithTwoParametersWithoutReturnValueWorksForSaltarelleEnumerable() {
			var result = new List<int>();
			new MyEnumerable(1, 5).ForEach((i, idx) => { result.Add(i); result.Add(idx); });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2, 4, 3, 5, 4 });
		}

		[Test(Category = "Action")]
		public void ForEachWithTwoParametersWithReturnValueWorksForSaltarelleEnumerable() {
			var result = new List<int>();
			new MyEnumerable(1, 5).ForEach((i, idx) => { result.Add(i); result.Add(idx); return i < 3; });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2 });
		}

		[Test(Category = "Action")]
		public void ForEachWithSingleParameterWithoutReturnValueWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Enumerable.Range(1, 5).ForEach(i => result.Add(i));
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, 5 });
		}

		[Test(Category = "Action")]
		public void ForEachWithSingleParameterWithReturnValueWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Enumerable.Range(1, 5).ForEach(i => { result.Add(i); return i < 3; });
			Assert.AreEqual(result, new[] { 1, 2, 3 });
		}

		[Test(Category = "Action")]
		public void ForEachWithTwoParametersWithoutReturnValueWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Enumerable.Range(1, 5).ForEach((i, idx) => { result.Add(i); result.Add(idx); });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2, 4, 3, 5, 4 });
		}

		[Test(Category = "Action")]
		public void ForEachWithTwoParametersWithReturnValueWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Enumerable.Range(1, 5).ForEach((i, idx) => { result.Add(i); result.Add(idx); return i < 3; });
			Assert.AreEqual(result, new[] { 1, 0, 2, 1, 3, 2 });
		}


		[Test(Category = "Action")]
		public void ForceWorksForSaltarelleEnumerable() {
			var enm = new MyEnumerable(1, 5);
			enm.Force();
			Assert.AreEqual(enm.LastReturnedValue, 5);
		}

		[Test(Category = "Action")]
		public void ForceWorksForLinqJSEnumerable() {
			var result = new List<int>();
			Enumerable.Range(1, 5).DoAction(x => result.Add(x)).Force();
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, 5 });
		}
#endregion

#region Functional
//		This test fails due to what seems like a bug in linq.js
//		[Test(Category = "Functional")]
		public void LetBindWorksForArray() {
			Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }.LetBind(a => a.Zip(a.Skip(1), (x, y) => x + ":" + y)).ToArray(), new[] { "1:2", "2:3", "3:4", "4:5" });
		}

		[Test(Category = "Functional")]
		public void LetBindWorksForLinqJSEnumerable() {
			Assert.AreEqual(Enumerable.Range(1,5).LetBind(a => a.Zip(a.Skip(1), (x, y) => x + ":" + y)).ToArray(), new[] { "1:2", "2:3", "3:4", "4:5" });
		}


		[Test(Category = "Functional")]
		public void ShareWorksForArray() {
			var result = new List<int>();
			var enm = new[] { 1, 2, 3, 4, 5 }.Share();
			enm.Take(3).ForEach(i => result.Add(i));
			result.Add(-1);
			enm.ForEach(i => result.Add(i));
			Assert.AreEqual(result, new[] { 1, 2, 3, -1, 4, 5 });
		}

		[Test(Category = "Functional")]
		public void ShareWorksForLinqJSEnumerable() {
			var result = new List<int>();
			var enm = Enumerable.Range(1, 5).Share();
			enm.Take(3).ForEach(i => result.Add(i));
			result.Add(-1);
			enm.ForEach(i => result.Add(i));
			Assert.AreEqual(result, new[] { 1, 2, 3, -1, 4, 5 });
		}


		[Test(Category = "Functional")]
		public void MemoizeWorksForSaltarelleEnumerable() {
			var enumerable = new MyEnumerable(1, 5);
			var enm = enumerable.Memoize();
			enm.Where(i => i % 2 == 0).Force();
			enm.Where(i => i % 2 == 0).Force();
			Assert.AreEqual(5, enumerable.NumMoveNextCalls);
		}

		[Test(Category = "Functional")]
		public void MemoizeWorksForLinqJSEnumerable() {
			var result = new List<string>();
			var enm = Enumerable.Range(1, 5).DoAction(i => result.Add("--->" + i)).Memoize();
			enm.Where(i => i % 2 == 0).ForEach(i => result.Add(i.ToString()));
			result.Add("---");
			enm.Where(i => i % 2 == 0).ForEach(i => result.Add(i.ToString()));
			Assert.AreEqual(result, new[] { "--->1", "--->2", "2", "--->3", "--->4", "4", "--->5", "---" , "2", "4" });
		}
#endregion

#region Error handling
		[Test(Category = "Error handling")]
		public void CatchErrorWorksSaltarelleEnumerable() {
			string errorMessage = null;
			var enumerable = new MyEnumerable(1, 10) { ThrowOnIndex = 4 };
			var result = enumerable.CatchError(ex => errorMessage = ex.Message).ToArray();
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, });
			Assert.AreEqual(errorMessage, "error");
		}

		[Test(Category = "Error handling")]
		public void CatchErrorWorksForLinqJSEnumerable() {
			string errorMessage = null;
			var result = Enumerable.Range(1, 10).Select(i => { if (i == 5) throw new Exception("enumerable_error"); return i; }).CatchError(ex => errorMessage = ex.Message).ToArray();
			Assert.AreEqual(result, new[] { 1, 2, 3, 4, });
			Assert.AreEqual(errorMessage, "enumerable_error");
		}

		[Test(Category = "Error handling")]
		public void FinallyActionWorksForArray() {
			bool finallyRun = false;
			new[] { 1, 2, 3, 4, 5 }.FinallyAction(() => finallyRun = true).Force();
			Assert.IsTrue(finallyRun);
		}

		[Test(Category = "Error handling")]
		public void FinallyActionWorksForLinqJSEnumerable() {
			bool finallyRun = false;
			Enumerable.Range(1, 10).FinallyAction(() => finallyRun = true).Force();
			Assert.IsTrue(finallyRun);
		}

#endregion

#region For debug
		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithoutArgumentsWorksForArray() {
			new[] { 1, 2, 3, 4, 5 }.Trace().Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithMessageWorksForArray() {
			new[] { 1, 2, 3, 4, 5 }.Trace("X").Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithMessageAndSelectorWorksForArray() {
			new[] { 1, 2, 3, 4, 5 }.Trace("X", i => i.ToString()).Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithoutArgumentsWorksForLinqJSEnumerable() {
			Enumerable.Range(1, 10).Trace().Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithMessageWorksForLinqJSEnumerable() {
			Enumerable.Range(1, 10).Trace("X").Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

		[Test(Category = "For debug", ExpectedAssertionCount = 0)]
		public void TraceWithMessageAndSelectorWorksForLinqJSEnumerable() {
			Enumerable.Range(1, 10).Trace("X", i => i.ToString()).Force();
			// Writes to console.log, assume it works if we don't get errors.
		}

#endregion
	}
}
