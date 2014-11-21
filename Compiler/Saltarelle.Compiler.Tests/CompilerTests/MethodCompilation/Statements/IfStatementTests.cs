using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class IfStatementTests : MethodCompilerTestBase {
		[Test]
		public void IfStatementWithoutElseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	if (true) {
		int x = 0;
	}
	// END
}",
@"	// @(3, 2) - (3, 11)
	if (true) {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void IfStatementWithElseWorks() {
			AssertCorrect(
@"public void M() {
	int x;
	// BEGIN
	if (true) {
		x = 0;
	}
	else {
		x = 1;
	}
	// END
}",
@"	// @(4, 2) - (4, 11)
	if (true) {
		// @(5, 3) - (5, 9)
		$x = 0;
	}
	else {
		// @(8, 3) - (8, 9)
		$x = 1;
	}
", addSourceLocations: true);
		}

		[Test]
		public void IfStatementWithConditionThatRequiresExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	if ((SomeProperty = 1) < 0) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 29)
	this.set_$SomeProperty(1);
	if (1 < 0) {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}
	}
}
