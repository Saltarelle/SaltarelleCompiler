using System;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class EnvironmentTests {
		[Test]
		public void TickCountWorks() {
			Assert.AreEqual(Environment.TickCount, DateTime.Now.GetTime() * 10000);
		}
	}
}
