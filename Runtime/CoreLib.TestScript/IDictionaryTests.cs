using System;
using System.Collections.Generic;
using QUnit;
using System.Collections;

namespace CoreLib.TestScript
{
	[TestFixture]
	public class IDictionaryTests
	{
		private class MyReadOnlyDictionary : IReadOnlyDictionary<int, string>
		{
			protected Dictionary<int, string> BackingDictionary { get; private set; }
			
			private IReadOnlyCollection<KeyValuePair<int, string>> m_CollectionCast;

			public MyReadOnlyDictionary()
				: this(new Dictionary<int, string>())
			{
			}

			public MyReadOnlyDictionary(IReadOnlyDictionary<int, string> initialValues)
			{
				BackingDictionary = new Dictionary<int,string>(initialValues);
				m_CollectionCast = BackingDictionary;
			}

			public string this[int key]
			{
				get { return BackingDictionary[key]; }
			}

			public new IEnumerable<int> Keys
			{
				get { return BackingDictionary.Keys; }
			}

			public IEnumerable<string> Values
			{
				get { return BackingDictionary.Values; }
			}

			public bool ContainsKey(int key)
			{
				return BackingDictionary.ContainsKey(key);
			}

			public bool TryGetValue(int key, out string value)
			{
				return BackingDictionary.TryGetValue(key, out value);
			}

			public int Count
			{
				get { return BackingDictionary.Count; }
			}

			public bool Contains(KeyValuePair<int, string> item)
			{
				return m_CollectionCast.Contains(item);
			}

			public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
			{
				return BackingDictionary.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return BackingDictionary.GetEnumerator();
			}
		}

		private class MyDictionary : MyReadOnlyDictionary, IDictionary<int, string>
		{
			private ICollection<KeyValuePair<int, string>> m_CollectionCast;
			public MyDictionary()
				: this(new Dictionary<int, string>())
			{
			}

			public MyDictionary(IReadOnlyDictionary<int, string> initialValues)
				: base(initialValues)
			{
				m_CollectionCast = BackingDictionary;
			}

			public void Add(int key, string value)
			{
				BackingDictionary.Add(key, value);
			}

			public new string this[int key]
			{
				get
				{
					return BackingDictionary[key];
				}
				set
				{
					BackingDictionary[key] = value;
				}
			}

			public new ICollection<int> Keys
			{
				get { return BackingDictionary.Keys; }
			}

			public new ICollection<string> Values
			{
				get { return BackingDictionary.Values; }
			}

			public bool Remove(int key)
			{
				return BackingDictionary.Remove(key);
			}

			public void Add(KeyValuePair<int, string> item)
			{
				m_CollectionCast.Add(item);
			}

			public void Clear()
			{
				BackingDictionary.Clear();
			}

			public bool Remove(KeyValuePair<int, string> item)
			{
				return m_CollectionCast.Remove(item);
			}

			public new IEnumerator GetEnumerator()
			{
				return BackingDictionary.GetEnumerator();
			}
		}


		[Test]
		public void ClassImplementsInterfaces()
		{
			Assert.IsTrue((object)new MyReadOnlyDictionary() is IReadOnlyDictionary<int, string>);
			Assert.IsTrue((object)new MyDictionary() is IDictionary<int, string>);
			Assert.IsTrue((object)new MyDictionary() is IReadOnlyDictionary<int, string>);
		}

		[Test]
		public void CountWorks()
		{
			var d = new MyReadOnlyDictionary();
			Assert.AreEqual(d.Count, 0);

			var d2 = new MyReadOnlyDictionary(new Dictionary<int, string>{ { 3, "c"} });
			Assert.AreEqual(d2.Count, 1);

			var d3 = new MyDictionary();
			Assert.AreEqual(d3.Count, 0);
		}

		[Test]
		public void KeysWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var actualKeys = new int[] {3,6,9};

			var keys = d.Keys;
			Assert.IsTrue(keys is IEnumerable<int>);
			int i = 0;

			foreach (var key in keys)
			{
				Assert.AreEqual(key, actualKeys[i]);
				i++;
			}
			Assert.AreEqual(i, actualKeys.Length);

			keys = ((IReadOnlyDictionary<int,string>)d).Keys;
			Assert.IsTrue(keys is IEnumerable<int>);

