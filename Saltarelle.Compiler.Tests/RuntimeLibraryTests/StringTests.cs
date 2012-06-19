using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class StringTests : RuntimeLibraryTestBase {
		[Test]
		public void ConcatWorks() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static string[] M() {
		return new[] { string.Concat(""X""), string.Concat(""Xa"", ""Ya""), string.Concat(""Some"", ""thing"", "" else""), string.Concat(1, 2) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "X", "XaYa", "Something else", "12" }));
		}

		[Test]
		public void ConstructorsWork() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static string[] M() {
		string s = ""abcd"";
		return new[] { new string(), new string(s), new string('x', 3) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "", "abcd", "xxx" }));
		}

		[Test]
		public void ConcatOperatorWorks() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static string M() {
		return ""ab"" + 15 + 'x';
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("ab15x"));
		}
	}
}
