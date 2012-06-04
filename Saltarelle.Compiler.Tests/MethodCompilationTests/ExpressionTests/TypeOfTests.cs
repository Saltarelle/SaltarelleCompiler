using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
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
@"	var $x = {X};
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
@"	var $x = $InstantiateGenericType({X}, {C}, {D});
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
@"	var $x = $InstantiateGenericType({X}, $InstantiateGenericType({X}, {C}, {D}), $InstantiateGenericType({X}, {D}, {C}));
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
@"	var $x = $InstantiateGenericType({X2}, {C}, {D});
");
		}
	}
}
