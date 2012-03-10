using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class BlockStatementTests : MethodCompilerTestBase {
		[Test]
		public void BlockStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	{
		int i = 0;
		int j = 1;
	}
	// END
}",
@"	{
		var $i = 0;
		var $j = 1;
	}
");
		}
	}
}
