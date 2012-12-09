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

			er.AllMessagesText.Where(m => m.StartsWith("Error:")).Should().NotBeEmpty();
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
		public void InvokingMethodImplementedAsInlineCodeWorks() {
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
@"	_({sm_Int32})._({sm_Byte})._({sm_String})._($o)._($a)._($b)._($c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({T1})._({T2})._({T3})._({this})._({x})._({y})._({z})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	_({sm_Int32})._({sm_String})._(this)._(1)._('X');
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
		public void InvokingMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { UnusableMethod(); } }" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "UnusableMethod" ? MethodScriptSemantics.NotUsableFromScript() : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableMethod")));
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
	$CallBase(bind_$InstantiateGenericType({B}, ga_$T2), '$F', [], [this, $c, $d]);
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
	$CallBase(bind_$InstantiateGenericType({B}, ga_$T2), '$F', [{ga_Int32}], [this, $c, $d]);
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("generic argument") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("F1"));

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
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("generic argument") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("F1"));
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
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void F(int x, int y, params int[] args) {}
	public void M() {
		F(4, 8, new[] { 59, 12, 4 });
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("C1.F") && er.AllMessagesText[0].Contains("expanded form"));
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
		public void InvokingParamArrayDelegateThatExpandsArgumentsInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public delegate void F(int x, int y, params int[] args);
	public void M() {
		F delegateVar = null;
		delegateVar(4, 8, new[] { 59, 12, 4 });
	}
}" }, metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) }, errorReporter: er);


			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("C1.F") && er.AllMessagesText[0].Contains("expanded form"));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("one argument")));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("one argument")));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("named argument") && er.AllMessagesText[0].Contains("Dynamic"));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("named argument") && er.AllMessagesText[0].Contains("Dynamic"));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("one argument")));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("same script name")));
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

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("not a normal method")));
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
	}
}
