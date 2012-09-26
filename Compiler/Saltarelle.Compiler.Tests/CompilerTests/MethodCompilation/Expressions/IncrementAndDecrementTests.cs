using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class IncrementAndDecrementTests : MethodCompilerTestBase {
		protected void AssertCorrectForBoth(string csharp, string expected, IMetadataImporter metadataImporter = null, bool addSkeleton = true) {
			AssertCorrect(csharp, expected, metadataImporter, addSkeleton: addSkeleton);
			AssertCorrect(csharp.Replace("+", "-"), expected.Replace("+", "-"), metadataImporter, addSkeleton: addSkeleton);
		}

		[Test]
		public void PrefixWorksForLocalVariables() {
			AssertCorrectForBoth(
@"public void M() {
	int i = 0;
	// BEGIN
	++i;
	// END
}
",
@"	++$i;
");
		}

		[Test]
		public void PostfixWorksForLocalVariables() {
			AssertCorrectForBoth(
@"public void M() {
	int i = 0;
	// BEGIN
	i++;
	// END
}
",
@"	$i++;
");
		}

		[Test]
		public void PrefixWorksForDynamicMembers() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	++d.someMember;
	// END
}
",
@"	++$d.someMember;
");
		}

		[Test]
		public void PostfixWorksForDynamicMembers() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d.someMember++;
	// END
}
",
@"	$d.someMember++;
");
		}

		[Test]
		public void PrefixWorksForDynamicObjects() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	++d;
	// END
}
",
@"	++$d;
");
		}

		[Test]
		public void PostfixWorksForDynamicObjects() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d++;
	// END
}
",
@"	$d++;
");
		}

		[Test]
		public void PrefixWorksForDynamicIndexers() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	++d[""X""];
	// END
}
",
@"	++$d['X'];
");
		}

		[Test]
		public void PostfixWorksForDynamicIndexers() {
			AssertCorrectForBoth(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d[""X""]++;
	// END
}
",
@"	$d['X']++;
");
		}

		[Test]
		public void PrefixForPropertyWithMethodsWorksWhenTheReturnValueIsNotUsed() {
			AssertCorrectForBoth(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	this.set_$P(this.get_$P() + 1);
");
		}

		[Test]
		public void PostfixForPropertyWithMethodsWorksWhenTheReturnValueIsNotUsed() {
			AssertCorrectForBoth(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P++;
	// END
}",
@"	this.set_$P(this.get_$P() + 1);
");
		}

		[Test]
		public void PrefixForPropertyWithMethodsWorksWhenTheReturnValueIsUsed() {
			AssertCorrectForBoth(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	int j = ++P;
	// END
}",
@"	var $tmp1 = this.get_$P() + 1;
	this.set_$P($tmp1);
	var $j = $tmp1;
");
		}

		[Test]
		public void PostfixForPropertyWithMethodsWorksWhenTheReturnValueIsUsed() {
			AssertCorrectForBoth(
@"public int P { get; set; }
public void M() {
	// BEGIN
	int j = P++;
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($tmp1 + 1);
	var $j = $tmp1;
");
		}

		[Test]
		public void PrefixForPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBoth(
@"class X { public int P { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	++F().P;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.set_$P($tmp1.get_$P() + 1);
");
		}

		[Test]
		public void PostfixForPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBoth(
@"class X { public int P { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	F().P++;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.set_$P($tmp1.get_$P() + 1);
");
		}

		[Test]
		public void PrefixForPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"public int F { get; set; }
public void M() {
	// BEGIN
	++F;
	// END
}",
@"	++this.$F;
");
		}

		[Test]
		public void PostfixForPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"public int F { get; set; }
public void M() {
	// BEGIN
	F++;
	// END
}",
@"	this.$F++;
");
		}

		[Test]
		public void PrefixForPropertyWithFieldImplementationDoesNotGenerateTemporary() {
			AssertCorrectForBoth(
@"class X { public int F { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	++F().F;
	// END
}",
@"	++this.$F().$F;
");
		}

		[Test]
		public void PostfixForPropertyWithFieldImplementationDoesNotGenerateTemporary() {
			AssertCorrectForBoth(
@"class X { public int F { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	F().F++;
	// END
}",
@"	this.$F().$F++;
");
		}

		[Test]
		public void PrefixForStaticPropertyWithSetMethodWorks() {
			AssertCorrectForBoth(
@"static int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	{sm_C}.set_$P({sm_C}.get_$P() + 1);
");
		}

		[Test]
		public void PostfixForStaticPropertyWithSetMethodWorks() {
			AssertCorrectForBoth(
@"static int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P++;
	// END
}",
@"	{sm_C}.set_$P({sm_C}.get_$P() + 1);
");
		}

		[Test]
		public void PrefixForStaticPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"static int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++F;
	// END
}",
@"	++{sm_C}.$F;
");
		}

		[Test]
		public void PostfixForStaticPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"static int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F++;
	// END
}",
@"	{sm_C}.$F++;
");
		}

		[Test]
		public void PrefixForIndexerWithSetMethodWorks() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	++this[i, j];
	// END
}",
@"	this.set_$Item($i, $j, this.get_$Item($i, $j) + 1);
");
		}

		[Test]
		public void PostfixForIndexerWithSetMethodWorks() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	this[i, j]++;
	// END
}",
@"	this.set_$Item($i, $j, this.get_$Item($i, $j) + 1);
");
		}

		[Test]
		public void PrefixForIndexerWithMethodsWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k;
	// BEGIN
	k = ++this[i, j];
	// END
}",
@"	var $tmp1 = this.get_$Item($i, $j) + 1;
	this.set_$Item($i, $j, $tmp1);
	$k = $tmp1;
