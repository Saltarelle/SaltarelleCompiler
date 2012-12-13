using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class IComparableTests {
		class MyComparable : IComparable<MyComparable> {
			public int result;
			public MyComparable other;

			public int CompareTo(MyComparable other) {
				this.other = other;
				return result;
			}
		}

		[Test]
		public void CallingMethodThroughIComparableInterfaceInvokesImplementingMethod() {
			MyComparable a = new MyComparable(), b = new MyComparable();
			a.result = 534;
			Assert.AreEqual(((IComparable<MyComparable>)a).CompareTo(b), 534);
			Assert.AreStrictEqual(a.other, b);

			a.result = -42;
			Assert.AreEqual(((IComparable<MyComparable>)a).CompareTo(null), -42);
			Assert.AreStrictEqual(a.other, null);

			a.result = -534;
			Assert.AreEqual(a.CompareTo(b), -534);
			Assert.AreStrictEqual(a.other, b);

			a.result = 42;
			Assert.AreEqual(a.CompareTo(null), 42);
			Assert.AreStrictEqual(a.other, null);
		}
	}
}
