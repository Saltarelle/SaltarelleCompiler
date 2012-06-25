using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class YieldStatementTests : MethodCompilerTestBase {
		[Test]
		public void YieldReturnWithoutAdditionalStatementsWorks() {
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

		[Test]
		public void YieldReturnWithAdditionalStatementsWorks() {
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

		[Test]
		public void YieldBreakWorks() {
			AssertCorrect(
@"public IEnumerable<int> M() {
	// BEGIN
	yield break;
	// END
}",
@"	yield break;
");
		}
	}
}
