using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ConditionalTests : MethodCompilerTestBase{
		[Test]
		public void SimpleConditionalStatementWorks() {
			AssertCorrect(
@"public void M() {
	bool b = false;
	int x = 0, y = 0;
	// BEGIN
	var t = b ? x : y;
	// END
}",
@"	var $t = ($b ? $x : $y);
");
		}

		[Test]
		public void ConditionalStatementWithPropertySetterInConditionWorks() {
			AssertCorrect(
@"public bool P { get; set; }
public void M() {
	bool b = false;
	int x = 0, y = 0;
	// BEGIN
	var t = (P = b) ? x : y;
	// END
}",
@"	this.set_$P($b);
	var $t = ($b ? $x : $y);
");
		}

		[Test]
		public void ConditionalStatementWithPropertySetterInTruePathWorks() {
			AssertCorrect(
@"public int P { get; set; }
public void M() {
	bool b = false;
	int x = 0, y = 0;
	// BEGIN
	var t = b ? (P = x) : y;
	// END
}",
@"	var $tmp1;
	if ($b) {
		this.set_$P($x);
		$tmp1 = $x;
	}
	else {
		$tmp1 = $y;
	}
	var $t = $tmp1;
");
		}

		[Test]
		public void ConditionalStatementWithPropertySetterInFalsePathWorks() {
			AssertCorrect(
@"public int P { get; set; }
public void M() {
	bool b = false;
	int x = 0, y = 0;
	// BEGIN
	var t = b ? x : (P = y);
	// END
}",
@"	var $tmp1;
	if ($b) {
		$tmp1 = $x;
	}
	else {
		this.set_$P($y);
		$tmp1 = $y;
	}
	var $t = $tmp1;
");
		}

		[Test]
		public void ConditionalStatementWithPropertySetterInBothPathsWorks() {
			AssertCorrect(
@"public bool P1 { get; set; }
public int P2 { get; set; }
public int P3 { get; set; }
public void M() {
	bool b = false;
	int x = 0, y = 0;
	// BEGIN
	var t = (P1 = b) ? (P2 = x) : (P3 = y);
	// END
}",
@"	this.set_$P1($b);
	var $tmp1;
	if ($b) {
		this.set_$P2($x);
		$tmp1 = $x;
	}
	else {
		this.set_$P3($y);
		$tmp1 = $y;
	}
	var $t = $tmp1;
");
		}
	}
}
