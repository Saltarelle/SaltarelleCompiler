using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class AnonymousObjectCreationTests : MethodCompilerTestBase {
		[Test]
		public void CreatingASimpleAnonymousObjectWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var o = new { i = 1, s = ""X"" };
	// END
}",
@"	var $o = { $i: 1, $s: 'X' };
");
		}

		[Test]
		public void ExpressionsAreEvaulatedInTheCorrectOrderWhenTemporariesAreRequired() {
			AssertCorrect(
@"
string F1() { return null; }
string F2() { return null; }
int P { get; set; }

public void M() {
	// BEGIN
	var o = new { a = F1(), b = (P = 10), c = F2() };
	// END
}",
@"	var $tmp1 = this.$F1();
	this.set_$P(10);
	var $o = { $a: $tmp1, $b: 10, $c: this.$F2() };
");
		}

		[Test]
		public void NestingObjectInitializersWorks() {
			AssertCorrect(
@"
public void M() {
	// BEGIN
	var o = new { a = 0, b = 1, c = new { d = 2, e = 3, f = 4 } };
	// END
}",
@"	var $o = { $a: 0, $b: 1, $c: { $d: 2, $e: 3, $f: 4 } };
");
		}

		[Test]
		public void NestingObjectInitializersEvaluatesExpressionsInTheCorrectOrder() {
			AssertCorrect(
@"
string F1() { return null; }
string F2() { return null; }
string F3() { return null; }
string F4() { return null; }
string F5() { return null; }
int P { get; set; }

public void M() {
	// BEGIN
	var o = new { a = F1(), b = F2(), c = new { d = F3(), e = (P = 10), f = F4() } };
	// END
}",
@"	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp1 = this.$F3();
	this.set_$P(10);
	var $o = { $a: $tmp2, $b: $tmp3, $c: { $d: $tmp1, $e: 10, $f: this.$F4() } };
");
		}
	}
}
