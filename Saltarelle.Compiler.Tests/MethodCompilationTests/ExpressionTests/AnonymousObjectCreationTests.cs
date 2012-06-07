using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class AnonymousObjectCreationTests : MethodCompilerTestBase {
		[Test, Ignore("WIP")]
		public void CreatingASimpleAnonymousObjectWorks() {
			AssertCorrect(
@"public void M() {
	var o = new { i = 1, s = ""X"" };
}",
@"	var o = { i: 1, s: ""X"" };
");
		}
	}
}
