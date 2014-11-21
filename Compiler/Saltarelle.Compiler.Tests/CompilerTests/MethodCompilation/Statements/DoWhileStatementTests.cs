using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class DoWhileStatementTests : MethodCompilerTestBase {
		[Test]
		public void DoWhileStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	do {
		int x = 0;
	} while (true);
	// END
}",
@"	do {
		// @(4, 3) - (4, 13)
		var $x = 0;
		// @(5, 4) - (5, 17)
	} while (true);
", addSourceLocations: true);
		}

		[Test]
		public void DoWhileStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	do {
		int x = 0;
	} while ((SomeProperty = 1) < 0);
	// END
}",
@"	do {
		// @(5, 3) - (5, 13)
		var $x = 0;
		// @(6, 4) - (6, 35)
		this.set_$SomeProperty(1);
	} while (1 < 0);
", addSourceLocations: true);
		}
	}
}
