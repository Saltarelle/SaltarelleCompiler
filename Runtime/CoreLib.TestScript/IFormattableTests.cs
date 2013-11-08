using System;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class IFormattableTests {
		class MyFormattable : IFormattable {
			public string ToString(string format) {
				return format + " success";
			}
		}

		[Test]
		public void IFormattableIsRecordedInInterfaceList() {
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(MyFormattable)));
			Assert.IsTrue((object)new MyFormattable() is IFormattable);
		}

		[Test]
		public void CallingMethodThroughIFormattableInterfaceInvokesImplementingMethod() {
			Assert.AreEqual(new MyFormattable().ToString("real"), "real success", "Non-interface call should succeed");
			Assert.AreEqual(((IFormattable)new MyFormattable()).ToString("real"), "real success", "Interface call should succeed");
		}
	}
}
