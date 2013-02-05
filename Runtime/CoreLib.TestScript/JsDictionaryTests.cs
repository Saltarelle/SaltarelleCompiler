using System.Collections;
using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class JsDictionaryTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(JsDictionary).FullName, "Object");
			Assert.IsTrue(typeof(JsDictionary).IsClass);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var d = new JsDictionary();
			Assert.IsTrue(d != null);
			Assert.AreEqual(d.Count, 0);
		}

		[Test]
		public void NameValuePairsConstructorWorks() {
			var d = new JsDictionary("a", "valueA", "b", 134);
			Assert.AreEqual(d.Count, 2);
			Assert.AreEqual(d["a"], "valueA");
			Assert.AreEqual(d["b"], 134);
		}

		[Test]
		public void KeysWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			var keys = d.Keys;
			Assert.IsTrue(keys.Contains("a"));
			Assert.IsTrue(keys.Contains("b"));
		}

		[Test]
		public void IndexingWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			Assert.AreEqual(d["a"], "valueA");
			Assert.AreEqual(d["b"], 134);
		}

		[Test]
		public void ClearWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			d.Clear();
			Assert.AreEqual(d.Count, 0);
		}

		[Test]
		public void ContainsKeyWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			Assert.IsTrue(d.ContainsKey("a"));
			Assert.IsFalse(d.ContainsKey("c"));
		}

		[Test]
		public void GetDictionaryWorks() {
			var obj = new { a = "valueA", b = 134 };
			var d = JsDictionary.GetDictionary(obj);
			Assert.AreStrictEqual(d, obj);
			Assert.AreEqual(2, d.Keys.Count);
			Assert.AreEqual(d["a"], "valueA");
			Assert.AreEqual(d["b"], 134);
		}

		[Test]
		public void GetEnumeratorWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			var d2 = new JsDictionary();
			foreach (var kvp in d) {
				d2[kvp.Key] = kvp.Value;
			}
			Assert.AreEqual(d, d2);
		}

		[Test]
		public void RemoveWorks() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			d.Remove("a");
			Assert.AreEqual(d.Keys, new[] { "b" });
		}

		[Test]
		public void ConvertingToGenericReturnsSameInstance() {
			var d = JsDictionary.GetDictionary(new { a = "valueA", b = 134 });
			var d2 = (JsDictionary<string, object>)d;
			Assert.AreStrictEqual(d2, d);
		}

		[Test]
		public void ConvertingFromGenericReturnsSameInstance() {
			var d = JsDictionary<string, object>.GetDictionary(new { a = "valueA", b = 134 });
			var d2 = (JsDictionary)d;
			Assert.AreStrictEqual(d2, d);
		}
	}
}
