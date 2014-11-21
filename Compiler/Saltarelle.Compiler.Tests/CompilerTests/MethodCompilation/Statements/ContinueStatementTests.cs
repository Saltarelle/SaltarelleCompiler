using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class ContinueStatementTests : MethodCompilerTestBase {
		[Test]
		public void ContinueStatementWorks() {
			AssertCorrect(
@"public void M() {
	for (int i = 0; i < 10; i++) {
		// BEGIN
		continue;
		// END
	}
}",
@"		// @(4, 3) - (4, 12)
		continue;
", addSourceLocations: true);
		}

	}
}
