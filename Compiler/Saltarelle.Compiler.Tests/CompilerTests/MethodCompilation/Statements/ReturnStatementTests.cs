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
@"	return;
");
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
@"	return $x;
");
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
@"	return $Clone($x, {to_Int32});
", mutableValueTypes: true);
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
@"	this.set_$SomeProperty(1);
	return 1;
");
		}
	}
}
