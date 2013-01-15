using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class ObjectTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue(new object() is object);
			Assert.AreEqual(typeof(object).FullName, "Object");
			Assert.IsTrue(typeof(object).IsClass);
		}

		[Test]
		public void CanGetHashCodeForObject() {
			var o = new object();
			int c = o.GetHashCode();
			Assert.IsTrue((object)c is int);
		}

		[Test]
		public void RepeatedCallsToGetHashCodeReturnsSameValue() {
			var o = new object();
			Assert.AreEqual(o.GetHashCode(), o.GetHashCode());
		}

		[Test]
		public void ObjectIsEqualToItself() {
			var o = new object();
			Assert.IsTrue(o.Equals(o));
		}

		[Test]
		public void ObjectIsNotEqualToOtherObject() {
			Assert.IsFalse(new object().Equals(new object()));
		}

		[Test]
		public void StaticEqualsWorks() {
			object o1 = new object(), o2 = new object();
			Assert.IsTrue(Equals(null, null));
			Assert.IsFalse(Equals(null, o1));
			Assert.IsFalse(Equals(o1, null));
			Assert.IsTrue(Equals(o1, o1));
			Assert.IsFalse(Equals(o1, o2));
		}
	}
}