			i = 0;
			foreach (var key in keys)
			{
				Assert.AreEqual(key, actualKeys[i]);
				i++;
			}
			Assert.AreEqual(i, actualKeys.Length);

			var d2 = new MyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			keys = ((IReadOnlyDictionary<int,string>)d2).Keys;
			Assert.IsTrue(keys is IEnumerable<int>);
			Assert.IsTrue(keys is ICollection<KeyValuePair<int, string>>);

			i = 0;
			foreach (var key in keys)
			{
				Assert.AreEqual(key, actualKeys[i]);
				i++;
			}
			Assert.AreEqual(i, actualKeys.Length);
		}

		[Test]
		public void ContainsWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var di = (IReadOnlyDictionary<int,string>)d;
			var d2 = new MyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var d2i = (IReadOnlyDictionary<int,string>)d2;
			var d2i2 = (IDictionary<int,string>)d2;
			
			Assert.IsTrue(d.Contains(new KeyValuePair<int,string>(6, "z")));
			Assert.IsTrue(di.Contains(new KeyValuePair<int,string>(3, "b")));
			Assert.IsTrue(d2.Contains(new KeyValuePair<int,string>(9, "x")));
			Assert.IsTrue(d2i.Contains(new KeyValuePair<int,string>(6, "z")));
			Assert.IsTrue(d2i2.Contains(new KeyValuePair<int,string>(3, "b")));
			
