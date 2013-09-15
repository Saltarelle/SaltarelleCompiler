using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class EnvironmentTests {
		[Test]
		public void NewLineIsAStringContainingOnlyTheNewLineChar() {
			Assert.AreEqual(Environment.NewLine, "\n");
		}
	}
}
