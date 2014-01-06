using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
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
		public void AssignmentWorksForLocalVariablesStruct() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	i = j;
	// END
}
",
@"	$i = $Clone($j, {to_Int32});
", valueTypes: true);
		}

		[Test]
		public void AssignmentChainWorksForLocalVariables() {
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
		public void AssignmentChainWorksForLocalVariablesStruct() {
			AssertCorrect(
@"struct S {}
public void M() {
	int i = 0, j = 0, k = 0;
	// BEGIN
	i = j = k;
	// END
}
",
@"	$j = $Clone($k, {to_Int32});
	$i = $Clone($j, {to_Int32});
", valueTypes: true);
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
		public void AssigningToPropertyWithSetMethodWorksStruct() {
			AssertCorrect(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	this.set_$P($Clone($i, {to_Int32}));
", valueTypes: true);
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
		public void AssignmentChainForPropertiesWithSetMethodsWorksWithSimpleArgumentStruct() {
			AssertCorrect(
@"public int P1 { get; set; }
public int P2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P1 = P2 = i;
	// END
}",
@"	this.set_$P2($Clone($i, {to_Int32}));
	this.set_$P1($Clone($i, {to_Int32}));
", valueTypes: true);
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
		public void AssigningPropertyWithSetMethodsWorksWithComplexArgumentStruct() {
			AssertCorrect(
@"public int P1 { get; set; }
public int F() { return 0; }
public void M() {
	// BEGIN
	P1 = F();
	// END
}",
@"	this.set_$P1(this.$F());
", valueTypes: true);
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
		public void AssignmentChainForPropertiesWithSetMethodsWorksWithComplexArgumentStruct() {
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
	this.set_$P2($Clone($tmp1, {to_Int32}));
	this.set_$P1($Clone($tmp1, {to_Int32}));
", valueTypes: true);
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
		public void AssignmentChainForPropertiesWithSetMethodsWorksWhenReturnValueUsedStruct() {
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
	this.set_$P2($Clone($tmp1, {to_Boolean}));
	this.set_$P1($Clone($tmp1, {to_Boolean}));
	if ($tmp1) {
	}
", valueTypes: true);
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
		public void AssigningToPropertyWithFieldImplementationWorksStruct() {
			AssertCorrect(
@"public int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F = i;
	// END
}",
@"	this.$F = $Clone($i, {to_Int32});
", valueTypes: true);
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
		public void AssignmentChainForPropertiesWithFieldImplementationWorksStruct() {
			AssertCorrect(
@"public int F1 { get; set; }
public int F2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1 = F2 = i;
	// END
}",
@"	this.$F2 = $Clone($i, {to_Int32});
	this.$F1 = $Clone(this.$F2, {to_Int32});
", valueTypes: true);
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
@"	{sm_C}.set_$P($i);
");
		}

		[Test]
		public void AssigningToStaticPropertyWithSetMethodWorksStruct() {
			AssertCorrect(
@"static int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	{sm_C}.set_$P($Clone($i, {to_Int32}));
", valueTypes: true);
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
@"	{sm_C}.$F = $i;
");
		}

		[Test]
		public void AssigningToStaticPropertyWithFieldImplementationWorksStruct() {
			AssertCorrect(
@"static int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F = i;
	// END
}",
@"	{sm_C}.$F = $Clone($i, {to_Int32});
", valueTypes: true);
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
		public void AssigningToIndexerWithSetMethodWorksStruct() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] = k;
	// END
}",
@"	this.set_$Item($Clone($i, {to_Int32}), $Clone($j, {to_Int32}), $Clone($k, {to_Int32}));
", valueTypes: true);
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
		public void AssigningToIndexerWithSetMethodWorksWhenUsingTheReturnValueStruct() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i, j] = k;
	// END
}",
@"	this.set_$Item($Clone($i, {to_Int32}), $Clone($j, {to_Int32}), $Clone($k, {to_Int32}));
	$l = $Clone($k, {to_Int32});
", valueTypes: true);
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
		public void AssigningToIndexerWorksWhenReorderingArgumentsStruct() {
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
	this.set_$Item($Clone(1, {to_Int32}), this.$F4(), $Clone(3, {to_Int32}), $Clone($tmp1, {to_Int32}), $Clone(5, {to_Int32}), $Clone($tmp3, {to_Int32}), $Clone($tmp2, {to_Int32}), $Clone($i, {to_Int32}));
", valueTypes: true);
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
@"	set_(this)._($i)._($j)._($k);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})._({x})._({y})"), MethodScriptSemantics.InlineCode("set_({this})._({x})._({y})._({value})")) : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void AssigningToIndexerImplementedAsInlineCodeWorksStruct() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] = k;
	// END
}",
@"	set_(this)._($Clone($i, {to_Int32}))._($Clone($j, {to_Int32}))._($Clone($k, {to_Int32}));
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})._({x})._({y})"), MethodScriptSemantics.InlineCode("set_({this})._({x})._({y})._({value})")) : PropertyScriptSemantics.Field(p.Name), GetTypeSemantics = t => TypeScriptSemantics.ValueType(t.Name) });
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
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void AssigningToPropertyImplementedAsNativeIndexerWorksStruct() {
			AssertCorrect(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i] = k;
	// END
}",
@"	this[$i] = $Clone($k, {to_Int32});
	$l = $Clone(this[$i], {to_Int32});
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name), GetTypeSemantics = t => TypeScriptSemantics.ValueType(t.Name) });
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
@"	set_(this)._($i);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")) });
		}

		[Test]
		public void AssigningToPropertyWithSetMethodImplementedAsInlineCodeWorksStruct() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P = i;
	// END
}",
@"	set_(this)._($Clone($i, {to_Int32}));
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")), GetTypeSemantics = t => TypeScriptSemantics.ValueType(t.Name) });
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
		public void AssigningToInstanceFieldWorksStruct() {
			AssertCorrect(
@"int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a = b = i;
	// END
}",
@"	this.$b = $Clone($i, {to_Int32});
	this.$a = $Clone(this.$b, {to_Int32});
", valueTypes: true);
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
@"	{sm_C}.$a = {sm_C}.$b = $i;
");
		}

		[Test]
		public void AssigningToStaticFieldWorksStruct() {
			AssertCorrect(
@"static int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a = b = i;
	// END
}",
@"	{sm_C}.$b = $Clone($i, {to_Int32});
	{sm_C}.$a = $Clone({sm_C}.$b, {to_Int32});
", valueTypes: true);
		}

		[Test]
		public void UsingPropertyThatIsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { UnusableProperty = 0; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableProperty")));
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
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenPropertiesAreSetStruct() {
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
	$tmp1.set_$P($Clone($tmp2, {to_Int32}));
	$tmp3.set_$P($Clone($tmp2, {to_Int32}));
", valueTypes: true);
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
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenPropertiesWithFieldImplementationAreSetStruct() {
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
	this.set_$P($Clone($tmp1, {to_Int32}));
	$tmp2.$F = $Clone($tmp1, {to_Int32});
	$tmp3.$F = $Clone($tmp2.$F, {to_Int32});
", valueTypes: true);
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
		public void ExpressionsAreEvaluatedInTheCorrectOrderWhenSetMethodIndexersAreUsedStruct() {
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
	$tmp1.set_$Item($tmp2, $tmp3, $Clone($tmp4, {to_Int32}));
	$i = $Clone($tmp4, {to_Int32});
", valueTypes: true);
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
		public void CanAssignToArrayElementStruct() {
			AssertCorrect(
@"public void M() {
	int[] arr = null;
	int i = 0;
	// BEGIN
	arr[0] = i;
	// END
}",
@"	$arr[0] = $Clone($i, {to_Int32});
", valueTypes: true);
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
		public void ArrayAccessEvaluatesArgumentsInTheCorrectOrderStruct() {
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
	this.set_$P($Clone($tmp1, {to_Int32}));
	$tmp2[$tmp3] = $Clone($tmp1, {to_Int32});
", valueTypes: true);
		}

		[Test]
		public void AssigningToMultiDimensionalArrayElementWorks() {
			AssertCorrect(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1, k = 2;
	// BEGIN
	arr[i, j] = k;
	// END
}",
@"	$MultidimArraySet($arr, $i, $j, $k);
");
		}

		[Test]
		public void AssigningToMultiDimensionalArrayElementWorksStruct() {
			AssertCorrect(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1, k = 2;
	// BEGIN
	arr[i, j] = k;
	// END
}",
@"	$MultidimArraySet($arr, $i, $j, $Clone($k, {to_Int32}));
", valueTypes: true);
		}

		[Test]
		public void AssigningToMultiDimensionalArrayEvaluatesExpressionsInTheCorrectOrderWhenUsingTheReturnValue() {
			AssertCorrect(
@"int[,] A() { return null; }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
public void M() {
	// BEGIN
	var x = A()[F1(), F2()] = F3();
	// END
}",
@"	var $tmp1 = this.$A();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	$MultidimArraySet($tmp1, $tmp2, $tmp3, $tmp4);
	var $x = $tmp4;
");
		}

		[Test]
		public void AssigningToMultiDimensionalArrayEvaluatesExpressionsInTheCorrectOrderWhenUsingTheReturnValueStruct() {
			AssertCorrect(
@"int[,] A() { return null; }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
public void M() {
	// BEGIN
	var x = A()[F1(), F2()] = F3();
	// END
}",
@"	var $tmp1 = this.$A();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	$MultidimArraySet($tmp1, $tmp2, $tmp3, $Clone($tmp4, {to_Int32}));
	var $x = $tmp4;
", valueTypes: true);
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

		[Test]
		public void AssigningToByRefLocalWorksStruct() {
			AssertCorrect(
@"int[] arr;
int i;
int F() { return 0; }
public void M(ref int i) {
	// BEGIN
	i = 1;
	// END
}",
@"	$i.$ = $Clone(1, {to_Int32});
", valueTypes: true);
		}

		[Test]
		public void NonVirtualAssignToBasePropertyWorks() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}
class D : B {
	public override int P { get; set; }
	public void M() {
		// BEGIN
		base.P = 10;
		// END
	}
}",
@"	$CallBase({bind_B}, '$set_P', [], [this, 10]);
", addSkeleton: false);
		}

		[Test]
		public void NonVirtualAssignToBasePropertyWorksStruct() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}
class D : B {
	public override int P { get; set; }
	public void M() {
		// BEGIN
		base.P = 10;
		// END
	}
}",
@"	$CallBase({bind_B}, '$set_P', [], [this, $Clone(10, {to_Int32})]);
", addSkeleton: false, valueTypes: true);
		}

		[Test]
		public void AssignmentToDynamicMemberWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d.someField = 123;
	// END
}",
@"	$d.someField = 123;
");
		}

		[Test]
		public void AssignmentToDynamicObjectWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d = 123;
	// END
}",
@"	$d = 123;
");
		}

		[Test]
		public void AssignmentToDynamicIndexerWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d[""X""] = 123;
	// END
}",
@"	$d['X'] = 123;
");
		}

		[Test]
		public void AssignmentToDynamicPropertyOfNonDynamicObject() {
			AssertCorrect(@"
public class SomeClass {
	public dynamic Value { get; set; }
}

class C {
	public void M() {
		var c = new SomeClass();
		// BEGIN
		c.Value = 1;
		// END
	}
}",
@"	$c.set_$Value(1);
", addSkeleton: false);
		}

		[Test]
		public void AssignmentToDynamicFieldOfNonDynamicObject() {
			AssertCorrect(@"
public class SomeClass {
	public dynamic Value;
}

class C {
	public void M() {
		var c = new SomeClass();
		// BEGIN
		c.Value = 1;
		// END
	}
}",
@"	$c.$Value = 1;
", addSkeleton: false);
		}
	}
}
