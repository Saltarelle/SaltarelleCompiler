using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
		var $x = 0;
	} while (true);
");
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
		var $x = 0;
		this.set_$SomeProperty(1);
	} while (1 < 0);
");
		}
	}
}
