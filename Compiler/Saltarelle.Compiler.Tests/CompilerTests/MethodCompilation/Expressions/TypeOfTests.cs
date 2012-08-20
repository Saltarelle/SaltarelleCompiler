using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
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
@"	var $x = {to_C};
");
		}

		[Test]
		public void TypeOfOpenGenericTypesWorks() {
			AssertCorrect(
@"class X<T1, T2> {}
public void M() {
	// BEGIN
	var x = typeof(X<,>);
	// END
}
",
@"	var $x = {to_X};
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
@"	var $x = to_$InstantiateGenericType({X}, {ga_C}, {ga_D});
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
@"	var $x = to_$InstantiateGenericType({X}, ga_$InstantiateGenericType({X}, {ga_C}, {ga_D}), ga_$InstantiateGenericType({X}, {ga_D}, {ga_C}));
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
@"	var $x = to_$T1;
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
@"	var $x = to_$T1;
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
@"	var $x = to_$T1;
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
@"	var $x = to_$InstantiateGenericType({X2}, {ga_C}, {ga_D});
");
		}

		[Test]
		public void CannotUseNotUsableTypeInATypeOfExpression() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {}
class C {
	public void M() {
		var t = typeof(C1);
	}
}" }, metadataImporter: metadataImporter, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("typeof") && er.AllMessagesText[0].Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {}
interface I1<T> {}
class C {
	public void M() {
		var t= typeof(I1<I1<C1>>);
	}
}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("typeof") && er.AllMessagesText[0].Contains("C1"));
		}
	}
}
