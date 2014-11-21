using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class EmptyStatementTests : MethodCompilerTestBase {
		[Test]
		public void EmptyStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	;
	// END
}",
@"	// @(3, 2) - (3, 3)
	;
", addSourceLocations: true);
		}

	}
}
