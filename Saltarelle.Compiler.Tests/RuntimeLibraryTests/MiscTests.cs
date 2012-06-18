using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class MiscTests : RuntimeLibraryTestBase {
		[Test]
		public void CoalesceWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static object[] M() {
		int? v1 = null, v2 = 1, v3 = 0, v4 = 2;
		string s1 = null, s2 = ""x"";
		return new object[] { v1 ?? v1, v1 ?? v2, v3 ?? v4, s1 ?? s1, s1 ?? s2 };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object [] { null, 1, 0, null, "x" }));
		}
	}
}
