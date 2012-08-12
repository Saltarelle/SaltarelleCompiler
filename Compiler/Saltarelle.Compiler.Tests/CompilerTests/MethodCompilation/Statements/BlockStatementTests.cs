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
		var $i = 0;
		var $j = 1;
	}
");
		}
	}
}
