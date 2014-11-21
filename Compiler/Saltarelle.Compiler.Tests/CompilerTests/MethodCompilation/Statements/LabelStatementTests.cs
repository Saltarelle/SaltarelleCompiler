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
	// @(4, 2) - (4, 12)
	var $i = 0;
", addSourceLocations: true);
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
@"	// @(4, 2) - (4, 12)
	switch ($k) {
		case 1: {
			// @(6, 4) - (6, 10)
			$x = 1;
			// @(7, 4) - (7, 10)
			break;
		}
		case 2: {
			// @(10, 4) - (10, 10)
			$x = 2;
			forward:
			// @(12, 4) - (12, 10)
			$x = 3;
			// @(13, 4) - (13, 10)
			break;
		}
		case 3: {
			// @(16, 4) - (16, 10)
			$x = 4;
			// @(17, 4) - (17, 17)
			goto forward;
			// @(18, 4) - (18, 10)
			break;
		}
	}
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
