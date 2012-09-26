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
@"public IEnumerable<int> M() {
	int i = 1;
	// BEGIN
	yield return i;
	// END
}",
@"	yield return $i;
");
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
public IEnumerable<int> M() {
	int i = 1;
	// BEGIN
	yield return (MyProperty = i);
	// END
}",
@"	this.set_$MyProperty($i);
	yield return $i;
");
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
@"public IEnumerable<int> M() {
	// BEGIN
	yield break;
	// END
}",
@"	yield break;
");
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
