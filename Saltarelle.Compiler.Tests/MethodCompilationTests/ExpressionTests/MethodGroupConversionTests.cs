using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class MethodGroupConversionTests : MethodCompilerTestBase {
		[Test]
		public void ReadingMethodGroupWithOneMethodWorks() {
			AssertCorrect(
@"void F(int x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind(this.$F, this);
");
		}

		[Test, Ignore("NRefactory loses method group conversion")]
		public void CombiningDeclarationAndAssignmentWorks() {
			AssertCorrect(
@"void F(int x) {}
public void M() {
	// BEGIN
	System.Action<int> f = F;
	// END
}",
@"	var $f = $Bind(this.$F, this);
");
		}

		[Test]
		public void ReadingStaticMethodGroupWorks() {
			AssertCorrect(
@"static void F(int x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = {C}.$F;
");
		}

		[Test]
		public void ReadingMethodGroupWithOverloadsWorks() {
			AssertCorrect(
@"void F(int x) {}
void F(string x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind(this.F_Int32, this);
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void ReadingMethodGroupWithAnotherTargetWorks() {
			AssertCorrect(
@"class X { public void F(int x) {} public void F(string x) {} }
public void M() {
	Action<int> f;
	var x = new X();
	// BEGIN
	f = x.F;
	// END
}
",
@"	$f = $Bind($x.F_Int32, $x);
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void ReadingMethodGroupWithAnotherTargetOnlyInvokesTheTargetOnce() {
			AssertCorrect(
@"class X { public void F(int x) {} public void F(string x) {} }
X F2() { return null; }
public void M() {
	Action<int> f;
	var x = new X();
	// BEGIN
	f = F2().F;
	// END
}
",
@"	var $tmp1 = this.F2();
	$f = $Bind($tmp1.F_Int32, $tmp1);
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void MethodGroupConversionCanInstantiateGenericMethod() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $Bind($InstantiateGenericMethod(this.$F, {Int32}), this);
");
		}

		[Test]
		public void MethodGroupConversionCanInstantiateGenericMethodWhenTheGenericArgumentIsNotExplicitlySpecified() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind($InstantiateGenericMethod(this.$F, {Int32}), this);
");
		}

		[Test]
		public void MethodGroupConversionDoesNotInstantiateGenericMethodIfIgnoreGenericArgumentsIsSet() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $Bind(this.$F, this);
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
		}


		[Test]
		public void MethodGroupConversionCanInstantiateGenericStaticMethod() {
			AssertCorrect(
@"static void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $InstantiateGenericMethod({C}.$F, {Int32});
");
		}

		[Test]
		public void UsingAMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { System.Func<int> f; f = UnusableMethod; } }" }, namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => m.Name == "UnusableMethod" ? MethodImplOptions.NotUsableFromScript() : MethodImplOptions.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableMethod")));
		}
	}
}