");
		}

		[Test]
		public void PostfixForIndexerWithMethodsWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k;
	// BEGIN
	k = this[i, j]++;
	// END
}",
@"	var $tmp1 = this.get_$Item($i, $j);
	this.set_$Item($i, $j, $tmp1 + 1);
	$k = $tmp1;
");
		}

		[Test]
		public void PrefixForIndexerWithMethodsOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	++this[F1(), F2()];
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$Item($tmp1, $tmp2, this.get_$Item($tmp1, $tmp2) + 1);
");
		}

		[Test]
		public void PostfixForIndexerWithMethodsOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"int this[int x, int y] { get { return 0; } set {} }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[F1(), F2()]++;
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$Item($tmp1, $tmp2, this.get_$Item($tmp1, $tmp2) + 1);
");
		}

		[Test]
		public void PrefixForPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrectForBoth(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	j = ++this[i];
	// END
}",
@"	$j = ++this[$i];
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void PostfixForPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrectForBoth(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	j = this[i]++;
	// END
}",
@"	$j = this[$i]++;
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void PrefixForIndexerWorksWhenReorderingArguments() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	++this[d: F1(), g: F2(), f: F3(), b: F4()];
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $tmp4 = this.$F4();
	this.set_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2, this.get_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2) + 1);
");
		}

		[Test]
		public void PostfixForIndexerWorksWhenReorderingArguments() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[d: F1(), g: F2(), f: F3(), b: F4()]++;
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $tmp4 = this.$F4();
	this.set_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2, this.get_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2) + 1);
");
		}

		[Test]
		public void PrefixForIndexerWorksWhenReorderingArgumentsAndUsingTheReturnValue() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	i = ++this[d: F1(), g: F2(), f: F3(), b: F4()];
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $tmp4 = this.$F4();
	var $tmp5 = this.get_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2) + 1;
	this.set_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2, $tmp5);
	$i = $tmp5;
");
		}

		[Test]
		public void PostfixForIndexerWorksWhenReorderingArgumentsAndUsingTheReturnValue() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	i = this[d: F1(), g: F2(), f: F3(), b: F4()]++;
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $tmp4 = this.$F4();
	var $tmp5 = this.get_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2);
	this.set_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2, $tmp5 + 1);
	$i = $tmp5;
