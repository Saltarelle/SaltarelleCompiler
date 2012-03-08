using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class EmptyStatementTests : StatementTestBase {
		[Test]
		public void EmptyStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	;
	// END
}",
@"	;
");
		}

	}
}
