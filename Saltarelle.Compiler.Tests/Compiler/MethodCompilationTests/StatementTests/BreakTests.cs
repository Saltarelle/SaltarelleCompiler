using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class BreakTests : MethodCompilerTestBase {
		[Test]
		public void BreakStatementWorks() {
			AssertCorrect(
@"public void M() {
	for (int i = 0; i < 10; i++) {
		// BEGIN
		break;
		// END
	}
}",
@"		break;
");
		}
	}
}