");
		}

		[Test]
		public void PrefixForIndexerImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	++this[i, j];
	// END
}",
@"	set_(this)._($i)._($j)._(get_(this)._($i)._($j) + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})._({x})._({y})"), MethodScriptSemantics.InlineCode("set_({this})._({x})._({y})._({value})")) : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void PostfixForIndexerImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	++this[i, j];
	// END
}",
@"	set_(this)._($i)._($j)._(get_(this)._($i)._($j) + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})._({x})._({y})"), MethodScriptSemantics.InlineCode("set_({this})._({x})._({y})._({value})")) : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void PrefixForPropertyWithSetMethodImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	set_(this)._(get_(this) + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")) });
		}

		[Test]
		public void PostfixForPropertyWithSetMethodImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	set_(this)._(get_(this) + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")) });
		}

		[Test]
		public void PrefixForMultiDimensionalArrayWorks() {
			AssertCorrectForBoth(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1;
	// BEGIN
	++arr[i, j];
	// END
}",
@"	$MultidimArraySet($arr, $i, $j, $MultidimArrayGet($arr, $i, $j) + 1);
");
		}

		[Test]
		public void PostfixForMultiDimensionalArrayWorks() {
			AssertCorrectForBoth(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1;
	// BEGIN
	arr[i, j]++;
	// END
}",
@"	$MultidimArraySet($arr, $i, $j, $MultidimArrayGet($arr, $i, $j) + 1);
");
		}

		[Test]
		public void PrefixForMultiDimensionalArrayWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1, k;
	// BEGIN
	k = ++arr[i, j];
	// END
}",
@"	var $tmp1 = $MultidimArrayGet($arr, $i, $j) + 1;
	$MultidimArraySet($arr, $i, $j, $tmp1);
	$k = $tmp1;
");
		}

		[Test]
		public void PostfixForMultiDimensionalArrayWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int[,] arr;
	int i = 0, j = 1, k;
	// BEGIN
	k = arr[i, j]++;
	// END
}",
@"	var $tmp1 = $MultidimArrayGet($arr, $i, $j);
	$MultidimArraySet($arr, $i, $j, $tmp1 + 1);
	$k = $tmp1;
