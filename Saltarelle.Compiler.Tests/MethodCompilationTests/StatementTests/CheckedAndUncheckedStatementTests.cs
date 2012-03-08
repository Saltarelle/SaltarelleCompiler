using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class CheckedAndUncheckedStatementTests : StatementTestBase {
		[Test]
		public void CheckedStatementActsAsABlockStatement() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	checked {
		int x = 0;
	}
	// END
}",
@"	{
		var $x = 0;
	}
");
		}

		[Test]
		public void UncheckedStatementActsAsABlockStatement() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	unchecked {
		int x = 0;
	}
	// END
}",
@"	{
		var $x = 0;
	}
");
		}
	}
}
