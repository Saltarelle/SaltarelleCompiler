using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class EqualityComparerTests {
		class MyClass {
			public int hashCode;
			public object other;
			public bool shouldEqual;

			public override int GetHashCode() {
				return hashCode;
			}

			public override bool Equals(object o) {
				other = o;
				return shouldEqual;
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(EqualityComparer<object>).FullName, "ss.EqualityComparer", "FullName should be correct");
			Assert.IsTrue(typeof(EqualityComparer<object>).IsClass, "IsClass should be true");
			Assert.AreStrictEqual(typeof(EqualityComparer<object>).BaseType, typeof(object), "BaseType should be correct");
			object dict = EqualityComparer<object>.Default;
			Assert.IsTrue(dict is EqualityComparer<object>, "is EqualityComparer<object> should be true");
			Assert.IsTrue(dict is IEqualityComparer<object>, "is IEqualityComparer<object> should be true");
		}

		[Test]
		public void DefaultComparerCanGetHashCodeOfNumber() {
			Assert.AreEqual(EqualityComparer<object>.Default.GetHashCode(12345), 12345.GetHashCode());
		}

		[Test]
		public void DefaultComparerReturnsZeroAsHashCodeForNullAndUndefined() {
			Assert.AreEqual(EqualityComparer<object>.Default.GetHashCode(null), 0);
			Assert.AreEqual(EqualityComparer<object>.Default.GetHashCode(Script.Undefined), 0);
		}

		[Test]
		public void DefaultComparerCanDetermineEquality() {
			object o1 = new object(), o2 = new object();
			Assert.IsTrue(EqualityComparer<object>.Default.Equals(null, null));
			Assert.IsFalse(EqualityComparer<object>.Default.Equals(null, o1));
			Assert.IsFalse(EqualityComparer<object>.Default.Equals(o1, null));
			Assert.IsTrue(EqualityComparer<object>.Default.Equals(o1, o1));
			Assert.IsFalse(EqualityComparer<object>.Default.Equals(o1, o2));
		}

		[Test]
		public void DefaultComparerInvokesOverriddenGetHashCode() {
			Assert.AreEqual(EqualityComparer<object>.Default.GetHashCode(new MyClass { hashCode = 42158 }), 42158);
		}

		[Test]
		public void DefaultComparerInvokesOverriddenEquals() {
			var c = new MyClass();
			var other = new MyClass();
			c.shouldEqual = false;
			Assert.IsFalse(EqualityComparer<object>.Default.Equals(c, other));
			Assert.AreStrictEqual(c.other, other);

			c.shouldEqual = true;
			c.other = null;
			Assert.IsTrue(EqualityComparer<object>.Default.Equals(c, other));
			Assert.AreStrictEqual(c.other, other);

			c.shouldEqual = true;
			c.other = other;
			Assert.IsFalse(EqualityComparer<object>.Default.Equals(c, null)); // We should not invoke our own equals so its return value does not matter.
			Assert.AreEqual(c.other, other); // We should not invoke our own equals so the 'other' member should not be set.
		}
	}
}
