using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class LazyTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Lazy<int>).FullName, "ss.Lazy");
			Assert.IsTrue(typeof(Lazy<int>).IsClass);
			object s = new Lazy<int>();
			Assert.IsTrue(s is Lazy<int>);
		}

		[Test]
		public void WorksWithoutValueFactory() {
			var l = new Lazy<int>();
			Assert.IsFalse(l.IsValueCreated);
			Assert.AreEqual(l.Value, 0);
			Assert.IsTrue(l.IsValueCreated);
			Assert.AreEqual(l.Value, 0);
		}

		[Test]
		public void WorksWithoutValueFactoryWithBooleanConstructor() {
			var l = new Lazy<int>(false);
			Assert.IsFalse(l.IsValueCreated);
			Assert.AreEqual(l.Value, 0);
			Assert.IsTrue(l.IsValueCreated);
			Assert.AreEqual(l.Value, 0);
		}

		[Test]
		public void WorksWithValueFactory() {
			var o = new object();
			int numCalls = 0;
			var l = new Lazy<object>(() => { numCalls++; return o; });
			Assert.IsFalse(l.IsValueCreated);
			Assert.AreStrictEqual(l.Value, o);
			Assert.AreEqual(numCalls, 1);
			Assert.IsTrue(l.IsValueCreated);
			Assert.AreStrictEqual(l.Value, o);
			Assert.AreEqual(numCalls, 1);
		}

		[Test]
		public void WorksWithValueFactoryAndBooleanConstructor() {
			var o = new object();
			int numCalls = 0;
			var l = new Lazy<object>(() => { numCalls++; return o; }, true);
			Assert.IsFalse(l.IsValueCreated);
			Assert.AreStrictEqual(l.Value, o);
			Assert.AreEqual(numCalls, 1);
			Assert.IsTrue(l.IsValueCreated);
			Assert.AreStrictEqual(l.Value, o);
			Assert.AreEqual(numCalls, 1);
		}
	}
}
