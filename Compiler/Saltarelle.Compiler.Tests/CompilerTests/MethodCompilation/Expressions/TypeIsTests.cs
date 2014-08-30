using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class TypeIsTests : MethodCompilerTestBase {
		[Test]
		public void TypeIsWorksForReferenceTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is X<int>;
	// END
}
",
@"	var $b = $TypeIs($o, ct_$InstantiateGenericType({X}, {ga_Int32}));
");
		}

		[Test]
		public void TypeIsWorksForUnboxingConversions() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is int;
	// END
}
",
@"	var $b = $TypeIs($o, {ct_Int32});
");
		}

		[Test]
		public void TypeIsWorksWithNullableTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is int?;
	// END
}
",
@"	var $b = $TypeIs($o, {ct_Int32});
");
		}

		[Test]
		public void FromBaseClassToTypeParameterWorks() {
			AssertCorrect(
@"public class B {}
public class D : B {}
public void M<T>() where T : D {
	object o = null;
	B b = null;
	D d = null;
	// BEGIN
	bool b1 = o is T;
	bool b2 = b is T;
	bool b3 = d is T;
	// END
}",
@"	var $b1 = $TypeIs($o, $T);
	var $b2 = $TypeIs($b, $T);
	var $b3 = $TypeIs($d, $T);
");
		}

		[Test]
		public void FromInterfaceToTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() {
	I i = null;
	// BEGIN
	bool b = i is T;
	// END
}",
@"	var $b = $TypeIs($i, $T);
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	I i = null;
	// BEGIN
	bool b = i is T;
	// END
}",
@"	var $b = $TypeIs($i, $T);
");
		}

		[Test]
		public void ToInterfaceFromTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() {
	T t = default(T);
	// BEGIN
	bool b = t is I;
	// END
}",
@"	var $b = $TypeIs($t, {ct_I});
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	T t = default(T);
	// BEGIN
	bool b = t is I;
	// END
}",
@"	var $b = $TypeIs($t, {ct_I});
");
		}

		[Test]
		public void TypeParameterToOtherTypeParameterWhichDependsOnItWorks() {
			AssertCorrect(
@"public void M<T, U>() where U : T {
	T t = default(T);
	// BEGIN
	bool b = t is U;
	// END
}",
@"	var $b = $TypeIs($t, $U);
");

			AssertCorrect(
@"public void M<T, U>() where U : class, T {
	T t = default(T);
	// BEGIN
	bool b = t is U;
	// END
}",
@"	var $b = $TypeIs($t, $U);
");

			AssertCorrect(
@"public void M<T, U>() where U : T where T : class {
	T t = default(T);
	// BEGIN
	bool b = t is U;
	// END
}",
@"	var $b = $TypeIs($t, $U);
");
		}
	}
}
