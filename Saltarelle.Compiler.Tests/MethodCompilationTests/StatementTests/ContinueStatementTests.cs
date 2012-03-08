using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class ContinueStatementTests : StatementTestBase {
		[Test]
		public void ContinueStatementWorks() {
			AssertCorrect(
@"public void M() {
	for (int i = 0; i < 10; i++) {
		// BEGIN
		continue;
		// END
	}
}",
@"		continue;
");
		}

	}
}
