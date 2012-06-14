using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
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
@"	{Ex}.$F($a, $b, $c);
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
@"	{Ex}.$F($a, $c, $b, 0);
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
@"	{C}.$F($a, $b, $c);
");
		}

		[Test]
		public void GlobalStaticMethodInvocationWithArgumentsWorks() {
			AssertCorrect(
@"static void F(int x, int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$F($a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, isGlobal: m.Name == "F") });
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
@"	$InstantiateGenericMethod(this.$F, {Int32}, {String}).call(this, $a, $b, $c);
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
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
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
	$InstantiateGenericMethod($tmp1.$F, {Int32}, {String}).call($tmp1, $a, $b, $c);
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
	$InstantiateGenericMethod($tmp1.$F, {Int32}, {String}).call($tmp1, $a, $b, $c);
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
@"	$InstantiateGenericMethod({C}.$F, {Int32}, {String}).call(null, $a, $b, $c);
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

			er.AllMessages.Where(m => m.StartsWith("Error:")).Should().NotBeEmpty();
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
", namingConvention: new MockNamingConventionResolver { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
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
@"	{C}.$F(this, $a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void GlobalMethodWithThisAsFirstArgumentWorks() {
			AssertCorrect(
@"void F(int x, int y, string z) {}
public void M() {
	int a = 0, b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$F(this, $a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name, isGlobal: true) : MethodScriptSemantics.NormalMethod(m.Name) });
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
@"	$InstantiateGenericType({X}, {Int32}).$F(this.$FX(), $a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	$InstantiateGenericMethod({C}.$F, {Int32}, {String}).call(null, this, $a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
	{C}.$F(this, 1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InstanceMethodOnFirstArgumentWorks() {
			AssertCorrect(
@"class X { }
public static void F(X x, int y, string z) {}
public void M() {
	X a = null;
	int b = 0;
	string c = null;
	// BEGIN
	F(a, b, c);
	// END
}",
@"	$a.$F($b, $c);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InstanceMethodOnFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InstanceMethodOnFirstArgumentWorksWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"static void F(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	// BEGIN
	F(d: F1(), g: F2(), f: F3(), a: F4());
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	this.$F4().$F(2, 3, $tmp1, 5, $tmp3, $tmp2);
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InstanceMethodOnFirstArgument("$" + m.Name) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	_{Int32}_{Byte}_{String}_$o_$a_$b_$c_;
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_{T1}_{T2}_{T3}_{this}_{x}_{y}_{z}_") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
	_1_this.$F4()_3_$tmp1_5_$tmp3_$tmp2_;
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_{a}_{b}_{c}_{d}_{e}_{f}_{g}_") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { UnusableMethod(); } }" }, namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "UnusableMethod" ? MethodScriptSemantics.NotUsableFromScript() : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableMethod")));
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
	$CallBase({B}, '$F', [], [this, $c, $d]);
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
	$CallBase($InstantiateGenericType({B}, {String}), '$F', [], [this, $c, $d]);
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
	$CallBase($InstantiateGenericType({B}, $T2), '$F', [], [this, $c, $d]);
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
@"	$InstantiateGenericMethod(this.$F, {Int32}).call(this, $a, $b);
	$CallBase({B}, '$F', [{Int32}], [this, $c, $d]);
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
@"	$InstantiateGenericMethod(this.$F, {Int32}).call(this, $a, $b);
	$CallBase($InstantiateGenericType({B}, $T2), '$F', [{Int32}], [this, $c, $d]);
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
	$CallBase({B}, '$F', [], [this, $c, $d]);
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
	$CallBase({D}, '$F', [], [this, $c, $d]);
", addSkeleton: false);
		}
	}
}