");
		}

		[Test]
		public void PrefixForMultiDimensionalArrayOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"public int[,] A() { return null; }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	++A()[F1(), F2()];
	// END
}",
@"	var $tmp1 = this.$A();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	$MultidimArraySet($tmp1, $tmp2, $tmp3, $MultidimArrayGet($tmp1, $tmp2, $tmp3) + 1);
");
		}

		[Test]
		public void PostfixForMultiDimensionalArrayOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"public int[,] A() { return null; }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	A()[F1(), F2()]++;
	// END
}",
@"	var $tmp1 = this.$A();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	$MultidimArraySet($tmp1, $tmp2, $tmp3, $MultidimArrayGet($tmp1, $tmp2, $tmp3) + 1);
");
		}

		[Test]
		public void PrefixForInstanceFieldWorks() {
			AssertCorrectForBoth(
@"int a;
public void M() {
	int i = 0;
	// BEGIN
	++a;
	// END
}",
@"	++this.$a;
");
		}

		[Test]
		public void PostfixForInstanceFieldWorks() {
			AssertCorrectForBoth(
@"int a;
public void M() {
	int i = 0;
	// BEGIN
	a++;
	// END
}",
@"	this.$a++;
");
		}

		[Test]
		public void PrefixForStaticFieldWorks() {
			AssertCorrectForBoth(
@"static int a;
public void M() {
	int i = 0;
	// BEGIN
	++a;
	// END
}",
@"	++{sm_C}.$a;
");
		}

		[Test]
		public void PostfixForStaticFieldWorks() {
			AssertCorrectForBoth(
@"static int a;
public void M() {
	int i = 0;
	// BEGIN
	a++;
	// END
}",
@"	{sm_C}.$a++;
");
		}

		[Test]
		public void LiftedPrefixWorksForLocalVariables() {
			AssertCorrectForBoth(
@"public void M() {
	int? i = 0;
	// BEGIN
	++i;
	// END
}
",
@"	$i = $Lift($i + 1);
");
		}

		[Test]
		public void LiftedPostfixWorksForLocalVariables() {
			AssertCorrectForBoth(
@"public void M() {
	int? i = 0;
	// BEGIN
	i++;
	// END
}
",
@"	$i = $Lift($i + 1);
");
		}

		[Test]
		public void LiftedPrefixWorksForLocalVariablesWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int? i = 0;
	// BEGIN
	var j = ++i;
	// END
}
",
@"	var $j = $i = $Lift($i + 1);
");
		}

		[Test]
		public void LiftedPostfixWorksForLocalVariablesWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int? i = 0;
	// BEGIN
	var j = i++;
	// END
}
",
@"	var $tmp1 = $i;
	$i = $Lift($tmp1 + 1);
	var $j = $tmp1;
");
		}
		[Test]
		public void LiftedPrefixForPropertyWithMethodsWorksWhenTheReturnValueIsNotUsed() {
			AssertCorrectForBoth(
@"public int? P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	this.set_$P($Lift(this.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPostfixForPropertyWithMethodsWorksWhenTheReturnValueIsNotUsed() {
			AssertCorrectForBoth(
@"public int? P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P++;
	// END
}",
@"	this.set_$P($Lift(this.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPrefixForPropertyWithMethodsWorksWhenTheReturnValueIsUsed() {
			AssertCorrectForBoth(
@"public int? P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	int? j = ++P;
	// END
}",
@"	var $tmp1 = $Lift(this.get_$P() + 1);
	this.set_$P($tmp1);
	var $j = $tmp1;
");
		}

		[Test]
		public void LiftedPostfixForPropertyWithMethodsWorksWhenTheReturnValueIsUsed() {
			AssertCorrectForBoth(
@"public int? P { get; set; }
public void M() {
	// BEGIN
	int? j = P++;
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($Lift($tmp1 + 1));
	var $j = $tmp1;
");
		}

		[Test]
		public void LiftedPrefixForPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBoth(
@"class X { public int? P { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	++F().P;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.set_$P($Lift($tmp1.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPostfixForPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBoth(
@"class X { public int? P { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	F().P++;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.set_$P($Lift($tmp1.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPrefixForPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"public int? F { get; set; }
public void M() {
	// BEGIN
	++F;
	// END
}",
@"	this.$F = $Lift(this.$F + 1);
");
		}

		[Test]
		public void LiftedPostfixForPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"public int? F { get; set; }
public void M() {
	// BEGIN
	F++;
	// END
}",
@"	this.$F = $Lift(this.$F + 1);
");
		}

		[Test]
		public void LiftedPrefixForPropertyWithFieldImplementationWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public int? F { get; set; }
public void M() {
	// BEGIN
	var x = ++F;
	// END
}",
@"	var $x = this.$F = $Lift(this.$F + 1);
");
		}

		[Test]
		public void LiftedPostfixForPropertyWithFieldImplementationWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public int? F { get; set; }
public void M() {
	// BEGIN
	var x = F++;
	// END
}",
@"	var $tmp1 = this.$F;
	this.$F = $Lift($tmp1 + 1);
	var $x = $tmp1;
");
		}

		[Test]
		public void LiftedPrefixForStaticPropertyWithSetMethodWorks() {
			AssertCorrectForBoth(
@"static int? P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++P;
	// END
}",
@"	{sm_C}.set_$P($Lift({sm_C}.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPostfixForStaticPropertyWithSetMethodWorks() {
			AssertCorrectForBoth(
@"static int? P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P++;
	// END
}",
@"	{sm_C}.set_$P($Lift({sm_C}.get_$P() + 1));
");
		}

		[Test]
		public void LiftedPrefixForStaticPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"static int? F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	++F;
	// END
}",
@"	{sm_C}.$F = $Lift({sm_C}.$F + 1);
");
		}

		[Test]
		public void LiftedPostfixForStaticPropertyWithFieldImplementationWorks() {
			AssertCorrectForBoth(
@"static int? F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F++;
	// END
}",
@"	{sm_C}.$F = $Lift({sm_C}.$F + 1);
");
		}

		[Test]
		public void LiftedPrefixForIndexerWithSetMethodWorks() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	++this[i, j];
	// END
}",
@"	this.set_$Item($i, $j, $Lift(this.get_$Item($i, $j) + 1));
");
		}

		[Test]
		public void LiftedPostfixForIndexerWithSetMethodWorks() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	this[i, j]++;
	// END
}",
@"	this.set_$Item($i, $j, $Lift(this.get_$Item($i, $j) + 1));
");
		}

		[Test]
		public void LiftedPrefixForIndexerWithMethodsWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	var x = ++this[i, j];
	// END
}",
@"	var $tmp1 = $Lift(this.get_$Item($i, $j) + 1);
	this.set_$Item($i, $j, $tmp1);
	var $x = $tmp1;
