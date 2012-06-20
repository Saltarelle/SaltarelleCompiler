using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
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
@"	var $arr = $CreateArray($c);
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
		public void DeclaringAMultiDimensionalArrayIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public void M() { var arr = new int[2, 2]; } }" }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("dimension")));
		}

		[Test]
		public void DeclaringAMultiDimensionalArrayIsAnError2() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public void M() { var arr = new int[,] {}; } }" }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("dimension")));
		}
	}
}
