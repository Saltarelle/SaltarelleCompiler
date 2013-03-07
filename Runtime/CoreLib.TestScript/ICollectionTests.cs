﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ICollectionTests {
		private class MyCollection : ICollection<string> {
			public MyCollection(string[] items) {
				Items = new List<string>(items);
			}

			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public List<string> Items { get; private set; }

			public IEnumerator<string> GetEnumerator() { return Items.GetEnumerator(); }
			public int Count { get { return Items.Count; } }
			public void Add(string item) { Items.Add(item); }
			public void Clear() { Items.Clear(); }
			public bool Contains(string item) { return Items.Contains(item); }
			public bool Remove(string item) { return Items.Remove(item); }
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
		public void ArrayImplementsICollection() {
			Assert.IsTrue((object)new int[1] is ICollection<int>);
		}

		//[Test]
		//public void ArrayImplementsIReadOnlyCollection()
		//{
		//	Assert.IsTrue((object)new int[1] is IReadOnlyCollection<int>);
		//}

		[Test]
		public void CustomClassThatShouldImplementICollectionDoesSo() {
			Assert.IsTrue((object)new MyCollection(new string[0]) is ICollection<string>);
		}

		[Test]
		public void CustomClassThatShouldImplementIReadOnlyCollectionDoesSo()
		{
			Assert.IsTrue((object)new MyCollection(new string[0]) is IReadOnlyCollection<string>);
		}

		[Test]
		public void ArrayCastToICollectionCountWorks()
		{
			Assert.AreEqual(((ICollection<string>)new[] { "x", "y", "z" }).Count, 3);
		}

		//[Test]
		//public void ArrayCastToIReadOnlyCollectionCountWorks()
		//{
		//	Assert.AreEqual(((IReadOnlyCollection<string>)new[] { "x", "y", "z" }).Count, 3);
		//}

		[Test]
		public void ClassImplementingICollectionCountWorks() {
			Assert.AreEqual(new MyCollection(new[] { "x", "y" }).Count, 2);
		}

		[Test]
		public void ClassImplementingICollectionCastToICollectionCountWorks()
		{
			Assert.AreEqual(((ICollection<string>)new MyCollection(new[] { "x", "y", "z" })).Count, 3);
		}

		[Test]
		public void ClassImplementingICollectionCastToIReadOnlyCollectionCountWorks()
		{
			Assert.AreEqual(((IReadOnlyCollection<string>)new MyCollection(new[] { "x", "y", "z" })).Count, 3);
		}

		[Test]
		public void ClassImplementingICollectionAddWorks() {
			MyCollection c = new MyCollection(new[] { "x", "y" });
			c.Add("z");
			Assert.AreEqual(c.Count, 3);
			Assert.IsTrue(c.Contains("z"));
		}

		[Test]
		public void ClassImplementingICollectionCastToICollectionAddWorks() {
			ICollection<string> c = new MyCollection(new[] { "x", "y" });
			c.Add("z");
			Assert.AreEqual(c.Count, 3);
			Assert.IsTrue(c.Contains("z"));
		}

		[Test]
		public void ClassImplementingICollectionClearWorks() {
			MyCollection c = new MyCollection(new[] { "x", "y" });
			c.Clear();
			Assert.AreEqual(c.Count, 0);
		}

		[Test]
		public void ClassImplementingICollectionCastToICollectionClearWorks() {
			ICollection<string> c = new MyCollection(new[] { "x", "y" });
			c.Clear();
			Assert.AreEqual(c.Count, 0);
		}

		[Test]
		public void ArrayCastToICollectionContainsWorks() {
			ICollection<C> arr = new[] { new C(1), new C(2), new C(3) };
			Assert.IsTrue(arr.Contains(new C(2)));
			Assert.IsFalse(arr.Contains(new C(4)));
		}

		//[Test]
		//public void ArrayCastToIReadOnlyCollectionContainsWorks()
		//{
		//	IReadOnlyCollection<C> arr = new[] { new C(1), new C(2), new C(3) };
		//	Assert.IsTrue(arr.Contains(new C(2)));
		//	Assert.IsFalse(arr.Contains(new C(4)));
		//}

		[Test]
		public void ClassImplementingICollectionContainsWorks() {
			MyCollection c = new MyCollection(new[] { "x", "y" });
			Assert.IsTrue(c.Contains("x"));
			Assert.IsFalse(c.Contains("z"));
		}

		[Test]
		public void ClassImplementingICollectionCastToICollectionContainsWorks() {
			ICollection<string> c = new MyCollection(new[] { "x", "y" });
			Assert.IsTrue(c.Contains("x"));
			Assert.IsFalse(c.Contains("z"));
		}

		[Test]
		public void ClassImplementingICollectionCastToIReadOnlyCollectionContainsWorks()
		{
			ICollection<string> c = new MyCollection(new[] { "x", "y" });
			Assert.IsTrue(c.Contains("x"));
			Assert.IsFalse(c.Contains("z"));
		}

		[Test]
		public void ClassImplementingICollectionRemoveWorks() {
			MyCollection c = new MyCollection(new[] { "x", "y" });
			c.Remove("x");
			Assert.AreEqual(c.Count, 1);
			c.Remove("y");
			Assert.AreEqual(c.Count, 0);
		}

		[Test]
		public void ClassImplementingICollectionCastToICollectionRemoveWorks() {
			ICollection<string> c = new MyCollection(new[] { "x", "y" });
			c.Remove("x");
			Assert.AreEqual(c.Count, 1);
			c.Remove("y");
			Assert.AreEqual(c.Count, 0);
		}
	}
}
