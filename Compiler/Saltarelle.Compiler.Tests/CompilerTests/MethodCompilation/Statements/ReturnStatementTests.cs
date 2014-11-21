using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class ReturnStatementTests : MethodCompilerTestBase {
		[Test]
		public void ReturnVoidStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	return;
	// END
}",
@"	// @(3, 2) - (3, 9)
	return;
", addSourceLocations: true);
		}

		[Test]
		public void ReturnExpressionStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public int M() {
	int x = 0;
	// BEGIN
	return x;
	// END
}",
@"	// @(4, 2) - (4, 11)
	return $x;
", addSourceLocations: true);
		}

		[Test]
		public void ReturnExpressionStatementWithExpressionWithoutExtraStatementsWorksStruct() {
			AssertCorrect(
@"public int M() {
	int x = 0;
	// BEGIN
	return x;
	// END
}",
@"	// @(4, 2) - (4, 11)
	return $Clone($x, {to_Int32});
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void ReturnExpressionStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public int M() {
	// BEGIN
	return (SomeProperty = 1);
	// END
}",
@"	// @(4, 2) - (4, 28)
	this.set_$SomeProperty(1);
	return 1;
", addSourceLocations: true);
		}
	}
}
