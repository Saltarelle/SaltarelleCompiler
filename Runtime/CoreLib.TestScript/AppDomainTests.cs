using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class AppDomainTests {
		[Test]
		public void GetAssembliesWorks() {
			var arr = AppDomain.CurrentDomain.GetAssemblies();
			Assert.AreEqual(arr.Length, 2);
			Assert.IsTrue(arr.Contains(typeof(int).Assembly), "#1");
			Assert.IsTrue(arr.Contains(typeof(AppDomainTests).Assembly), "#2");
		}
	}
}
