using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class BlockStatementTests : MethodCompilerTestBase {
		[Test]
		public void BlockStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	{
		int i = 0;
		int j = 1;
	}
	// END
}",
@"	{
		// @(4, 3) - (4, 13)
		var $i = 0;
		// @(5, 3) - (5, 13)
		var $j = 1;
	}
", addSourceLocations: true);
		}
	}
}
