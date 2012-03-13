using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
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

		[Test, Ignore("NRefactory bug")]
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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
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

		[Test, Ignore("NRefactory bug")]
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
@"void F(ref int x, out int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
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
@"void F(ref int x, out int y, int z) {}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(ref a, out b, a = b = c);
	// END
}
",
@"	$b = $c;
	$a = $c;
	this.$F($a, $b, $c.$);
");
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
			Assert.Inconclusive("TODO");
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
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
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
			Assert.Inconclusive("TODO");
		}

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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "F" ? MethodImplOptions.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodImplOptions.NormalMethod(m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "F" ? MethodImplOptions.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodImplOptions.NormalMethod("$" + m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "F" ? MethodImplOptions.StaticMethodWithThisAsFirstArgument("$" + m.Name) : MethodImplOptions.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentWorksWithReorderedAndDefaultArguments() {
			Assert.Inconclusive("TODO");
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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "F" ? MethodImplOptions.InstanceMethodOnFirstArgument("$" + m.Name) : MethodImplOptions.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InstanceMethodOnFirstArgumentWorksWithReorderedAndDefaultArguments() {
			Assert.Inconclusive("TODO");
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
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "F" ? MethodImplOptions.InlineCode("_{T1}_{T2}_{T3}_{this}_{x}_{y}_{z}_") : MethodImplOptions.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingMethodImplementedAsInlineCodeWorksWithReorderedAndDefaultArguments() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void InvokingMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { UnusableMethod(); } }" }, namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "UnusableMethod" ? MethodImplOptions.NotUsableFromScript() : MethodImplOptions.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableMethod")));
		}
	}
}