");
		}

		[Test]
		public void LiftedPostfixForIndexerWithMethodsWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1;
	// BEGIN
	var k = this[i, j]++;
	// END
}",
@"	var $tmp1 = this.get_$Item($i, $j);
	this.set_$Item($i, $j, $Lift($tmp1 + 1));
	var $k = $tmp1;
");
		}

		[Test]
		public void LiftedPrefixForIndexerWithMethodsOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	++this[F1(), F2()];
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$Item($tmp1, $tmp2, $Lift(this.get_$Item($tmp1, $tmp2) + 1));
");
		}

		[Test]
		public void LiftedPostfixForIndexerWithMethodsOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBoth(
@"int? this[int x, int y] { get { return 0; } set {} }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[F1(), F2()]++;
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$Item($tmp1, $tmp2, $Lift(this.get_$Item($tmp1, $tmp2) + 1));
");
		}

		[Test]
		public void LiftedPrefixForPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrectForBoth(
@"int? this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	// BEGIN
	++this[i];
	// END
}",
@"	this[$i] = $Lift(this[$i] + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void LiftedPostfixForPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrectForBoth(
@"int? this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	// BEGIN
	this[i]++;
	// END
}",
@"	this[$i] = $Lift(this[$i] + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void LiftedPrefixForPropertyImplementedAsNativeIndexerWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	// BEGIN
	var x = ++this[i];
	// END
}",
@"	var $x = this[$i] = $Lift(this[$i] + 1);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void LiftedPostfixForPropertyImplementedAsNativeIndexerWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	// BEGIN
	var x = this[i]++;
	// END
}",
@"	var $tmp1 = this[$i];
	this[$i] = $Lift($tmp1 + 1);
	var $x = $tmp1;
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void LiftedPrefixForArrayAccessWorks() {
			AssertCorrectForBoth(
@"public void M() {
	int?[] arr = null;
	int i = 0;
	// BEGIN
	++arr[i];
	// END
}",
@"	$arr[$i] = $Lift($arr[$i] + 1);
");
		}

		[Test]
		public void LiftedPostfixForArrayAccessWorks() {
			AssertCorrectForBoth(
@"public void M() {
	int?[] arr = null;
	int i = 0;
	// BEGIN
	arr[i]++;
	// END
}",
@"	$arr[$i] = $Lift($arr[$i] + 1);
");
		}

		[Test]
		public void LiftedPrefixForArrayAccessWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int?[] arr = null;
	int i = 0;
	// BEGIN
	var x = ++arr[i];
	// END
}",
@"	var $x = $arr[$i] = $Lift($arr[$i] + 1);
");
		}

		[Test]
		public void LiftedPostfixForArrayAccessWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"public void M() {
	int?[] arr = null;
	int i = 0;
	// BEGIN
	var x = arr[i]++;
	// END
}",
@"	var $tmp1 = $arr[$i];
	$arr[$i] = $Lift($tmp1 + 1);
	var $x = $tmp1;
