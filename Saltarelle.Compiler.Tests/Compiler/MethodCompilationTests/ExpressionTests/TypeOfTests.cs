using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class TypeOfTests : MethodCompilerTestBase {
		[Test]
		public void TypeOfUseDefinedTypesWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var x = typeof(C);
	// END
}
",
@"	var $x = {C};
");
		}

		[Test]
		public void TypeOfUninstantiatedGenericTypesWorks() {
			AssertCorrect(
@"class X<T1, T2> {}
public void M() {
	// BEGIN
	var x = typeof(X<,>);
	// END
}
",
@"	var $x = {C$X};
");
		}

		[Test]
		public void TypeOfInstantiatedGenericTypesWorks() {
			AssertCorrect(
@"class X<T1, T2> {}
class D {}
public void M() {
	// BEGIN
	var x = typeof(X<C, D>);
	// END
}
",
@"	var $x = $InstantiateGenericType({C$X}, {C}, {C$D});
");
		}

		[Test]
		public void TypeOfNestedGenericTypesWorks() {
			AssertCorrect(
@"class X<T1, T2> {}
class D {}
public void M() {
	// BEGIN
	var x = typeof(X<X<C, D>, X<D, C>>);
	// END
}
",
@"	var $x = $InstantiateGenericType({C$X}, $InstantiateGenericType({C$X}, {C}, {C$D}), $InstantiateGenericType({C$X}, {C$D}, {C}));
");
		}

		[Test]
		public void TypeOfTypeParametersForContainingTypeWorks() {
			AssertCorrect(
@"class X<T1, T2> {
	public void M() {
		// BEGIN
		var x = typeof(T1);
		// END
	}
}
",
@"	var $x = $T1;
");
		}

		[Test]
		public void TypeOfTypeParametersForCurrentMethodWorks() {
			AssertCorrect(
@"public void M<T1, T2>() {
	// BEGIN
	var x = typeof(T1);
	// END
}
",
@"	var $x = $T1;
");
		}

		[Test]
		public void TypeOfTypeParametersForParentContainingTypeWorks() {
			AssertCorrect(
@"class X<T1> {
	class X2<T2>
	{
		public void M() {
			// BEGIN
			var x = typeof(T1);
			// END
		}
	}
}
",
@"	var $x = $T1;
");
		}

		[Test]
		public void TypeOfTypePartiallyInstantiatedTypeWorks() {
			AssertCorrect(
@"class X<T1> {
	public class X2<T2> {
	}
}
class D {}
class Y : X<C> {
	public void M() {
		// BEGIN
		var x = typeof(X2<D>);
		// END
	}
}
",
@"	var $x = $InstantiateGenericType({C$X$X2}, {C}, {C$D});
");
		}

		[Test]
		public void CannotUseNotUsableTypeInATypeOfExpression() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {}
class C {
	public void M() {
		var t = typeof(C1);
	}
}" }, namingConvention: nc, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("not usable from script") && er.AllMessages[0].Contains("typeof") && er.AllMessages[0].Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {}
interface I1<T> {}
class C {
	public void M() {
		var t= typeof(I1<I1<C1>>);
	}
}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("not usable from script") && er.AllMessages[0].Contains("typeof") && er.AllMessages[0].Contains("C1"));
		}
	}
}
