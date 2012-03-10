using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class EmptyStatementTests : MethodCompilerTestBase {
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
