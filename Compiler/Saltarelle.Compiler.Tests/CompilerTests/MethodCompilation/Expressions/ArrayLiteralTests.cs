using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ArrayLiteralTests : MethodCompilerTestBase {
		[Test]
		public void ArrayWithSizeZeroCanBeCompiled() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var arr = new int[0];
	// END
}",
@"	var $arr = [];
");
		}

		[Test]
		public void ArrayWithEmptyInitializerCanBeCompiled() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var arr = new int[] {};
	// END
}",
@"	var $arr = [];
");
		}

		[Test]
		public void ArrayWithSpecifiedDimensionCanBeCreated() {
			AssertCorrect(
@"public void M() {
	int c = 0;
	// BEGIN
	var arr = new int[c];
	// END
}",
@"	var $arr = $CreateArray({def_Int32}, $c);
");
		}

		[Test]
		public void SimpleArrayCreationWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0, b = 0, c = 0, d = 0;
	// BEGIN
	var arr = new[] { a, b, c, d };
	// END
}",
@"	var $arr = [$a, $b, $c, $d];
");
		}

		[Test]
		public void ArrayCreationEvaluatesArgumentsInCorrectOrder() {
			AssertCorrect(
@"public int P { get; set; }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public void M() {
	int a = 0;
	// BEGIN
	var arr = new[] { F1(), F2(), (P = a), F3() };
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$P($a);
	var $arr = [$tmp1, $tmp2, $a, this.$F3()];
");
		}

		[Test]
		public void CreatingAMultiDimensionalArrayWithoutInitializerWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	var arr = new int[i, j];
	// END
}",
@"	var $arr = $CreateArray({def_Int32}, $i, $j);
");
		}

		[Test]
		public void CreatingAMultiDimensionalArrayWithoutInitializerInvokeSizesInTheCorrectOrder() {
			AssertCorrect(
@"public int P { get; set; }
public int F1() { return 0; }
public int F2() { return 0; }

public void M() {
	// BEGIN
	var arr = new int[F1(), P = F2()];
	// END
}",
@"	var $tmp2 = this.$F1();
	var $tmp1 = this.$F2();
	this.set_$P($tmp1);
	var $arr = $CreateArray({def_Int32}, $tmp2, $tmp1);
");
		}

		[Test]
		public void CreatingAMultiDimensionalArrayWithInitializerWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var arr = new int[,,] { { { 1, 2 }, { 3, 4 }, { 5, 6 } }, { { 7, 8 }, { 9, 10 }, { 11, 12 } }, { { 13, 14 }, { 15, 16 }, { 17, 18 } }, { { 19, 20 }, { 21, 22 }, { 23, 24 } } };
	// END
}",
@"	var $tmp1 = $CreateArray({def_Int32}, 4, 3, 2);
	$MultidimArraySet($tmp1, 0, 0, 0, 1);
	$MultidimArraySet($tmp1, 0, 0, 1, 2);
	$MultidimArraySet($tmp1, 0, 1, 0, 3);
	$MultidimArraySet($tmp1, 0, 1, 1, 4);
	$MultidimArraySet($tmp1, 0, 2, 0, 5);
	$MultidimArraySet($tmp1, 0, 2, 1, 6);
	$MultidimArraySet($tmp1, 1, 0, 0, 7);
	$MultidimArraySet($tmp1, 1, 0, 1, 8);
	$MultidimArraySet($tmp1, 1, 1, 0, 9);
	$MultidimArraySet($tmp1, 1, 1, 1, 10);
	$MultidimArraySet($tmp1, 1, 2, 0, 11);
	$MultidimArraySet($tmp1, 1, 2, 1, 12);
	$MultidimArraySet($tmp1, 2, 0, 0, 13);
	$MultidimArraySet($tmp1, 2, 0, 1, 14);
	$MultidimArraySet($tmp1, 2, 1, 0, 15);
	$MultidimArraySet($tmp1, 2, 1, 1, 16);
	$MultidimArraySet($tmp1, 2, 2, 0, 17);
	$MultidimArraySet($tmp1, 2, 2, 1, 18);
	$MultidimArraySet($tmp1, 3, 0, 0, 19);
	$MultidimArraySet($tmp1, 3, 0, 1, 20);
	$MultidimArraySet($tmp1, 3, 1, 0, 21);
	$MultidimArraySet($tmp1, 3, 1, 1, 22);
	$MultidimArraySet($tmp1, 3, 2, 0, 23);
	$MultidimArraySet($tmp1, 3, 2, 1, 24);
	var $arr = $tmp1;
");
		}

		[Test]
		public void CreatingAMultiDimensionalArrayWithInitializerEvaluatesItemsInTheCorrectOrder() {
			AssertCorrect(
@"public int P { get; set; }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public int F4() { return 0; }

public void M() {
	// BEGIN
	var arr = new int[,] { { F1(), F2() }, { P = F3(), F4() } };
	// END
}",
@"	var $tmp1 = $CreateArray({def_Int32}, 2, 2);
	var $tmp3 = this.$F1();
	var $tmp4 = this.$F2();
	var $tmp2 = this.$F3();
	this.set_$P($tmp2);
	$MultidimArraySet($tmp1, 0, 0, $tmp3);
	$MultidimArraySet($tmp1, 0, 1, $tmp4);
	$MultidimArraySet($tmp1, 1, 0, $tmp2);
	$MultidimArraySet($tmp1, 1, 1, this.$F4());
	var $arr = $tmp1;
");
		}
	}
}
