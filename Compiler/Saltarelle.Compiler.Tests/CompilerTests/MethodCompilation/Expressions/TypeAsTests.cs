using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class TypeAsTests : MethodCompilerTestBase {
		[Test]
		public void TypeAsWorksForReferenceTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	var r = o as X<int>;
	// END
}
",
@"	var $r = $TryCast($o, ct_$InstantiateGenericType({X}, {ga_Int32}));
");
		}

		[Test]
		public void TypeAsWorksWithNullableTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	var x = o as int?;
	// END
}
",
@"	var $x = $TryCast($o, {ct_Int32});
");
		}

		[Test]
		public void ConversionFromBaseClassToTypeParameterWorks() {
			AssertCorrect(
@"public class B {}
public class D : B {}
public void M<T>() where T : D {
	object o = null;
	B b = null;
	D d = null;
	// BEGIN
	T t1 = o as T;
	T t2 = b as T;
	T t3 = d as T;
	// END
}",
@"	var $t1 = $TryCast($o, ct_$T);
	var $t2 = $TryCast($b, ct_$T);
	var $t3 = $TryCast($d, ct_$T);
");

			AssertCorrect(
@"public class B {}
public class D : B {}
public void M<T>() where T : class, D {
	object o = null;
	B b = null;
	D d = null;
	// BEGIN
	T t1 = o as T;
	T t2 = b as T;
	T t3 = d as T;
	// END
}",
@"	var $t1 = $TryCast($o, ct_$T);
	var $t2 = $TryCast($b, ct_$T);
	var $t3 = $TryCast($d, ct_$T);
");
		}

		[Test]
		public void ConversionFromInterfaceToTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	I i = null;
	// BEGIN
	T t = i as T;
	// END
}",
@"	var $t = $TryCast($i, ct_$T);
");
		}

		[Test]
		public void ConversionToInterfaceFromTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() {
	T t = default(T);
	// BEGIN
	I i = t as I;
	// END
}",
@"	var $i = $TryCast($t, {ct_I});
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	T t = default(T);
	// BEGIN
	I i = t as I;
	// END
}",
@"	var $i = $TryCast($t, {ct_I});
");
		}

		[Test]
		public void TypeParameterCanBeConvertedToOtherTypeParameterWhichDependsOnIt() {
			AssertCorrect(
@"public void M<T, U>() where U : class, T {
	T t = default(T);
	// BEGIN
	U u = t as U;
	// END
}",
@"	var $u = $TryCast($t, ct_$U);
");

			AssertCorrect(
@"public void M<T, U>() where U : class, T where T : class {
	T t = default(T);
	// BEGIN
	U u = t as U;
	// END
}",
@"	var $u = $TryCast($t, ct_$U);
");
		}

	}
}
