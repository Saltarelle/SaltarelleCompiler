using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
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

		[Test]
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
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
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
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { System.Func<int> f; f = UnusableMethod; } }" }, namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => m.Name == "UnusableMethod" ? MethodScriptSemantics.NotUsableFromScript() : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableMethod")));
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfOverriddenMethodWorks() {
			AssertCorrect(
@"
class B {
	public virtual void F(int x, int y) {}
}
class D : B {
	public override void F(int x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({B}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForMethodOverriddenFromGenericClassWorks() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D : B<string> {
	public override void F(string x, int y) {}
	public void M() {
		System.Action<string, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall($InstantiateGenericType({B}, {String}), '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D<T2> : B<T2> {
	public override void F(T2 x, int y) {}
	public void M() {
		System.Action<T2, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall($InstantiateGenericType({B}, $T2), '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForGenericOverriddenMethodWorks() {
			AssertCorrect(
@"class B {
	public virtual void F<T>(T x, int y) {}
}
class D : B {
	public override void F<U>(U x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind($InstantiateGenericMethod(this.$F, {Int32}), this);
	$a = $BindBaseCall({B}, '$F', [{Int32}], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForGenericMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F<U>(U x, int y) {}
}
class D<T2> : B<T2> {
	public override void F<S>(S x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind($InstantiateGenericMethod(this.$F, {Int32}), this);
	$a = $BindBaseCall($InstantiateGenericType({B}, $T2), '$F', [{Int32}], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfMethodInheritedFromGrandParentWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
}
class D2 : D {
	public override void F(int x, int y) {}

	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({B}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfMethodDefinedInGrandParentAndOverriddenInParentWorks() {
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
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({D}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void CannotPerformMethodGroupConversionOnMethodThatExpandsParams() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void F(int x, int y, params int[] args) {}
	public void M() {
		System.Action<int, int, int[]> a = F;
	}
}" }, namingConvention: new MockNamingConventionResolver { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("C1.F") && er.AllMessagesText[0].Contains("expand") && er.AllMessagesText[0].Contains("param array"));
		}
	}
}
