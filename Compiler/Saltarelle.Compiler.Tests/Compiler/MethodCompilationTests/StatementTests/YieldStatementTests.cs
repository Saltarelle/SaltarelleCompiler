using NUnit.Framework;
using MC = Saltarelle.Compiler.Compiler.MethodCompiler;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class YieldStatementTests : MethodCompilerTestBase {
		[Test]
		public void YieldReturnWithoutAdditionalStatementsWorks() {
			try {
				MC.DisableStateMachineRewriteTestingUseOnly = true;
				
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
				MC.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void YieldReturnWithAdditionalStatementsWorks() {
			try {
				MC.DisableStateMachineRewriteTestingUseOnly = true;

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
				MC.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void YieldBreakWorks() {
			try {
				MC.DisableStateMachineRewriteTestingUseOnly = true;

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
				MC.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
