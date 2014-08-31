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
		public void SimpleArrayCreationWorksStruct() {
			AssertCorrect(
@"public void M() {
	int a = 0, b = 0, c = 0, d = 0;
	// BEGIN
	var arr = new[] { a, b, c, d };
	// END
}",
@"	var $arr = [$Clone($a, {to_Int32}), $Clone($b, {to_Int32}), $Clone($c, {to_Int32}), $Clone($d, {to_Int32})];
", mutableValueTypes: true);
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
	$MultidimArraySet($tmp1, 0, 0, this.$F1());
	$MultidimArraySet($tmp1, 0, 1, this.$F2());
	var $tmp2 = this.$F3();
	this.set_$P($tmp2);
	$MultidimArraySet($tmp1, 1, 0, $tmp2);
	$MultidimArraySet($tmp1, 1, 1, this.$F4());
	var $arr = $tmp1;
");
		}

		[Test]
		public void MultiDimensionalArrayCreationStruct() {
			AssertCorrect(
@"void M() {
	// BEGIN
	var arr = new int[,] { { 3, 2 }, { 6, 1 } };
	// END
}
",
@"	var $tmp1 = $CreateArray({def_Int32}, 2, 2);
	$MultidimArraySet($tmp1, 0, 0, $Clone(3, {to_Int32}));
	$MultidimArraySet($tmp1, 0, 1, $Clone(2, {to_Int32}));
	$MultidimArraySet($tmp1, 1, 0, $Clone(6, {to_Int32}));
	$MultidimArraySet($tmp1, 1, 1, $Clone(1, {to_Int32}));
	var $arr = $tmp1;
", mutableValueTypes: true);
		}

		[Test]
		public void JaggedArray() {
			AssertCorrect(
@"void M() {
	// BEGIN
	var arr = new int[2][];
	// END
}
",
@"	var $arr = $CreateArray(def_$Array({ga_Int32}), 2);
");
		}

		[Test]
		public void JaggedArrayWithInitializer() {
			AssertCorrect(
@"void M() {
	// BEGIN
	var arr1 = new [] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
	var arr2 = new int[][] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
	var arr3 = new int[3][] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
	// END
}
",
@"	var $arr1 = [[1, 2], [3, 4], [5, 6]];
	var $arr2 = [[1, 2], [3, 4], [5, 6]];
	var $arr3 = [[1, 2], [3, 4], [5, 6]];
");
		}

		[Test]
		public void MultiDimensionalArrayWithZeroSize() {
			AssertCorrect(
@"void M() {
	// BEGIN
	var arr = new int[,] { {}, {} };
	// END
}
",
@"	var $tmp1 = $CreateArray({def_Int32}, 2, 0);
	var $arr = $tmp1;
", mutableValueTypes: true);
		}
	}
}