");
		}

		[Test]
		public void LiftedPrefixForInstanceFieldWorks() {
			AssertCorrectForBoth(
@"int? a;
public void M() {
	int i = 0;
	// BEGIN
	++a;
	// END
}",
@"	this.$a = $Lift(this.$a + 1);
");
		}

		[Test]
		public void LiftedPostfixForInstanceFieldWorks() {
			AssertCorrectForBoth(
@"int? a;
public void M() {
	int i = 0;
	// BEGIN
	a++;
	// END
}",
@"	this.$a = $Lift(this.$a + 1);
");
		}

		[Test]
		public void LiftedPrefixForInstanceFieldWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? a;
public void M() {
	int i = 0;
	// BEGIN
	var x = ++a;
	// END
}",
@"	var $x = this.$a = $Lift(this.$a + 1);
");
		}

		[Test]
		public void LiftedPostfixForInstanceFieldWorksWhenUsingTheReturnValue() {
			AssertCorrectForBoth(
@"int? a;
public void M() {
	int i = 0;
	// BEGIN
	var x = a++;
	// END
}",
@"	var $tmp1 = this.$a;
	this.$a = $Lift($tmp1 + 1);
	var $x = $tmp1;
");
		}

		[Test]
		public void LiftedPrefixForStaticFieldWorks() {
			AssertCorrectForBoth(
@"static int? a;
public void M() {
	int i = 0;
	// BEGIN
	++a;
	// END
}",
@"	{sm_C}.$a = $Lift({sm_C}.$a + 1);
");
		}

		[Test]
		public void LiftedPostfixForStaticFieldWorks() {
			AssertCorrectForBoth(
@"static int? a;
public void M() {
	int i = 0;
	// BEGIN
	a++;
	// END
}",
@"	{sm_C}.$a = $Lift({sm_C}.$a + 1);
");
		}

		[Test]
		public void PrefixWorksForBaseCall() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}
class D : B {
	public override int P { get; set; }
	public void M() {
		// BEGIN
		int i = ++base.P;
		// END
	}
}",
@"	var $tmp1 = $CallBase({bind_B}, 'get_$P', [], [this]) + 1;
	$CallBase({bind_B}, 'set_$P', [], [this, $tmp1]);
	var $i = $tmp1;
", addSkeleton: false);
		}

		[Test]
		public void PostfixWorksForBaseCall() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}
class D : B {
	public override int P { get; set; }
	public void M() {
		// BEGIN
		int i = base.P++;
		// END
	}
}",
@"	var $tmp1 = $CallBase({bind_B}, 'get_$P', [], [this]);
	$CallBase({bind_B}, 'set_$P', [], [this, $tmp1 + 1]);
	var $i = $tmp1;
", addSkeleton: false);
		}

		[Test]
		public void PrefixWorksForDynamicPropertyOfNonDynamicObject() {
			AssertCorrectForBoth(@"
public class SomeClass {
    public dynamic Value { get; set; }
}

class C {
    public void M() {
        var c = new SomeClass();
		// BEGIN
        ++c.Value;
		// END
    }
}",
@"	$c.set_$Value($c.get_$Value() + 1);
", addSkeleton: false);
		}

		[Test]
		public void PostfixWorksForDynamicPropertyOfNonDynamicObject() {
			AssertCorrectForBoth(@"
public class SomeClass {
    public dynamic Value { get; set; }
}

class C {
    public void M() {
        var c = new SomeClass();
		// BEGIN
        c.Value++;
		// END
    }
}",
@"	$c.set_$Value($c.get_$Value() + 1);
", addSkeleton: false);
		}

		[Test]
		public void PrefixWorksForDynamicFieldOfNonDynamicObject() {
			AssertCorrectForBoth(@"
public class SomeClass {
    public dynamic Value;
}

class C {
    public void M() {
        var c = new SomeClass();
		// BEGIN
        ++$c.Value;
		// END
    }
}",
@"	++$c.$Value;
", addSkeleton: false);
		}

		[Test]
		public void PostfixWorksForDynamicFieldOfNonDynamicObject() {
			AssertCorrectForBoth(@"
public class SomeClass {
    public dynamic Value;
}

class C {
    public void M() {
        var c = new SomeClass();
		// BEGIN
        $c.Value++;
		// END
    }
}",
@"	$c.$Value++;
", addSkeleton: false);
		}
	}
}
