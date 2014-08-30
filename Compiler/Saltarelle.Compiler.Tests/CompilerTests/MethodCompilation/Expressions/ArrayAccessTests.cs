using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ArrayAccessTests : MethodCompilerTestBase {
		[Test]
		public void SimpleArrayAccessWorks() {
			AssertCorrect(
@"void M() {
	var arr = new int[0];
	int i = 0;
	// BEGIN
	int x = arr[i];
	// END
}",
@"	var $x = $arr[$i];
");
		}

		[Test]
		public void ArrayAccessEvaluatesExpressionsInTheCorrectOrder() {
			AssertCorrect(
@"int P { get; set; }
int[] F() { return null; }
void M() {
	int i = 0;
	// BEGIN
	int x = F()[P = i];
	// END
}",
@"	var $tmp1 = this.$F();
	this.set_$P($i);
	var $x = $tmp1[$i];
");
		}

		[Test]
		public void IndexingArrayWithDynamicArgumentWorks() {
			AssertCorrect(
@"public void M() {
	int[] arr = null;
	dynamic d = null;
	// BEGIN
	var x = arr[d];
	// END
}",
@"	var $x = $arr[$FromNullable($Cast($d, {ct_Int32}))];
");
		}

		[Test]
		public void MultiDimensionalArrayAccessWorks() {
			AssertCorrect(
@"void M() {
	int[,] arr = null;
	int i = 0, j = 0;
	// BEGIN
	int x = arr[i, j];
	// END
}",
@"	var $x = $MultidimArrayGet($arr, $i, $j);
");
		}

		[Test]
		public void MultiDimensionalArrayAccessEvaluatesExpressionsInTheCorrectOrder() {
			AssertCorrect(
@"int P1 { get; set; }
int P2 { get; set; }
int[,] F() { return null; }
void M() {
	int i = 0, j = 0;
	// BEGIN
	int x = F()[P1 = i, P2 = j];
	// END
}",
@"	var $tmp1 = this.$F();
	this.set_$P1($i);
	this.set_$P2($j);
	var $x = $MultidimArrayGet($tmp1, $i, $j);
");
		}

		[Test, Category("Wait")]
		public void IndexingMultiDimensionalArrayWithDynamicArgumentWorks() {
			AssertCorrect(
@"public void M() {
	int[] arr = null;
	dynamic d1 = null, d2 = null;
	// BEGIN
	var x = arr[d1, d2];
	// END
}",
@"	var $x = $MultidimArrayGet($arr, $FromNullable($Cast($d1, {ct_Int32})), $FromNullable($Cast($d2, {ct_Int32})));
");
		}
	}
}
