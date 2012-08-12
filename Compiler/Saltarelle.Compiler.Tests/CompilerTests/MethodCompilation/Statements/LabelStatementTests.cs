using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class LabelStatementTests : MethodCompilerTestBase {
		[Test]
		public void LabelWorks() {
			try {
				MethodCompiler.DisableStateMachineRewriteTestingUseOnly = true;

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
				MethodCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
