using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class YieldStatementTests : MethodCompilerTestBase {
		[Test]
		public void YieldReturnWithoutAdditionalStatementsWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;
				
				AssertCorrect(
@"public System.Collections.Generic.IEnumerable<int> M() {
	int i = 1;
	// BEGIN
	yield return i;
	// END
}",
@"	// @(4, 2) - (4, 17)
	yield return $i;
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void YieldReturnWithoutAdditionalStatementsWorksStruct() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;
				
				AssertCorrect(
@"public System.Collections.Generic.IEnumerable<int> M() {
	int i = 1;
	// BEGIN
	yield return i;
	// END
}",
@"	// @(4, 2) - (4, 17)
	yield return $Clone($i, {to_Int32});
", mutableValueTypes: true, addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void YieldReturnWithAdditionalStatementsWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"int MyProperty { get; set; }
public System.Collections.Generic.IEnumerable<int> M() {
	int i = 1;
	// BEGIN
	yield return (MyProperty = i);
	// END
}",
@"	// @(5, 2) - (5, 32)
	this.set_$MyProperty($i);
	yield return $i;
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void YieldBreakWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"public System.Collections.Generic.IEnumerable<int> M() {
	// BEGIN
	yield break;
	// END
}",
@"	// @(3, 2) - (3, 14)
	yield break;
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
