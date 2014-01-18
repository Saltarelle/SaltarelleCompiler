using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class InvocationTests : MethodCompilerTestBase {
		[Test]
		public void MethodInvocationWithNoArgumentsWorks() {
			AssertCorrect(
@"void F() {}
public void M() {
	// BEGIN
	F();
	// END
}",
@"	this.$F();
");
		}

		[Test]
		public void MethodInvocationWithArgumentsWorks() {
			AssertCorrect(
@"void F(int x, int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	this.$F($a, $b, $c);
");
		}

		[Test]
		public void MethodInvocationWithArgumentsWorksStruct() {
			AssertCorrect(
@"void F(int x, int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	this.$F($Clone($a, {to_Int32}), $Clone($b, {to_Int32}), $Clone($c, {to_Int32}));
", mutableValueTypes: true);
		}

		[Test]
		public void ExtensionMethodInvocationWorks() {
			AssertCorrect(
@"namespace N {
	public class X {}
	public static class Ex {
		public static void F(this X x, int y, int z) {}
	}
	class C {
		public void M() {
			X a = null;
			int b = 0, c = 0;
			// BEGIN
			a.F(b, c);
			// END
		}
	}
}",
@"	{sm_Ex}.$F($a, $b, $c);
", addSkeleton: false);
		}

		[Test]
		public void ExtensionMethodInvocationWithReorderedAndDefaultArgumentsWorks() {
			AssertCorrect(
@"namespace N {
	public class X {}
	public static class Ex {
		public static void F(this X x, int y, int z, int t = 0) {}
	}
	class C {
		public void M() {
			X a = null;
			int b = 0, c = 0;
			// BEGIN
			a.F(z: b, y: c);
			// END
		}
	}
}",
@"	{sm_Ex}.$F($a, $c, $b, 0);
", addSkeleton: false);
		}

		[Test]
		public void MethodInvocationWithDefaultArgumentsWorks() {
			AssertCorrect(
@"void F(int x, int y = 123, int z = 456) {}
public void M() {
	int a = 0;
	// BEGIN
	F(a);
	// END
}",
@"	this.$F($a, 123, 456);
");
		}

		[Test]
		public void StaticMethodInvocationWithArgumentsWorks() {
			AssertCorrect(
@"static void F(int x, int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	{sm_C}.$F($a, $b, $c);
");
		}

		[Test]
		public void InvokingBaseStaticMemberFromDerivedClassWorks() {
			AssertCorrect(@"
public class Class1 {
	public static void Test1() {}
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1();
		// END
	}
}",
@"	{sm_Class1}.$Test1();
", addSkeleton: false);
		}

		[Test]
		public void InvokingBaseStaticMemberThroughDerivedClassWorks() {
			AssertCorrect(@"
public class Class1 {
	public static void Test1() {}
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1();
		// END
	}
}",
@"	{sm_Class1}.$Test1();
", addSkeleton: false);
		}

		[Test]
		public void GenericMethodInvocationWorks() {
			AssertCorrect(
@"void F<T1, T2>(T1 x, int y, T2 z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$InstantiateGenericMethod(this.$F, {ga_Int32}, {ga_String}).call(this, $a, $b, $c);
");
		}

		[Test]
		public void GenericMethodInvocationWithIgnoreGenericArgumentsWorks() {
			AssertCorrect(
@"void F<T1, T2>(T1 x, int y, T2 z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	this.$F($a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
		}

		[Test]
		public void GenericMethodInvocationOnlyInvokesTargetOnce() {
			AssertCorrect(
@"class X { public void F<T1, T2>(T1 x, int y, T2 z) {} }
X FX() { return null; }
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	FX().F(a, b, c);
	// END
}",
@"	var $tmp1 = this.$FX();
	$InstantiateGenericMethod($tmp1.$F, {ga_Int32}, {ga_String}).call($tmp1, $a, $b, $c);
");
		}

		[Test]
		public void GenericMethodInvocationWorksWhenTheDeclaringTypeIsGeneric() {
			AssertCorrect(
@"class X<TX> { public void F<T1, T2>(T1 x, TX y, T2 z) {} }
X<byte> FX() { return null; }
public void M() {
	int a = 0;
	byte b = 0;
	string c = null;
	// BEGIN
	FX().F(a, b, c);
	// END
}",
@"	var $tmp1 = this.$FX();
	$InstantiateGenericMethod($tmp1.$F, {ga_Int32}, {ga_String}).call($tmp1, $a, $b, $c);
");
		}

		[Test]
		public void GenericMethodInvocationWorksForStaticMethod() {
			AssertCorrect(
@"static void F<T1, T2>(T1 x, int y, T2 z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$InstantiateGenericMethod({sm_C}.$F, {ga_Int32}, {ga_String}).call(null, $a, $b, $c);
");
		}

		[Test]
		public void NormalMethodInvocationWorksForReorderedAndDefaultArguments() {
			AssertCorrect(
@"void F(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	this.$F(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
");
		}

		[Test]
		public void NormalMethodInvocationWorksForReorderedAndDefaultArgumentsStruct() {
			AssertCorrect(
@"void F(int a = 1, uint b = 2, long c = 3, ushort d = 4, short e = 5, float f = 6, double g = 7) {}
ushort F1() { return 0; }
double F2() { return 0; }
float F3() { return 0; }
uint F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	this.$F($Clone(1, {to_Int32}), this.$F4(), $Clone(3, {to_Int64}), $tmp1, $Clone(5, {to_Int16}), $tmp3, $tmp2);
", mutableValueTypes: true);
		}

		[Test]
		public void NormalMethodInvocationWorksForReorderedAndDefaultArgumentsStruct2() {
			AssertCorrect(
@"struct S1 { public int a; }
struct S2 {}
struct S3 {}
void F(int a = 1, int b = 2, long c = 3, S1 d = default(S1), short e = 5, S3 f = default(S3), S2 g = default(S2)) {}
S2 F2() { return default(S2); }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: new S1 { a = 5 }, g: F2(), f: new S3(), b: F4());
	// END
}
",
@"	var $tmp1 = new {sm_S1}();
	$tmp1.$a = $Clone(5, {to_Int32});
	var $tmp2 = this.$F2();
	var $tmp3 = new {sm_S3}();
	this.$F($Clone(1, {to_Int32}), this.$F4(), $Clone(3, {to_Int64}), $tmp1, $Clone(5, {to_Int16}), $tmp3, $tmp2);
", mutableValueTypes: true);
		}


		[Test]
		public void NormalMethodInvocationWithRefAndOutArgumentsWorksForReorderedAndDefaultArguments() {
			AssertCorrect(
@"void F(int a, ref int b, out int c) { c = 0; }
int F1() { return 0; }
public void M() {
	int x = 0, y = 0;
	// BEGIN
	F(b: ref x, c: out y, a: F1());
	// END
}
",
@"	this.$F(this.$F1(), $x, $y);
");
		}

		[Test]
		public void NormalMethodInvocationWorksForReorderedAndDefaultArgumentsWithAdditionalStatements() {
			AssertCorrect(
@"void F(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
int F5(int x) { return 0; }
int F6(int x) { return 0; }
int P1 { get; set; }
int P2 { get; set; }
public void M() {
	// BEGIN
	F(d: F1(), g: F5(P1 = F2()), f: F6(P2 = F3()), b: F4());
	// END
}
",
@"	var $tmp2 = this.$F1();
	var $tmp1 = this.$F2();
	this.set_$P1($tmp1);
	var $tmp4 = this.$F5($tmp1);
	var $tmp3 = this.$F3();
	this.set_$P2($tmp3);
	var $tmp5 = this.$F6($tmp3);
	this.$F(1, this.$F4(), 3, $tmp2, 5, $tmp5, $tmp4);
");
		}

		[Test]
		public void ParametersAreEvaluatedLeftToRightWhenReorderingParameters1() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), a: F2(), c: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	this.$F($tmp2, this.$F4(), $tmp3, $tmp1);
");
		}

		[Test]
		public void ParametersAreEvaluatedLeftToRightWhenReorderingParameters2() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(c: F1(), a: 1, d: 2, b: F2());
	// END
}
",
@"	var $tmp1 = this.$F1();
	this.$F(1, this.$F2(), $tmp1, 2);
");
		}
		
		[Test]
		public void PassingRefAndOutParametersToNormalMethodWorks() {
			AssertCorrect(
@"void F(ref int x, out int y, int z) { y = 0; }
public void M(ref int a, ref int b, ref int c) {
	// BEGIN
	F(ref a, out b, c);
	// END
}
",
@"	this.$F($a, $b, $c.$);
");
		}

		[Test]
		public void PassingRefAndOutParametersToNormalMethodWorksStruct() {
			AssertCorrect(
@"void F(ref int x, out int y, int z) { y = 0; }
public void M(ref int a, ref int b, ref int c) {
	// BEGIN
	F(ref a, out b, c);
	// END
}
",
@"	this.$F($a, $b, $Clone($c.$, {to_Int32}));
", mutableValueTypes: true);
		}

		[Test]
		public void RefAndOutParametersAreNotSubjectToReordering() {
			AssertCorrect(
@"void F(ref int x, out int y, int z) { y = 0; }
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(z: a = b = c, x: ref a, y: out b);
	// END
}
",
@"	this.$F($a, $b, $a.$ = $b.$ = $c);
");
		}

		[Test]
		public void PassingAFieldByReferenceGivesAnError() {
			var er = new MockErrorReporter(false);
			CompileMethod(@"
				public int f;
				public void OtherMethod(int a, ref int b) {}
				public void M(int x) {
					OtherMethod(x, ref f);
				}
			", errorReporter: er);

			er.AllMessages.Where(m => m.Severity == MessageSeverity.Error).Should().NotBeEmpty();
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a, b];
	// END
}",
@"	var $i = this.get_$Item($a, $b);
");
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodWorksStruct() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a, b];
	// END
}",
@"	var $i = this.get_$Item($Clone($a, {to_Int32}), $Clone($b, {to_Int32}));
", mutableValueTypes: true);
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodEvaluatesArgumentsInOrder() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
int P { get; set; }
public void M() {
	int a = 0;
	// BEGIN
	int i = this[P, P = a];
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($a);
	var $i = this.get_$Item($tmp1, $a);
");
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodWorksWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	int i = this[d: F1(), g: F2(), f: F3(), b: F4()];
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $i = this.get_$Item(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
");
		}

		[Test]
		public void ReadingPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrect(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a];
	// END
}",
@"	var $i = this[$a];
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void DelegateInvocationWorks() {
			AssertCorrect(
@"public void M() {
	Action<int, string> f = null;
	int a = 0;
	string b = null;
	// BEGIN
	f(a, b);
	// END
}",
@"	$f($a, $b);
");
		}

		[Test]
		public void DelegateInvocationWorksStruct() {
			AssertCorrect(
@"public void M() {
	Action<int, string> f = null;
	int a = 0;
	string b = null;
	// BEGIN
	f(a, b);
	// END
}",
@"	$f($a, $b);
");
		}

		[Test]
		public void DelegateInvocationWorksForReorderedAndDefaultArguments() {
			AssertCorrect(
@"delegate void D(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7);
void F(int f1, int f2, int f3, int f4, int f5, int f6, int f7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	D d;
	d = F;
	// BEGIN
	d(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}

",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	$d(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
");		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentWorks() {
			AssertCorrect(
@"void F(int x, int y, string z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	{sm_C}.$F(this, $a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentWorksStruct() {
			AssertCorrect(
@"void F(int x, int y, string z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	{sm_C}.$F(this, $Clone($a, {to_Int32}), $Clone($b, {to_Int32}), $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name), GetTypeSemantics = t => TypeScriptSemantics.MutableValueType(t.Name) });
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentDeclaredInGenericTypeWorks() {
			AssertCorrect(
@"class X<TX> { public void F(TX x, int y, string z) {} }
X<int> FX() { return null; }
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	FX().F(a, b, c);
	// END
}",
@"	sm_$InstantiateGenericType({X}, {ga_Int32}).$F(this.$FX(), $a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void GenericStaticMethodWithThisAsFirstArgumentWorks() {
			AssertCorrect(
@"void F<T1, T2>(T1 x, int y, T2 z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$InstantiateGenericMethod({sm_C}.$F, {ga_Int32}, {ga_String}).call(null, this, $a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentWorksWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"void F(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	{sm_C}.$F(this, 1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeExpressionWorks() {
			AssertCorrect(
@"class X<T1> { public class Y<T2> { public int F<T3>(T1 x, T2 y, T3 z) { return 0; } } }
public void M() {
	X<int>.Y<byte> o = null;
	int a = 0;
	byte b = 0;
	string c = null;
	// BEGIN
	int x = o.F(a, b, c);
	// END
}",
@"	var $x = _({sm_Object})._({ga_Int32})._({ga_Byte})._({ga_String})._($o)._($a)._($b)._($c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({$System.Object})._({T1})._({T2})._({T3})._({this})._({x})._({y})._({z})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}


		[Test]
		public void InvokingVoidMethodImplementedAsInlineCodeWithMultipleStatementsWorks() {
			AssertCorrect(
@"class X<T1> { public class Y<T2> { public void F<T3>(T1 x, T2 y, T3 z) {} } }
public void M() {
	X<int>.Y<byte> o = null;
	int a = 0;
	byte b = 0;
	string c = null;
	// BEGIN
	o.F(a, b, c);
	// END
}",
@"	if ({ga_Int32}) {
		{ga_Byte};
	}
	else {
		{ga_String};
	}
	var $$ = _($o)._($a)._($b)._($c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("if ({T1}) {T2}; else {T3}; var $$ = _({this})._({x})._({y})._({z})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeWithCustomNonVirtualCodeWorks() {
			AssertCorrect(
@"class B<T1> {
	public virtual void F<T2>(T1 x, T2 y) {}
}
class D : B<int> {
	public override void M() {
		// BEGIN
		base.F(1, ""X"");
		// END
	}
}",
@"	_({ga_Int32})._({ga_String})._(this)._(1)._('X');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("X", nonVirtualInvocationLiteralCode: "_({T1})._({T2})._({this})._({x})._({y})") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSkeleton: false);
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeWorksWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"void F(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	_(1)._(this.$F4())._(3)._($tmp1)._(5)._($tmp3)._($tmp2);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({c})._({d})._({e})._({f})._({g})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeCreatesTemporariesForParametersUsedTwice() {
			AssertCorrect(
@"void F(int a, int b, int c, int d, int e) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
class X { public int x; }
public void M() {
	int a = 0;
	X x;
	// BEGIN
	F(F1(), a, F2(), x.x, F3());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	_($tmp1)._($a)._($tmp2)._($x.$x)._($tmp2)._($x.$x)._(this.$F3());
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({c})._({d})._({c})._({d})._({e})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeCreatesTemporariesForParametersUsedTwiceWhenArgumentsReordered() {
			AssertCorrect(
@"void F(int a, int b, int c, int d, int e) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
class X { public int x; }
public void M() {
	int a = 0;
	X x;
	// BEGIN
	F(F1(), c: F2(), b: a, d: x.x, e: F3());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	_($tmp1)._($a)._($tmp2)._($x.$x)._($tmp2)._($x.$x)._(this.$F3());
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({c})._({d})._({c})._({d})._({e})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeCreatesTemporariesForThisIfRequired1() {
			AssertCorrect(
@"void F(int a) {}
int F1() { return 0; }
C X() { return null; }
public void M() {
	// BEGIN
	X().F(F1());
	// END
}
",
@"	var $tmp1 = this.$X();
	var $tmp2 = this.$F1();
	_($tmp1)._($tmp2)._($tmp2);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({a})._({a})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeCreatesTemporariesForThisIfRequired2() {
			AssertCorrect(
@"void F(int a, int b) {}
int F1() { return 0; }
int F2() { return 0; }
C X() { return null; }
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	X().F(F1(), a);
	// END
}
",
@"	var $tmp1 = this.$X();
	_($tmp1)._($tmp1)._(this.$F1())._($a);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({this})._({a})._({b})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesThisIfNotUsedInTheInlineCode() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
class X { public int x; }
public void M() {
	int a = 0;
	// BEGIN
	F(F1(), F2(), F3(), a);
	// END
}
",
@"	var $tmp1 = this.$F1();
	this.$F2();
	_($tmp1)._(this.$F3())._($a);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({c})._({d})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesArgumentNotUsedInTheInlineCode() {
			AssertCorrect(
@"void F(int a) {}
int F1() { return 0; }
C X() { return null; }
public void M() {
	// BEGIN
	X().F(F1());
	// END
}
",
@"	this.$X();
	_(this.$F1());
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesArgumentsLeftTorightWhenTheInlineCodeDoesNotReorderArguments() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d:F1(), a:F2(), c:F3(), b:F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	_($tmp2)._(this.$F4())._($tmp3)._($tmp1);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({c})._({d})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesArgumentsLeftTorightWhenTheInlineCodeReordersArguments() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(F1(), F2(), F3(), F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	_($tmp2)._(this.$F4())._($tmp3)._($tmp1);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({b})._({d})._({c})._({a})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesArgumentsLeftToRightWhenTheInlineCodeReorderingUndoesTheNamedArgumentReordering() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(b: F1(), d: F2(), c: F3(), a: F4());
	// END
}
",
@"	_(this.$F1())._(this.$F2())._(this.$F3())._(this.$F4());
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({b})._({d})._({c})._({a})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeEvaluatesArgumentsLeftTorightWhenTheInlineCodeReordersThis() {
			AssertCorrect(
@"void F(int a, int b, int c, int d) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
C X() { return null; }
public void M() {
	// BEGIN
	X().F(F1(), F2(), F3(), F4());
	// END
}
",
@"	var $tmp1 = this.$X();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	_($tmp3)._(this.$F4())._($tmp1)._($tmp4)._($tmp2);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({b})._({d})._({this})._({c})._({a})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeWorksWithReorderedAndDefaultArgumentsWorksWhenTheInlineCodeMethodAlsoReordersArguments() {
			AssertCorrect(
@"void F(int a, int b, int c, int d, int e = 2) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
C X() { return null; }
public void M() {
	// BEGIN
	X().F(d: F1(), c: F2(), a: F3(), b: F4());
	// END
}
",
@"	var $tmp1 = this.$X();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp4 = this.$F3();
	_(this.$F4())._($tmp2)._($tmp1)._($tmp3)._($tmp4)._(2);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({b})._({d})._({this})._({c})._({a})._({e})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { UnusableMethod(); } }" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "UnusableMethod" ? MethodScriptSemantics.NotUsableFromScript() : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableMethod")));
		}

		[Test]
		public void InvokingOverriddenMethodWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
	public override void F(int x, int y) {}
	public void M() {
		int a = 0, b = 0, c = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	this.$F($a, $b);
	$CallBase({bind_B}, '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingMethodOverriddenFromGenericClassWorks() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D : B<string> {
	public override void F(string x, int y) {}
	public void M() {
		string a = null, c = null;
		int b = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	this.$F($a, $b);
	$CallBase(bind_$InstantiateGenericType({B}, {ga_String}), '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D<T2> : B<T2> {
	public override void F(T2 x, int y) {}
	public void M() {
		T2 a = default(T2), c = default(T2);
		int b = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	this.$F($a, $b);
	$CallBase(bind_$InstantiateGenericType({B}, $T2), '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingGenericOverriddenMethodWorks() {
			AssertCorrect(
@"class B {
	public virtual void F<T>(T x, int y) {}
}
class D : B {
	public override void F<U>(U x, int y) {}
	public void M() {
		int a = 0, b = 0, c = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	$InstantiateGenericMethod(this.$F, {ga_Int32}).call(this, $a, $b);
	$CallBase({bind_B}, '$F', [{ga_Int32}], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingGenericMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F<U>(U x, int y) {}
}
class D<T2> : B<T2> {
	public override void F<S>(S x, int y) {}
	public void M() {
		int a = 0, b = 0, c = 0, d = 0;
		// BEGIN
		F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	$InstantiateGenericMethod(this.$F, {ga_Int32}).call(this, $a, $b);
	$CallBase(bind_$InstantiateGenericType({B}, $T2), '$F', [{ga_Int32}], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingBaseVersionOfMethodInheritedFromGrandParentWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
}
class D2 : D {
	public override void F(int x, int y) {}

	public void M() {
		int a = 0, b = 0, c = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	this.$F($a, $b);
	$CallBase({bind_B}, '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void InvokingBaseVersionOfMethodDefinedInGrandParentAndOverriddenInParentWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
	public virtual void F(int x, int y) {}
}
class D2 : D {
	public override void F(int x, int y) {}

	public void M() {
		int a = 0, b = 0, c = 0, d = 0;
		// BEGIN
		this.F(a, b);
		base.F(c, d);
		// END
	}
}
",
@"	this.$F($a, $b);
	$CallBase({bind_D}, '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}

		[Test]
		public void CannotUseNotUsableTypeAsAGenericArgument() {
			var nc = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {}
class C {
	public void F1<T>() {}
	public void M() {
		F1<C1>();
	}
}" }, metadataImporter: nc, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("generic argument") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("F1"));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {}
interface I1<T> {}
class C {
	public void F1<T>() {}
	public void M() {
		F1<I1<I1<C1>>>();
	}
}" }, metadataImporter: nc, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("generic argument") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("F1"));
		}

		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, 59, 12, 4);
	// END
}",
@"	this.$F(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInExpandedFormWorksStruct() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, 59, 12, 4);
	// END
}",
@"	this.$F($Clone(4, {to_Int32}), $Clone(8, {to_Int32}), [$Clone(59, {to_Int32}), $Clone(12, {to_Int32}), $Clone(4, {to_Int32})]);
", mutableValueTypes: true);
		}


		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	this.$F(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInNonExpandedFormWorksStruct() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	this.$F($Clone(4, {to_Int32}), $Clone(8, {to_Int32}), [$Clone(59, {to_Int32}), $Clone(12, {to_Int32}), $Clone(4, {to_Int32})]);
", mutableValueTypes: true);
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, 59, 12, 4);
	// END
}",
@"	this.$F(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInExpandedFormWorksStruct() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	// BEGIN
	F(4, 8, 59, 12, 4);
	// END
}",
@"	this.$F($Clone(4, {to_Int32}), $Clone(8, {to_Int32}), $Clone(59, {to_Int32}), $Clone(12, {to_Int32}), $Clone(4, {to_Int32}));
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F"), GetTypeSemantics = t => TypeScriptSemantics.MutableValueType(t.Name) });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C {
	public void F1(int x, int y, params int[] args) {}
	public void F2(int x, params int[] args) {}
	public void F3(params int[] args) {}
	public void M() {
		C c = null;
		var args = new[] { 59, 12, 4 };
		// BEGIN
		F1(4, 8, args);
		c.F2(42, args);
		F3(args);
		F1(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"	this.$F1.apply(this, [4, 8].concat($args));
	$c.$F2.apply($c, [42].concat($args));
	this.$F3.apply(this, $args);
	this.$F1(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name.StartsWith("F")) });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksStruct() {
			AssertCorrect(
@"class C {
	public void F1(int x, int y, params int[] args) {}
	public void F2(int x, params int[] args) {}
	public void F3(params int[] args) {}
	public void M() {
		C c = null;
		var args = new[] { 59, 12, 4 };
		// BEGIN
		F1(4, 8, args);
		c.F2(42, args);
		F3(args);
		F1(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"	this.$F1.apply(this, [$Clone(4, {to_Int32}), $Clone(8, {to_Int32})].concat($args));
	$c.$F2.apply($c, [$Clone(42, {to_Int32})].concat($args));
	this.$F3.apply(this, $args);
	this.$F1($Clone(4, {to_Int32}), $Clone(8, {to_Int32}), $Clone(59, {to_Int32}), $Clone(12, {to_Int32}), $Clone(4, {to_Int32}));
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name.StartsWith("F")), GetTypeSemantics = t => TypeScriptSemantics.MutableValueType(t.Name) });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormDoesNotEvaluateTargetTwice() {
			AssertCorrect(
@"public C X() { return null; }
public void F(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	X().F(4, args);
	X().F(4, new[] { 59, 12, 4 });
	// END
}",
@"	var $tmp1 = this.$X();
	$tmp1.$F.apply($tmp1, [4].concat($args));
	this.$X().$F(4, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksForStaticMethod() {
			AssertCorrect(
@"public static void F(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	F(4, args);
	F(4, new[] { 59, 12, 4 });
	// END
}",
@"	{sm_C}.$F.apply(null, [4].concat($args));
	{sm_C}.$F(4, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksForGenericMethod() {
			AssertCorrect(
@"public void F1<T>(int x, params int[] args) {}
public static void F2<T>(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	F1<int>(4, args);
	F2<int>(4, args);
	F1<int>(4, new[] { 59, 12, 4 });
	F2<int>(4, new[] { 59, 12, 4 });
	// END
}",
@"	$InstantiateGenericMethod(this.$F1, {ga_Int32}).apply(this, [4].concat($args));
	$InstantiateGenericMethod({sm_C}.$F2, {ga_Int32}).apply(null, [4].concat($args));
	$InstantiateGenericMethod(this.$F1, {ga_Int32}).call(this, 4, 59, 12, 4);
	$InstantiateGenericMethod({sm_C}.$F2, {ga_Int32}).call(null, 4, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name.StartsWith("F")) });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksForBaseCall() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, params int[] args) {}
}
class C : B{
	public override void F(int x, params int[] args) {}
	public void M() {
		var args = new[] { 59, 12, 4 };
		// BEGIN
		base.F(4, args);
		base.F(4, new[] { 59, 12, 4 });
		// END
	}
}",
@"	$CallBase({bind_B}, '$F', [], [this, 4, $args]);
	$CallBase({bind_B}, '$F', [], [this, 4, [59, 12, 4]]);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name.StartsWith("F")) }, addSkeleton: false);
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksForNonGenericStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"public void F(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	F(4, args);
	F(4, new[] { 59, 12, 4 });
	// END
}",
@"	{sm_C}.$F.apply(null, [this, 4].concat($args));
	{sm_C}.$F(this, 4, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F", expandParams: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorksForGenericStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"public void F<T>(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	F<int>(4, args);
	F<int>(4, new[] { 59, 12, 4 });
	// END
}",
@"	$InstantiateGenericMethod({sm_C}.$F, {ga_Int32}).apply(null, [this, 4].concat($args));
	$InstantiateGenericMethod({sm_C}.$F, {ga_Int32}).call(null, this, 4, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F", expandParams: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingParamArrayDelegateThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"public delegate void F(int x, int y, params int[] args);
public void M() {
	F f = null;
	// BEGIN
	f(4, 8, 59, 12, 4);
	// END
}",
@"	$f(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void InvokingParamArrayDelegateThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public delegate void F(int x, int y, params int[] args);
public void M() {
	F f = null;
	var args = new[] { 59, 12, 4 };
	// BEGIN
	f(4, 8, args);
	f(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	$f.apply(null, [4, 8].concat($args));
	$f(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void InvokingParamArrayDelegateWithBindThisToFirstParameterThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public delegate void F(int x, int y, params int[] args);
public void M() {
	F f = null;
	var args = new[] { 59, 12, 4 };
	// BEGIN
	f(4, 8, args);
	f(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	$f.apply(4, [8].concat($args));
	$f.call(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true, expandParams: true) });
		}

		[Test]
		public void InvokingDynamicMemberWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var x = d.someMember(123, ""456"");
	// END
}",
@"	var $x = $d.someMember(123, '456');
");
		}

		[Test]
		public void InvokingDynamicObjectWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d(123, ""456"");
	// END
}",
@"	var $i = $d(123, '456');
");
		}

		[Test]
		public void CanIndexDynamicMember() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someMember[123];
	// END
}",
@"	var $i = $d.someMember[123];
");
		}

		[Test]
		public void CanIndexDynamicObject() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d[123];
	// END
}",
@"	var $i = $d[123];
");
		}

		[Test]
		public void IndexingDynamicMemberWithMoreThanOneArgumentGivesAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C {
	public void M() {
		dynamic d = null;
		var i = d.someMember[123, 456];
	}
}" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("one argument")));
		}

		[Test]
		public void IndexingDynamicObjectWithMoreThanOneArgumentGivesAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C {
	public void M() {
		dynamic d = null;
		var i = d[123, 456];
	}
}" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("one argument")));
		}

		[Test]
		public void InvokingDynamicMemberEnsuresArgumentsAreEvaluatedInTheCorrectOrder() {
			AssertCorrect(
@"public int P { get; set; }
public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	dynamic d;
	// BEGIN
	var x = d(F1(), P = F2());
	// END
}",
@"	var $tmp2 = this.$F1();
	var $tmp1 = this.$F2();
	this.set_$P($tmp1);
	var $x = $d($tmp2, $tmp1);
");
		}

		[Test]
		public void UsingNamedArgumentInInvocationOfDynamicMemberIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void M() {
		dynamic d = null;
		d.someMethod(a: 123, b: 465);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("named argument") && er.AllMessages[0].FormattedMessage.Contains("Dynamic"));
		}

		[Test]
		public void UsingNamedArgumentInInvocationOfDynamicObjectIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void M() {
		dynamic d = null;
		d(a: 123, b: 465);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("named argument") && er.AllMessages[0].FormattedMessage.Contains("Dynamic"));
		}

		[Test]
		public void InvokingMemberWithDynamicArgumentWorks() {
			AssertCorrect(
@"class X {
	public int F(int a, string b) { return 0; }
	public int F(int a, int b) { return 0; }
}
public void M() {
	dynamic d = null;
	var x = new X();
	// BEGIN
	var a = x.F(123, d);
	// END
}",
@"	var $a = $x.$F(123, $d);
");
		}

		[Test]
		public void InvokingStaticMethodWithDynamicArgumentWorks() {
			AssertCorrect(
@"class Other {
	public static void S(int i) {}
	public static void S(string s) {}
}
public static void S(int i) {}
public static void S(string s) {}
public void M() {
	dynamic d = null;
	// BEGIN
	S(d);
	Other.S(d);
	// END
}",
@"	{sm_C}.$S($d);
	{sm_Other}.$S($d);
");
		}

		[Test]
		public void InvokingIndexerWithDynamicArgumentWorksWhenOnlyOneMemberIsApplicable() {
			AssertCorrect(
@"class X {
	public int this[int a, string b] { get { return 0; } set {} }
	public int this[int a] { get { return 0; } set {} }
}

public void M() {
	dynamic d = null;
	var x = new X();
	// BEGIN
	var a = x[123, d];
	// END
}",
@"	var $a = $x.get_$Item(123, $Cast($d, {ct_String}));
");
		}

		[Test]
		public void InvokingIndexerWithDynamicArgumentIsAnErrorWhenMoreThanOneMemberIsApplicable() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public int this[int a, string b] { get { return 0; } set {} }
	public int this[int a, int b] { get { return 0; } set {} }
}
class C {
	public void M() {
		dynamic d = null;
		var x = new X();
		// BEGIN
		var a = x[123, d];
		// END
	}
}" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.FormattedMessage.Contains("one argument")));
		}

		[Test]
		public void InvokingMethodWithDynamicArgumentIsAnErrorWhenAllMethodsInTheGroupDoNotHaveTheSameScriptName() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public int F(int a, string b) { return 0; }
	public int F(int a, int b) { return 0; }
}
class C {
	public void M() {
		dynamic d = null;
		var x = new X();
		// BEGIN
		var a = x.F(123, d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Name + "$" + string.Join("$", m.Parameters.Select(p => p.Type.Name))) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.FormattedMessage.Contains("same script name")));
		}

		[Test]
		public void InvokingMethodWithDynamicArgumentIsAnErrorWhenAMethodInTheGroupIsNotImplementedAsANormalMethod() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public int F(int a, string b) { return 0; }
	public int F(int a, int b) { return 0; }
}
class C {
	public void M() {
		dynamic d = null;
		var x = new X();
		// BEGIN
		var a = x.F(123, d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" && m.Parameters[1].Type.Name == "String" ? MethodScriptSemantics.InlineCode("X") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.FormattedMessage.Contains("not a normal method")));
		}

		[Test]
		public void CallingBaseWorksWhenTheBaseMethodIsAnOverride() {
			AssertCorrect(
@"using System.Collections;
class Base {
	protected virtual void M(JsDictionary d) {
	}
}
class Middle : Base {
	protected override void M(JsDictionary d) {
		// BEGIN
		base.M(d);
		// END
	}
}
class Derived : Middle {
	protected override void M(JsDictionary d) {
		// BEGIN
		base.M(d);
		// END
	}
}",
@"	$CallBase({bind_Middle}, '$M', [], [this, $d]);
", addSkeleton: false);

		}

		[Test]
		public void InvokeDelegateWithBindThisToFirstParameterWorks() {
			AssertCorrect(
@"int i;
public void M() {
	Func<int, int, int> f = (_this, x) => _this + x + i;
	int a = 1, b = 2;
	// BEGIN
	f(a, b);
	// END
}",
@"	$f.call($a, $b);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true) });
		}

		[Test]
		public void InvokingAGenericMethodImplementedAsANormalMethodWithAnIgnoredGenericArgumentFromTypeIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public void F<T>(T t) {}
}
class C1<T> {
	public void M(T t) {
		new X().F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void InvokingAGenericMethodImplementedAsANormalMethodWithAnIgnoredGenericArgumentFromMethodIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public void F<T>(T t) {}
}
class C1 {
	public void M<T>(T t) {
		new X().F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Name, ignoreGenericArguments: m.Name == "M") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("method C1.M")));
		}

		[Test]
		public void InvokingMethodImplementedAsANormalMethodWithIgnoredGenericArgumentsIsNotAnErrorIfTheMethodAlsoIgnoresGenericArguments() {
			AssertCorrect(
@"class X {
	public void F<T1, T2>(T1 t1, T2 t2) {}
}
class C<T1> {
	public void M<T2>(T1 t1, T2 t2) {
		// BEGIN
		new X().F(t1, t2);
		// END
	}
}",
@"	(new {sm_X}()).$F($t1, $t2);
", metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.Name, ignoreGenericArguments: true), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
		}

		[Test]
		public void InvokingAGenericMethodImplementedAsAStaticMethodWithThisAsFirstArgumentMethodWithAnIgnoredGenericArgumentFromTypeIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public void F<T>(T t) {}
}
class C1<T> {
	public void M(T t) {
		new X().F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void InvokingAGenericMethodImplementedAsAStaticMethodWithThisAsFirstArgumentMethodWithAnIgnoredGenericArgumentFromMethodIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public void F<T>(T t) {}
}
class C1 {
	public void M<T>(T t) {
		new X().F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name, ignoreGenericArguments: true) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("method C1.M")));
		}

		[Test]
		public void InvokingAGenericMethodImplementedAsAStaticMethodWithThisAsFirstArgumentMethodWithATypeThatNeedsAnIgnoredGenericArgumentFromTypeIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X<T> {
	public void F(X<T> t) {}
}
class C1<T> {
	public void M(X<T> x) {
		x.F(x);
	}
}" }, metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: t.Name == "C1"), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}


		[Test]
		public void InvokingAGenericMethodImplementedAsAStaticMethodWithThisAsFirstArgumentMethodWithATypeThatNeedsAnIgnoredGenericArgumentFromMethodIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X<T> {
	public void F(X<T> t) {}
}
class C1 {
	public void M<T>(X<T> x) {
		x.F(x);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name, ignoreGenericArguments: true) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("method C1.M")));
		}

		[Test]
		public void InvokingMethodImplementedAsAStaticMethodWithThisAsFirstArgumentWithIgnoredGenericArgumentsIsNotAnErrorIfTheMethodAlsoIgnoresGenericArguments() {
			AssertCorrect(
@"class X {
	public void F<T1, T2>(T1 t1, T2 t2) {}
}
class C<T1> {
	public void M<T2>(T1 t1, T2 t2) {
		// BEGIN
		new X().F(t1, t2);
		// END
	}
}",
@"	{sm_X}.$F(new {sm_X}(), $t1, $t2);
", metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.Name, ignoreGenericArguments: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F", ignoreGenericArguments: true) : MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
		}

		[Test]
		public void InvokingAGenericInlineCodeMethodWithAnIgnoredGenericArgumentFromTypeIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public static void F<T>(T t) {}
}
class C1<T> {
	public void M(T t) {
		X.F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({T})._({t})") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void InvokingAGenericInlineCodeMethodWithAnIgnoredGenericArgumentFromMethodIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public static void F<T>(T t) {}
}
class C1 {
	public void M<T>(T t) {
		X.F(t);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Name, ignoreGenericArguments: m.Name == "M") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("method C1.M")));
		}

		[Test]
		public void InvokingMethodInlineCodeMethodWithIgnoredGenericArgumentsIsNotAnErrorIfTheMethodDoesNotUseTheGenericArgument() {
			AssertCorrect(
@"class X {
	public static void F<T1, T2>(T1 t1, T2 t2) {}
}
class C<T1> {
	public void M<T2>(T1 t1, T2 t2) {
		// BEGIN
		X.F(t1, t2);
		// END
	}
}",
@"	_($t1)._($t2);
", metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.Name, ignoreGenericArguments: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({t1})._({t2})") : MethodScriptSemantics.NormalMethod(m.Name, ignoreGenericArguments: true) });
		}

		[Test]
		public void InvokingAMethodOfAGenericMethodWithAnUnavailableTypeParameterIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X<T> {
	public static void F() {}
}
class C1 {
	public void M<T>() {
		X<T>.F();
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: m.Name == "M") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("method C1.M")));
		}

		[Test]
		public void InvokingInlineCodeMethodThatExpandsParamArrayInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void F1(params int[] args) {}
	public void M() {
		int[] a = new[] { 1, 2, 3 };
		F1(a);
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F1" ? MethodScriptSemantics.InlineCode("_({*args})") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7525 && m.FormattedMessage.Contains("C1.F1") && m.FormattedMessage.Contains("params parameter expanded")));
		}

		[Test]
		public void InvokingInlineCodeMethodInNonExpandedFormUsesTheNonExpandedFormPattern() {
			AssertCorrect(
@"class C1 {
	public void F(params int[] args) {}
	public void M() {
		int[] a = new[] { 1, 2, 3 };
		// BEGIN
		F(a);
		// END
	}
}",
@"	_2($a);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({*args})", nonExpandedFormLiteralCode: "_2({args})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void ParamArrayWithSideEffects() {
			AssertCorrect(@"
public class C {
	object Id;

	static int Y(params object[] args) { return 0; }

	void M() {
		// BEGIN
		Y(1, Y(new C() { Id = 2 }), 3);
		// END
	}
}",
@"	var $tmp1 = new {sm_C}();
	$tmp1.$Id = 2;
	{sm_C}.$Y([1, {sm_C}.$Y([$tmp1]), 3]);
", addSkeleton: false, runtimeLibrary: new MockRuntimeLibrary { Upcast = (a, b, c, d) => a });
		}

		[Test]
		public void ParamArrayWithSideEffectsExpandParams() {
			AssertCorrect(@"
public class C {
	object Id;

	static int Y(params object[] args) { return 0; }

	void M() {
		// BEGIN
		Y(1, Y(new C() { Id = 2 }), 3);
		// END
	}
}",
@"	var $tmp1 = new {sm_C}();
	$tmp1.$Id = 2;
	{sm_C}.$Y(1, {sm_C}.$Y($tmp1), 3);
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: true) }, runtimeLibrary: new MockRuntimeLibrary { Upcast = (a, b, c, d) => a });
		}

		[Test]
		public void InvokingInstanceMemberOfMultidimensionalStructArrayWorks() {
			AssertCorrect(@"
struct S { public void F(int a) {} }
void M() {
	S[,] arr = null;
	// BEGIN
	arr[3, 5].F(2);
	// END
}
",
@"	$MultidimArrayGet($arr, 3, 5).$F($Clone(2, {to_Int32}));
", mutableValueTypes: true);
		}

		[Test]
		public void ReadonlyValueTypesAreClonedWhenTheyAreInvocationTargets() {
			AssertCorrect(@"
struct S { public void M() {} }
readonly S s;
void M() {
	// BEGIN
	s.M();
	// END
}",
@"	$Clone(this.$s, {to_S}).$M();
", mutableValueTypes: true);
		}

		[Test]
		public void ReadonlyValueTypesAreClonedWhenTheyAreInvocationTargetsNested() {
			AssertCorrect(@"
struct S1 { public S3 s; }
struct S2 { public readonly S3 s; }
struct S3 { public void M() {} }
class C1 {
	public S1 s1;
	public readonly S1 s1r;
	public S2 s2;
}
C1 c1 = null;
readonly C1 c1r = null;
public S1 s1;
readonly S1 s1r;
S2 s2;

void M() {
	S1 s1v;
	S2 s2v;
	C1 c1v;
	// BEGIN
	s1.s.M();
	s1r.s.M();
	s2.s.M();
	s1v.s.M();
	s2v.s.M();
	c1.s1.s.M();
	c1.s1r.s.M();
	c1.s2.s.M();
	c1r.s1.s.M();
	c1r.s1r.s.M();
	c1r.s2.s.M();
	c1v.s1.s.M();
	c1v.s1r.s.M();
	c1v.s2.s.M();
	// END
}",
@"	this.$s1.$s.$M();
	$Clone(this.$s1r.$s, {to_S3}).$M();
	$Clone(this.$s2.$s, {to_S3}).$M();
	$s1v.$s.$M();
	$Clone($s2v.$s, {to_S3}).$M();
	this.$c1.$s1.$s.$M();
	$Clone(this.$c1.$s1r.$s, {to_S3}).$M();
	$Clone(this.$c1.$s2.$s, {to_S3}).$M();
	this.$c1r.$s1.$s.$M();
	$Clone(this.$c1r.$s1r.$s, {to_S3}).$M();
	$Clone(this.$c1r.$s2.$s, {to_S3}).$M();
	$c1v.$s1.$s.$M();
	$Clone($c1v.$s1r.$s, {to_S3}).$M();
	$Clone($c1v.$s2.$s, {to_S3}).$M();
", mutableValueTypes: true);
		}
	}
}
