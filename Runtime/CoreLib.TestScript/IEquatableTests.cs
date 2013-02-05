using System;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class IEquatableTests {
		class MyEquatable : IEquatable<MyEquatable> {
			public bool result;
			public MyEquatable other;

			public bool Equals(MyEquatable other) {
				this.other = other;
				return result;
			}
		}

		[Test]
		public void CallingMethodThroughIComparableInterfaceInvokesImplementingMethod() {
			MyEquatable a = new MyEquatable(), b = new MyEquatable();
			a.result = true;
			Assert.IsTrue(((IEquatable<MyEquatable>)a).Equals(b));
			Assert.AreStrictEqual(a.other, b);
			a.result = false;
			Assert.IsFalse(((IEquatable<MyEquatable>)a).Equals(b));

			a.result = true;
			Assert.IsTrue(((IEquatable<MyEquatable>)a).Equals(null));
			Assert.AreStrictEqual(a.other, null);
			a.result = false;
			Assert.IsFalse(((IEquatable<MyEquatable>)a).Equals(null));

			a.result = true;
			Assert.IsTrue(a.Equals(b));
			Assert.AreStrictEqual(a.other, b);
			a.result = false;
			Assert.IsFalse(a.Equals(b));

			a.result = true;
			Assert.IsTrue(a.Equals(null));
			Assert.AreStrictEqual(a.other, null);
			a.result = false;
			Assert.IsFalse(a.Equals(null));
		}
	}
}
