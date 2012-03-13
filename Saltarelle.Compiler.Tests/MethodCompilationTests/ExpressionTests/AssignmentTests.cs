using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class AssignmentTests : MethodCompilerTestBase {
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
@"	this.set_$P($i);
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
@"	this.set_$P2($i);
	this.set_$P1($i);
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
@"	this.set_$P1(this.$F());
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
@"	var $tmp1 = this.$F();
	this.set_$P2($tmp1);
	this.set_$P1($tmp1);
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
@"	var $tmp1 = this.$F();
	this.set_$P2($tmp1);
	this.set_$P1($tmp1);
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
@"	this.$F = $i;
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
@"	this.$F1 = this.$F2 = $i;
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
@"	{C}.set_$P($i);
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
@"	{C}.$F = $i;
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
@"	this.set_$Item($i, $j, $k);
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
@"	this.set_$Item($i, $j, $k);
	$l = $k;
");
		}

		[Test]
		public void AssigningToIndexerWorksWhenReorderingArguments() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[d: F1(), g: F2(), f: F3(), b: F4()] = i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	this.set_$Item(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2, $i);
");
		}

		[Test]
		public void AssigningToIndexerImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] = k;
	// END
}",
@"	set_this_$i_$j_$k;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InlineCode("get_{this}_{x}_{y}"), MethodImplOptions.InlineCode("set_{this}_{x}_{y}_{value}")) : PropertyImplOptions.Field(p.Name) });
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
		public void AssigningToPropertyWithSetMethodImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	set_this_$i;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InlineCode("get_{this}"), MethodImplOptions.InlineCode("set_{this}_{value}")) });
		}

		[Test]
		public void AssigningToInstanceFieldWorks() {
			AssertCorrect(
@"int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a = b = i;
	// END
}",
@"	this.$a = this.$b = $i;
");
		}

		[Test]
		public void AssigningToStaticFieldWorks() {
			AssertCorrect(
@"static int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a = b = i;
	// END
}",
@"	{C}.$a = {C}.$b = $i;
");
		}

		[Test]
		public void UsingPropertyThatIsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { UnusableProperty = 0; } }" }, namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableProperty")));
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
@"	var $tmp3 = this.$F1();
	var $tmp1 = this.$F2();
	var $tmp2 = this.$F();
	$tmp1.set_$P($tmp2);
	$tmp3.set_$P($tmp2);
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
@"	var $tmp3 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp1 = this.$F();
	this.set_$P($tmp1);
	$tmp3.$F = $tmp2.$F = $tmp1;
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
@"	var $tmp1 = this.$FC();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	$tmp1.set_$Item($tmp2, $tmp3, $tmp4);
	$i = $tmp4;
");
		}

		[Test]
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenIndexersWithGetMethodsAreUsed() {
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
@"	var $tmp1 = this.$FC();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	$tmp1.set_$Item($tmp2, $tmp3, $tmp4);
	$i = $tmp4;
");
		}

		[Test]
		public void CanAssignToArrayElement() {
			AssertCorrect(
@"public void M() {
	int[] arr = null;
	int i = 0;
	// BEGIN
	arr[0] = i;
	// END
}",
@"	$arr[0] = $i;
");
		}

		[Test]
		public void ArrayAccessEvaluatesArgumentsInTheCorrectOrder() {
			AssertCorrect(
@"int[] arr;
int P { get; set; }
int F() { return 0; }
public void M() {
	// BEGIN
	arr[P] = (P = F());
	// END
}",
@"	var $tmp2 = this.$arr;
	var $tmp3 = this.get_$P();
	var $tmp1 = this.$F();
	this.set_$P($tmp1);
	$tmp2[$tmp3] = $tmp1;
");
		}

		[Test]
		public void AssigningToByRefLocalWorks() {
			AssertCorrect(
@"int[] arr;
int i;
int F() { return 0; }
public void M(ref int i) {
	// BEGIN
	i = 1;
	// END
}",
@"	$i.$ = 1;
");
		}
	}
}
