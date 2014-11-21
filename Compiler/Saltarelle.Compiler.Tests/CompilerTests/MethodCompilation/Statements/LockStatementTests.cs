using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class LockStatementTests : MethodCompilerTestBase {
		[Test]
		public void LockStatementEvaluatesArgumentThatDoesNotRequireExtraStatementsAndActsAsABlockStatement() {
			AssertCorrect(
@"public object SomeProperty { get; set; }
public object Method(object o) { return null; }
public void M() {
	object o = null;
	// BEGIN
	lock (Method(SomeProperty = o)) {
		int x = 0;
	}
	// END
}",
@"	// @(6, 2) - (6, 33)
	this.set_$SomeProperty($o);
	this.$Method($o);
	{
		// @(7, 3) - (7, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void LockStatementEvaluatesArgumentThatDoesRequireExtraStatementsAndActsAsABlockStatement() {
			AssertCorrect(
@"public object P1 { get; set; }
public object P2 { get; set; }
public void M() {
	object o = null;
	// BEGIN
	lock (P1 = P2 = o) {
		int x = 0;
	}
	// END
}",
@"	// @(6, 2) - (6, 20)
	this.set_$P2($o);
	this.set_$P1($o);
	{
		// @(7, 3) - (7, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}
	}
}
