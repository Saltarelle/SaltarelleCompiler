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
@"	if (true) {
		var $x = 0;
	}
");
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
@"	if (true) {
		$x = 0;
	}
	else {
		$x = 1;
	}
");
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
@"	this.set_$SomeProperty(1);
	if (1 < 0) {
		var $x = 0;
	}
");
		}
	}
}
