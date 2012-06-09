using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
