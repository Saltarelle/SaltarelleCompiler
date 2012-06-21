using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
@"		continue;
");
		}

	}
}
