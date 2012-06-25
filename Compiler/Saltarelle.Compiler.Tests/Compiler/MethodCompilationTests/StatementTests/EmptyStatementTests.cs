using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
@"	;
");
		}

	}
}
