using NUnit.Framework;
using MC = Saltarelle.Compiler.Compiler.MethodCompiler;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class LabelStatementTests : MethodCompilerTestBase {
		[Test]
		public void LabelWorks() {
			try {
				MC.DisablePostProcessingTestingUseOnly = true;

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
			finally {
				MC.DisablePostProcessingTestingUseOnly = false;
			}
		}
	}
}
