using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class LabelStatementTests : MethodCompilerTestBase {
		[Test]
		public void LabelWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"public void M() {
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
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void LabelInsideSwitchWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"public void M() {
	int k = 0, x = 0;
	// BEGIN
	switch (k) {
		case 1:
			x = 1;
			break;
	
		case 2:
			x = 2;
		forward:
			x = 3;
			break;
	
		case 3:
			x = 4;
			goto forward;
			break;
	}
	// END
}",
@"	switch ($k) {
		case 1: {
			$x = 1;
			break;
		}
		case 2: {
			$x = 2;
			forward:
			$x = 3;
			break;
		}
		case 3: {
			$x = 4;
			goto forward;
			break;
		}
	}
");
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
