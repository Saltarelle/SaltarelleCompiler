using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class OperatorTests : MethodCompilerTestBase {
		[Test]
		public void AssignmentWorksForLocalVariables() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1;
	// BEGIN
	i = j;
	// END
}
",
@"	$i = $j;
");
		}

		[Test]
		public void AssignmentChainWorksForlLocalVariables() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1, k = 2;;
	// BEGIN
	i = j = k;
	// END
}
",
@"	$i = $j = $k;
");
		}

		[Test]
		public void AssigningToPropertyWithSetMethodWorks() {
			AssertCorrect(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	this.set_P($i);
");
		}

		[Test]
		public void AssignmentChainForPropertiesWithSetMethodsWorksWithSimpleArgument() {
			AssertCorrect(
@"public int P1 { get; set; }
public int P2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P1 = P2 = i;
	// END
}",
@"	this.set_P2($i);
	this.set_P1($i);
");
		}

		[Test]
		public void AssigningPropertyWithSetMethodsWorksWithComplexArgument() {
			AssertCorrect(
@"public int P1 { get; set; }
public int F() { return 0; }
public void M() {
	// BEGIN
	P1 = F();
	// END
}",
@"	this.set_P1(this.F());
");
		}


		[Test]
		public void AssignmentChainForPropertiesWithSetMethodsWorksWithComplexArgument() {
			AssertCorrect(
@"public int P1 { get; set; }
public int P2 { get; set; }
public int F() { return 0; }
public void M() {
	// BEGIN
	P1 = P2 = F();
	// END
}",
@"	var $tmp1 = this.F();
	this.set_P2($tmp1);
	this.set_P1($tmp1);
");
		}

		[Test]
		public void AssignmentChainForPropertiesWithSetMethodsWorksWhenReturnValueUsed() {
			AssertCorrect(
@"public bool P1 { get; set; }
public bool P2 { get; set; }
public bool F() { return false; }
public void M() {
	// BEGIN
	if (P1 = P2 = F()) {
	}
	// END
}",
@"	var $tmp1 = this.F();
	this.set_P2($tmp1);
	this.set_P1($tmp1);
	if ($tmp1) {
	}
");
		}

		[Test]
		public void AssigningToPropertyWithFieldImplementationWorks() {
			AssertCorrect(
@"public int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F = i;
	// END
}",
@"	this.F = $i;
");
		}

		[Test]
		public void AssignmentChainForPropertiesWithFieldImplementationWorks() {
			AssertCorrect(
@"public int F1 { get; set; }
public int F2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1 = F2 = i;
	// END
}",
@"	this.F1 = this.F2 = $i;
");
		}

		[Test]
		public void AssigningToStaticPropertyWithSetMethodWorks() {
			AssertCorrect(
@"static int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	{C}.set_P($i);
");
		}

		[Test]
		public void AssigningToStaticPropertyWithFieldImplementationWorks() {
			AssertCorrect(
@"static int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F = i;
	// END
}",
@"	{C}.F = $i;
");
		}

		[Test]
		public void AssigningToIndexerWithSetMethodWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] = k;
	// END
}",
@"	this.set_Item($i, $j, $k);
");
		}

		[Test]
		public void AssigningToIndexerWithSetMethodWorksWhenUsingTheReturnValue() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i, j] = k;
	// END
}",
@"	this.set_Item($i, $j, $k);
	$l = $k;
");
		}

		[Test]
		public void AssigningToPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrect(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i] = k;
	// END
}",
@"	$l = this[$i] = $k;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
		}

		[Test]
		public void UsingPropertyThatIsNotUsableFromScriptGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenPropertiesAreSet() {
			AssertCorrect(
@"class C { public int P { get; set; } }
public C F1() { return null; }
public C F2() { return null; }
public int F() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	F1().P = F2().P = F();
	// END
}",
@"	var $tmp3 = this.F1();
	var $tmp1 = this.F2();
	var $tmp2 = this.F();
	$tmp1.set_P($tmp2);
	$tmp3.set_P($tmp2);
");
		}

		[Test]
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenPropertiesWithFieldImplementationAreSet() {
			AssertCorrect(
@"class C { public int F { get; set; } }
C F1() { return null; }
C F2() { return null; }
int F() { return 0; }
int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1().F = F2().F = P = F();
	// END
}",
@"	var $tmp3 = this.F1();
	var $tmp2 = this.F2();
	var $tmp1 = this.F();
	this.set_P($tmp1);
	$tmp3.F = $tmp2.F = $tmp1;
");
		}

		[Test]
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenSetMethodIndexersAreUsed() {
			AssertCorrect(
@"class C { public int this[int x, int y] { get { return 0; } set {} } }
public C FC() { return null; }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	i = FC()[F1(), F2()] = F3();
	// END
}",
@"	var $tmp1 = this.FC();
	var $tmp2 = this.F1();
	var $tmp3 = this.F2();
	var $tmp4 = this.F3();
	$tmp1.set_Item($tmp2, $tmp3, $tmp4);
	$i = $tmp4;
");
		}

		[Test]
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenNativeIndexersAreUsed() {
			AssertCorrect(
@"class C { public int this[int x, int y] { get { return 0; } set {} } }
public C FC() { return null; }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	i = FC()[F1(), F2()] = F3();
	// END
}",
@"	var $tmp1 = this.FC();
	var $tmp2 = this.F1();
	var $tmp3 = this.F2();
	var $tmp4 = this.F3();
	$tmp1.set_Item($tmp2, $tmp3, $tmp4);
	$i = $tmp4;
");
		}
	}
}
