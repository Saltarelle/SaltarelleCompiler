using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class RuntimeHelpersTests {
		class C {
			public override int GetHashCode() {
				return 0;
			}
		}

		[Test]
		public void GetHashCodeWoksForObject() {
			object o1 = new object(), o2 = new object();
			Assert.AreEqual(o1.GetHashCode(), RuntimeHelpers.GetHashCode(o1));
			Assert.AreEqual(RuntimeHelpers.GetHashCode(o2), o2.GetHashCode());
		}

		[Test]
		public void GetHashCodeCallsGetHashCodeNonVirtually() {
			bool isOK = false;
			for (int i = 0; i < 3; i++) {
				// Since we might be unlucky and roll a 0 hash code, try 3 times.
				var c = new C();
				if (RuntimeHelpers.GetHashCode(c) != 0) {
					isOK = true;
					break;
				}
			}
			Assert.IsTrue(isOK, "GetHashCode should be invoked non-virtually");
		}
	}
}
