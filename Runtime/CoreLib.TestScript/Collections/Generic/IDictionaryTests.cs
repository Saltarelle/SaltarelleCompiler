using System;
using System.Collections.Generic;
using QUnit;
using System.Collections;

namespace CoreLib.TestScript.Collections.Generic {
	[TestFixture]
	public class IDictionaryTests {
		private class MyDictionary : IDictionary<int, string>, IReadOnlyDictionary<int, string> {
			private readonly Dictionary<int, string> _backingDictionary;

			public MyDictionary() : this(new Dictionary<int, string>()) {
			}

			public MyDictionary(Dictionary<int, string> initialValues) {
				_backingDictionary = initialValues;
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public IEnumerator<KeyValuePair<int, string>> GetEnumerator() {
				return _backingDictionary.GetEnumerator();
			}

			public string this[int key] {
				get { return _backingDictionary[key]; }
				set { _backingDictionary[key] = value; }
			}

			public ICollection<int> Keys {
				get { return _backingDictionary.Keys; }
			}

			public ICollection<string> Values {
				get { return _backingDictionary.Values; }
			}

			public int Count { get { return _backingDictionary.Count; } }

			public void Add(int key, string value) {
				_backingDictionary.Add(key, value);
			}

			public bool Remove(int key) {
				return _backingDictionary.Remove(key);
			}

			public bool ContainsKey(int key) {
				return _backingDictionary.ContainsKey(key);
			}

			public bool TryGetValue(int key, out string value) {
				return _backingDictionary.TryGetValue(key, out value);
			}

			public void Clear() {
				_backingDictionary.Clear();
			}
		}


		[Test]
		public void ClassImplementsInterfaces() {
			Assert.IsTrue((object)new MyDictionary() is IDictionary<int, string>);
			Assert.IsTrue((object)new MyDictionary() is IReadOnlyDictionary<int, string>);
		}

		[Test]
		public void CountWorks() {
			var d = new MyDictionary();
			Assert.AreEqual(d.Count, 0);

			var d2 = new MyDictionary(new Dictionary<int, string>{ { 3, "c"} });
			Assert.AreEqual(d2.Count, 1);

			var d3 = new MyDictionary();
			Assert.AreEqual(d3.Count, 0);
		}

		[Test]
		public void KeysWorks() {
			var actualKeys = new int[] {3,6,9};
			var d = new MyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var keys = ((IReadOnlyDictionary<int,string>)d).Keys;
			Assert.IsTrue(keys is IEnumerable<int>);
			Assert.IsTrue(keys is ICollection<KeyValuePair<int, string>>);

			int i = 0;
			foreach (var key in keys) {
				Assert.AreEqual(key, actualKeys[i]);
				i++;
			}
			Assert.AreEqual(i, actualKeys.Length);
		}

		[Test]
		public void GetItemWorks() {
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var di = (IReadOnlyDictionary<int, string>)d;
			var di2 = (IDictionary<int, string>)d;

			Assert.AreEqual(d[9], "x");
			Assert.AreEqual(di[3], "b");
			Assert.AreEqual(di2[6], "z");

			try {
				var x = d[1];
				Assert.Fail("Should throw");
			}
			catch (Exception) {
			}

			try {
				var x = di[1];
				Assert.Fail("Should throw");
			}
			catch (Exception) {
			}

			try {
				var x = di2[1];
				Assert.Fail("Should throw");
			}
			catch (Exception) {
			}
		}

		[Test]
		public void ValuesWorks() {
			var actualValues = new string[] { "b", "z", "x" };
			var d2 = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var values = ((IReadOnlyDictionary<int, string>)d2).Values;
			Assert.IsTrue(values is IEnumerable<int>);

			int i = 0;

			foreach (var val in values) {
				Assert.AreEqual(val, actualValues[i]);
				i++;
			}
			Assert.AreEqual(i, actualValues.Length);
		}

		[Test]
		public void ContainsKeyWorks() {
			var d = new MyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var di = (IReadOnlyDictionary<int,string>)d;
			var di2 = (IDictionary<int,string>)d;
			
			Assert.IsTrue(d.ContainsKey(9));
			Assert.IsTrue(di.ContainsKey(6));
			Assert.IsTrue(di2.ContainsKey(3));
			
			Assert.IsFalse(d.ContainsKey(923));
			Assert.IsFalse(di.ContainsKey(6124));
			Assert.IsFalse(di2.ContainsKey(353));
		}

		[Test]
		public void TryGetValueWorks() {
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var di = (IReadOnlyDictionary<int, string>)d;
			var di2 = (IDictionary<int, string>)d;

			string outVal;
			Assert.IsTrue(d.TryGetValue(9, out outVal));
			Assert.AreEqual(outVal, "x");
			Assert.IsTrue(di.TryGetValue(6, out outVal));
			Assert.AreEqual(outVal, "z");
			Assert.IsTrue(di2.TryGetValue(3, out outVal));
			Assert.AreEqual(outVal, "b");

			outVal = "!!!";
			Assert.IsFalse(d.TryGetValue(923, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(di.TryGetValue(6124, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(di2.TryGetValue(353, out outVal));
			Assert.AreEqual(outVal, null);
		}

		[Test]
		public void AddWorks() {
			var d = new MyDictionary();
			var di = (IDictionary<int, string>)d;
			var di2 = (ICollection<KeyValuePair<int, string>>)d;

			d.Add(5, "aa");
			Assert.AreEqual(d[5], "aa");
			Assert.IsTrue(di2.Contains(new KeyValuePair<int,string>(5, "aa")));
			Assert.AreEqual(d.Count, 1);

			di.Add(3, "bb");
			Assert.AreEqual(di[3], "bb");
			Assert.IsTrue(di2.Contains(new KeyValuePair<int,string>(3, "bb")));
			Assert.AreEqual(di.Count, 2);

			di2.Add(new KeyValuePair<int,string>(1, "cc"));
			Assert.AreEqual(di[1], "cc");
			Assert.IsTrue(di2.Contains(new KeyValuePair<int,string>(1, "cc")));
			Assert.AreEqual(di2.Count, 3);

			try {
				d.Add(5, "zz");
				Assert.Fail("Should throw");
			}
			catch (Exception) {
			}
		}

		[Test]
		public void ClearWorks() {
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });

			Assert.AreEqual(d.Count, 3);
			d.Clear();
			Assert.AreEqual(d.Count, 0);

			var di = (ICollection<KeyValuePair<int, string>>)new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });

			Assert.AreEqual(di.Count, 3);
			di.Clear();
			Assert.AreEqual(di.Count, 0);
		}

		[Test]
		public void RemoveWorks() {
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" }, {13, "y"} });
			var di = (IDictionary<int, string>)d;

			Assert.AreStrictEqual(d.Remove(6), true);
			Assert.AreEqual(d.Count, 3);
			Assert.IsFalse(d.ContainsKey(6));

			Assert.AreStrictEqual(di.Remove(3), true);
			Assert.AreEqual(di.Count, 2);
			Assert.IsFalse(di.ContainsKey(3));

			Assert.IsTrue(di.ContainsKey(13));
		}

		[Test]
		public void SetItemWorks() {
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" }, { 13, "y" } });
			var di = (IDictionary<int, string>)d;

			d[3] = "check";
			Assert.AreEqual(d[3], "check");
			Assert.IsTrue(d.ContainsKey(10));

			di[10] = "stuff";
			Assert.AreEqual(di[10], "stuff");
			Assert.IsTrue(di.ContainsKey(10));
		}
	}
}
