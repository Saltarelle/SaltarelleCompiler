using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class LabelStatementTests : MethodCompilerTestBase {
		[Test]
		public void LabelWorks() {
			AssertCorrect(
@"Exception MyProperty { get; set; }
public void M() {
	// BEGIN
myLabel:
	int i = 0;
	// END
}",
@"	myLabel:
	var $i = 0;
");
		}
	}
}