			Assert.IsFalse(d.Contains(new KeyValuePair<int,string>(6, "zxzc")));
			Assert.IsFalse(di.Contains(new KeyValuePair<int,string>(32, "b")));
			Assert.IsFalse(d2.Contains(new KeyValuePair<int,string>(923, "x")));
			Assert.IsFalse(d2i.Contains(new KeyValuePair<int,string>(6, "sdafz")));
			Assert.IsFalse(d2i2.Contains(new KeyValuePair<int,string>(353, "b332")));
		}

		[Test]
		public void GetItemWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var di = (IReadOnlyDictionary<int, string>)d;
			var d2 = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var d2i = (IReadOnlyDictionary<int, string>)d2;
			var d2i2 = (IDictionary<int, string>)d2;


			Assert.AreEqual(d[3], "b");
			Assert.AreEqual(di[6], "z");
			Assert.AreEqual(d2[9], "x");
			Assert.AreEqual(d2i[3], "b");
			Assert.AreEqual(d2i2[6], "z");

			try
			{
				var x = d[1];
				Assert.IsTrue(false);
			}
			catch (Exception)
			{
			}
			try
			{
				var x = di[1];
				Assert.IsTrue(false);
			}
			catch (Exception)
			{
			}
			try
			{
				var x = d2[1];
				Assert.IsTrue(false);
			}
			catch (Exception)
			{
			}
			try
			{
				var x = d2i[1];
				Assert.IsTrue(false);
			}
			catch (Exception)
			{
			}
			try
			{
				var x = d2i2[1];
				Assert.IsTrue(false);
			}
			catch (Exception)
			{
			}
		}

		[Test]
		public void ValuesWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var actualValues = new string[] { "b", "z", "x" };

			var values = d.Values;
			int i = 0;

			Assert.IsTrue(values is IEnumerable<int>);
			foreach (var val in values)
			{
				Assert.AreEqual(val, actualValues[i]);
				i++;
			}
			Assert.AreEqual(i, actualValues.Length);

			values = ((IReadOnlyDictionary<int, string>)d).Values;
			Assert.IsTrue(values is IEnumerable<int>);

			i = 0;

			foreach (var val in values)
			{
				Assert.AreEqual(val, actualValues[i]);
				i++;
			}
			Assert.AreEqual(i, actualValues.Length);

			var d2 = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			values = ((IReadOnlyDictionary<int, string>)d2).Values;
			Assert.IsTrue(values is IEnumerable<int>);

			i = 0;

			foreach (var val in values)
			{
				Assert.AreEqual(val, actualValues[i]);
				i++;
			}
			Assert.AreEqual(i, actualValues.Length);
		}

		[Test]
		public void ContainsKeyWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var di = (IReadOnlyDictionary<int,string>)d;
			var d2 = new MyDictionary(new Dictionary<int, string>{ { 3, "b"}, {6, "z"}, {9, "x"} });
			var d2i = (IReadOnlyDictionary<int,string>)d2;
			var d2i2 = (IDictionary<int,string>)d2;
			
			Assert.IsTrue(d.ContainsKey(6));
			Assert.IsTrue(di.ContainsKey(3));
			Assert.IsTrue(d2.ContainsKey(9));
			Assert.IsTrue(d2i.ContainsKey(6));
			Assert.IsTrue(d2i2.ContainsKey(3));
			
			Assert.IsFalse(d.ContainsKey(6123));
			Assert.IsFalse(di.ContainsKey(32));
			Assert.IsFalse(d2.ContainsKey(923));
			Assert.IsFalse(d2i.ContainsKey(6124));
			Assert.IsFalse(d2i2.ContainsKey(353));
		}

		[Test]
		public void TryGetValueWorks()
		{
			var d = new MyReadOnlyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var di = (IReadOnlyDictionary<int, string>)d;
			var d2 = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" } });
			var d2i = (IReadOnlyDictionary<int, string>)d2;
			var d2i2 = (IDictionary<int, string>)d2;

			string outVal;
			Assert.IsTrue(d.TryGetValue(6, out outVal));
			Assert.AreEqual(outVal, "z");
			Assert.IsTrue(di.TryGetValue(3, out outVal));
			Assert.AreEqual(outVal, "b");
			Assert.IsTrue(d2.TryGetValue(9, out outVal));
			Assert.AreEqual(outVal, "x");
			Assert.IsTrue(d2i.TryGetValue(6, out outVal));
			Assert.AreEqual(outVal, "z");
			Assert.IsTrue(d2i2.TryGetValue(3, out outVal));
			Assert.AreEqual(outVal, "b");

			outVal = "!!!";
			Assert.IsFalse(d.TryGetValue(6123, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(di.TryGetValue(32, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(d2.TryGetValue(923, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(d2i.TryGetValue(6124, out outVal));
			Assert.AreEqual(outVal, null);
			outVal = "!!!";
			Assert.IsFalse(d2i2.TryGetValue(353, out outVal));
			Assert.AreEqual(outVal, null);
		}

		[Test]
		public void AddWorks()
		{
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

			try
			{
				d.Add(5, "zz");
				Assert.IsTrue(false);
			}
			catch (Exception)
			{ }

			try
			{
				d.Add(new KeyValuePair<int, string>(1, "zz"));
				Assert.IsTrue(false);
			}
			catch (Exception)
			{ }
		}

		[Test]
		public void ClearWorks()
		{
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
		public void RemoveWorks()
		{
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" }, {13, "y"} });
			var di = (IDictionary<int, string>)d;
			var di2 = (ICollection<KeyValuePair<int, string>>)d;

			Assert.AreStrictEqual(d.Remove(6), true);
			Assert.AreEqual(d.Count, 3);
			Assert.IsFalse(d.ContainsKey(6));

			Assert.AreStrictEqual(di.Remove(3), true);
			Assert.AreEqual(di.Count, 2);
			Assert.IsFalse(di.ContainsKey(3));

			Assert.AreStrictEqual(di2.Remove(new KeyValuePair<int,string>(9, "x")), true);
			Assert.AreEqual(di2.Count, 1);
			Assert.IsFalse(di2.Contains(new KeyValuePair<int, string>(9, "x")));
			Assert.IsFalse(di.ContainsKey(9));

			Assert.AreStrictEqual(di2.Remove(new KeyValuePair<int, string>(13, "xxx")), false);
			Assert.AreStrictEqual(d.Remove(20), false);

			Assert.IsTrue(di.ContainsKey(13));
		}

		[Test]
		public void SetItemWorks()
		{
			var d = new MyDictionary(new Dictionary<int, string> { { 3, "b" }, { 6, "z" }, { 9, "x" }, { 13, "y" } });
			var di = (IDictionary<int, string>)d;

			d[3] = "check";
			Assert.AreEqual(d[3], "check");
			Assert.IsTrue(d.Contains(new KeyValuePair<int, string>(3, "check")));

			di[10] = "stuff";
			Assert.AreEqual(di[10], "stuff");
			Assert.IsTrue(di.ContainsKey(10));
			Assert.IsTrue(di.Contains(new KeyValuePair<int, string>(10, "stuff")));
		}
	}
}
