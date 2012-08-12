using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
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
