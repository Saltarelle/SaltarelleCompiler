using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
@"	while (true) {
		var $x = 0;
	}
");
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
@"	while (true) {
		this.set_$SomeProperty(1);
		if (!(1 < 0)) {
			break;
		}
		var $x = 0;
	}
");
		}
	}
}
