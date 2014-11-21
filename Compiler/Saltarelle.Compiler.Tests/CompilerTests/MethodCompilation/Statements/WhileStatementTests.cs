using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class WhileStatementTests : MethodCompilerTestBase {
		[Test]
		public void WhileStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	while (true) {
		int x = 0;
	}
	// END
}",
@"	// @(3, 2) - (3, 14)
	while (true) {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void WhileStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	while ((SomeProperty = 1) < 0) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 32)
	while (true) {
		this.set_$SomeProperty(1);
		if (!(1 < 0)) {
			break;
		}
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}
	}
}
