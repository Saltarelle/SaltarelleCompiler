using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	public class NumberTests : RuntimeLibraryTestBase {
		[Test]
		public void IntegerDivisionWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;
public class C {
	public static int[] M() {
		int a = 17, b = 4;
		return new[] { a / b, -a / b, a / -b, -a / -b };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { 17 / 4, -17 / 4, 17 / -4, -17 / -4 }));
		}

		[Test]
		public void DoublesAreTruncatedWhenConvertedToIntegers() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;
public class C {
	public static object[] M() {
		double d1 = 4.5;
		double? d2 = null;
		double? d3 = 8.5;
		return new object[] { (int)d1, (int)-d1, (int?)d2, (int)d3, (int)-d3, (int?)d3, (int?)-d3 };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object[] { 4, -4, null, 8, -8, 8, -8 }));
		}
	}
}
