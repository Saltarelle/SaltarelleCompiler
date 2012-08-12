using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class GotoStatementTests : MethodCompilerTestBase {
		[Test]
		public void GotoWorks() {
			try {
				MethodCompiler.DisableStateMachineRewriteTestingUseOnly = true;

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
				MethodCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
