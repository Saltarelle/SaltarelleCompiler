using System;
using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript.Collections.Generic {
	[TestFixture]
	public class GenericDictionaryTests {
		class TestEqualityComparer : EqualityComparer<string> {
			public override bool Equals(string x, string y) {
				return x[0] == y[0];
			}

			public override int GetHashCode(string obj) {
				return obj[0];
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Dictionary<int, string>).FullName, "ss.Dictionary$2[ss.Int32,String]", "FullName should be correct");
			Assert.IsTrue(typeof(Dictionary<int, string>).IsClass, "IsClass should be true");
			object dict = new Dictionary<int, string>();
			Assert.IsTrue(dict is Dictionary<int, string>, "is Dictionary<int,string> should be true");
			Assert.IsTrue(dict is IDictionary<int, string>, "is IDictionary<int,string> should be true");
			Assert.IsTrue(dict is IEnumerable<KeyValuePair<int,string>>, "is IEnumerable<KeyValuePair<int,string>> should be true");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var d = new Dictionary<int, string>();
			Assert.AreEqual(d.Count, 0);
			Assert.AreStrictEqual(d.Comparer, EqualityComparer<int>.Default);
		}

		[Test]
		public void CapacityConstructorWorks() {
			var d = new Dictionary<int, string>(10);
			Assert.AreEqual(d.Count, 0);
			Assert.AreStrictEqual(d.Comparer, EqualityComparer<int>.Default);
		}

		[Test]
		public void CapacityAndEqualityComparerWorks() {
			var c = new TestEqualityComparer();
			var d = new Dictionary<string, string>(10, c);
			Assert.AreEqual(d.Count, 0);
			Assert.AreStrictEqual(d.Comparer, c);
		}

		[Test]
		public void JsDictionaryConstructorWorks() {
			var orig = JsDictionary<string, int>.GetDictionary(new { a = 1, b = 2 });
			var d = new Dictionary<string, int>(orig);
			Assert.IsFalse((object)d == (object)orig);
			Assert.AreEqual(d.Count, 2);
			Assert.AreEqual(d["a"], 1);
			Assert.AreEqual(d["b"], 2);
			Assert.AreStrictEqual(d.Comparer, EqualityComparer<string>.Default);
		}

		[Test]
		public void JsDictionaryAndEqualityComparerConstructorWorks() {
			var c = new TestEqualityComparer();
			var orig = JsDictionary<string, int>.GetDictionary(new { a = 1, b = 2 });
			var d = new Dictionary<string, int>(orig, c);
			Assert.IsFalse((object)d == (object)orig);
			Assert.AreEqual(d.Count, 2);
			Assert.AreEqual(d["a"], 1);
			Assert.AreEqual(d["b"], 2);
			Assert.AreStrictEqual(d.Comparer, c);
		}

		[Test]
		public void CopyConstructorWorks() {
			var orig = JsDictionary<string,int>.GetDictionary(new { a = 1, b = 2 });
			var d = new Dictionary<string, int>(orig);
			var d2 = new Dictionary<string, int>(d);
			Assert.IsFalse((object)d == (object)d2);
			Assert.AreEqual(d2.Count, 2);
			Assert.AreEqual(d2["a"], 1);
			Assert.AreEqual(d2["b"], 2);
			Assert.AreStrictEqual(d2.Comparer, EqualityComparer<string>.Default);
		}

		[Test]
		public void EqualityComparerOnlyConstructorWorks() {
			var c = new TestEqualityComparer();
			var d = new Dictionary<string, int>(c);
			Assert.AreEqual(d.Count, 0);
			Assert.AreStrictEqual(d.Comparer, c);
		}

		[Test]
		public void ConstructorWithBothDictionaryAndEqualityComparerWorks() {
			var c = new TestEqualityComparer();
			var orig = JsDictionary<string,int>.GetDictionary(new { a = 1, b = 2 });
			var d = new Dictionary<string, int>(orig);
			var d2 = new Dictionary<string, int>(d, c);
			Assert.IsFalse((object)d == (object)d2);
			Assert.AreEqual(d2.Count, 2);
			Assert.AreEqual(d2["a"], 1);
			Assert.AreEqual(d2["b"], 2);
			Assert.AreStrictEqual(d2.Comparer, c);
		}

		[Test]
		public void CountWorks() {
			var d = new Dictionary<int, string>();
			Assert.AreEqual(d.Count, 0);
			d.Add(1, "1");
			Assert.AreEqual(d.Count, 1);
			d.Add(2, "2");
			Assert.AreEqual(d.Count, 2);
		}

		[Test]
		public void KeysWorks() {
			var d = new Dictionary<string, string> { { "1", "a" }, { "2", "b" } };
			var keys = d.Keys;
			Assert.IsTrue((object)keys is IEnumerable<string>);
			Assert.IsTrue((object)keys is ICollection<string>);
			Assert.AreEqual(keys.Count, 2);
			Assert.IsTrue(keys.Contains("1"));
			Assert.IsTrue(keys.Contains("2"));
			Assert.IsFalse(keys.Contains("a"));

			int count = 0;
			foreach (var key in d.Keys) {
				if (key != "1" && key != "2") {
					Assert.Fail("Unexpected key " + key);
				}
				count++;
			}
			Assert.AreEqual(count, 2);
		}

		[Test]
		public void ValuesWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			var values = d.Values;
			Assert.IsTrue((object)values is IEnumerable<string>);
			Assert.IsTrue((object)values is ICollection<string>);
			Assert.AreEqual(values.Count, 2);
			Assert.IsTrue(values.Contains("a"));
			Assert.IsTrue(values.Contains("b"));
			Assert.IsFalse(values.Contains("1"));

			int count = 0;
			foreach (var value in d.Values) {
				if (value != "a" && value != "b") {
					Assert.Fail("Unexpected key " + value);
				}
				count++;
			}
			Assert.AreEqual(count, 2);
		}

		[Test]
		public void IndexerGetterWorksForExistingItems() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			Assert.AreEqual(d[1], "a");
		}

		[Test]
		public void IndexerSetterWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			d[2] = "c";
			d[3] = "d";
			Assert.AreEqual(3, d.Count);
			Assert.AreEqual(d[1], "a");
			Assert.AreEqual(d[2], "c");
			Assert.AreEqual(d[3], "d");
		}

		[Test(ExpectedAssertionCount = 0)]
		public void IndexerGetterThrowsForNonExistingItems() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			try {
				var x = d[10];
				Assert.IsTrue(false);
			}
			catch (KeyNotFoundException) {
			}
		}

		[Test]
		public void AddWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			d.Add(3, "c");
			Assert.AreEqual(3, d.Count);
			Assert.AreEqual(d[1], "a");
			Assert.AreEqual(d[2], "b");
			Assert.AreEqual(d[3], "c");
		}

		[Test(ExpectedAssertionCount = 0)]
		public void AddThrowsIfItemAlreadyExists() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			try {
				d.Add(2, "b");
				Assert.IsTrue(false);
			}
			catch (ArgumentException) {
			}
		}

		[Test]
		public void ClearWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			d.Clear();
			Assert.AreEqual(d.Count, 0);
		}

		[Test]
		public void ContainsKeyWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			Assert.IsTrue(d.ContainsKey(1));
			Assert.IsFalse(d.ContainsKey(3));
		}

		[Test]
		public void EnumeratingWorks() {
			var d = new Dictionary<string, string> { { "1", "a" }, { "2", "b" } };
			int count = 0;
			foreach (var kvp in d) {
				if (kvp.Key == "1") {
					Assert.AreEqual(kvp.Value, "a");
				}
				else if (kvp.Key == "2") {
					Assert.AreEqual(kvp.Value, "b");
				}
				else {
					Assert.Fail("Invalid key " + kvp.Key);
				}
				count++;
			}
			Assert.AreEqual(count, 2);
		}

		[Test]
		public void RemoveWorks() {
			var d = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
			Assert.AreStrictEqual(d.Remove(2), true);
			Assert.AreStrictEqual(d.Remove(3), false);
			Assert.AreEqual(d.Count, 1);
			Assert.AreEqual(d[1], "a");
		}

		[Test]
		public void TryGetValueWithIntKeysWorks() {
			var d = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
			int i;

			Assert.IsTrue(d.TryGetValue("a", out i));
			Assert.AreEqual(i, 1);
			Assert.IsFalse(d.TryGetValue("c", out i));
			Assert.AreEqual(i, 0);
		}

		[Test]
		public void TryGetValueWithObjectKeysWorks() {
			var d = new Dictionary<string, object> { { "a", 1 }, { "b", "X" } };
			object o;

			Assert.IsTrue(d.TryGetValue("a", out o));
			Assert.AreEqual(o, 1);
			Assert.IsFalse(d.TryGetValue("c", out o));
			Assert.AreStrictEqual(o, null);
		}

		[Test]
		public void CanUseCustomComparer() {
			var d = new Dictionary<string, int>(new TestEqualityComparer()) { { "a", 1 }, { "b", 2 } };
			d["a2"] = 100;
			Assert.AreEqual(d["a3"], 100);
			Assert.AreEqual(d.Count, 2);
		}
	}
}
