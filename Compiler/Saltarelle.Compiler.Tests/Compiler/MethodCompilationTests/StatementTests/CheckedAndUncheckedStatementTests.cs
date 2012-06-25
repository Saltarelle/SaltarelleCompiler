using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class CheckedAndUncheckedStatementTests : MethodCompilerTestBase {
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
