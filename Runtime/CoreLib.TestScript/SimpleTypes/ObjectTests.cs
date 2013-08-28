using System;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class ObjectTests {
		private class C1 {
			public override string ToString() {
				return "test";
			}
		}
		private class C2 : C1 {}

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

		[Test]
		public void ReferenceEqualsWorks() {
			object o1 = new object(), o2 = new object(), n = null;
			Assert.IsTrue (ReferenceEquals(n, n));
			Assert.IsTrue (ReferenceEquals(n, Script.Undefined));
			Assert.IsFalse(ReferenceEquals(o1, o2));
			Assert.IsFalse(ReferenceEquals(o1, n));
			Assert.IsFalse(ReferenceEquals(o1, Script.Undefined));
			Assert.IsTrue (ReferenceEquals(o1, o1));
		}

		[Test]
		public void ToStringOverride() {
			object c1 = new C1(), c2 = new C2();
			Assert.AreEqual(c1.ToString(), "test", "#1");
			Assert.AreEqual(c2.ToString(), "test", "#1");
		}
	}
}
