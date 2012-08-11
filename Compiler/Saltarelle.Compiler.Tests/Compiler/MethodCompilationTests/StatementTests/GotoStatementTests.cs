using NUnit.Framework;
using MC = Saltarelle.Compiler.Compiler.MethodCompiler;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class GotoStatementTests : MethodCompilerTestBase {
		[Test]
		public void GotoWorks() {
			try {
				MC.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"Exception MyProperty { get; set; }
public void M() {
myLabel:
	int i = 0;
	// BEGIN
	goto myLabel;
	// END
}",
@"	goto myLabel;
");
			}
			finally {
				MC.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
