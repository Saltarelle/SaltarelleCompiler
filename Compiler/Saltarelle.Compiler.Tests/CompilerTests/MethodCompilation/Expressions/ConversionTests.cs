using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ConversionTests  : MethodCompilerTestBase {
		[Test]
		public void IdentityConversionDoesNotProduceAnyOutput() {
			AssertCorrect(
@"public class C1 {}
public void M() {
	int si = 0;
	int di = (int)si;

	C1 sc = null;
	C1 dc = (C1)sc;
}",
@"function() {
	var $si = 0;
	var $di = $si;
	var $sc = null;
	var $dc = $sc;
}");
		}

		[Test]
		public void ConversionToNullable() {
			AssertCorrect(
@"public void M() {
	int  i1 = 0;
	int? i2 = i1;
	int? i3 = (int?)i1;

	byte  b1 = 0;
	byte? b2 = b1;
	byte? b3 = (byte?)b1;

	decimal  d1 = 0m;
	decimal? d2 = d1;
	decimal? d3 = (decimal?)d1;

	double  f1 = 0.0;
	double? f2 = f1;
	double? f3 = (double?)f1;

	double? f4 = (double?)i1;

	int? i4 = (int?)f1;
	int? i5 = (int?)f2;
}",
@"function() {
	var $i1 = 0;
	var $i2 = $i1;
	var $i3 = $i1;
	var $b1 = 0;
	var $b2 = $b1;
	var $b3 = $b1;
	var $d1 = 0;
	var $d2 = $d1;
	var $d3 = $d1;
	var $f1 = 0;
	var $f2 = $f1;
	var $f3 = $f1;
	var $f4 = $i1;
	var $i4 = $FloatToInt($f1, {ct_Int32});
	var $i5 = $FloatToInt($f2, ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
}");
		}

		[Test]
		public void ToNullableAndTruncatingAtTheSameTime() {
			AssertCorrect(
@"public void M() {
	double d = 1;
	// BEGIN
	var i = (int?)d;
	// END
}",
@"	var $i = $FloatToInt($d, {ct_Int32});
");
		}

		[Test]
		public void FromNullableAndTruncatingAtTheSameTime() {
			AssertCorrect(
@"public void M() {
	double? d = 1;
	// BEGIN
	var i = (int)d;
	// END
}",
@"	var $i = $FloatToInt($FromNullable($d), {ct_Int32});
");
		}

		[Test]
		public void NullLiteralToNullable() {
			AssertCorrect(
@"public void M() {
	int? x = null;
}",
@"function() {
	var $x = null;
}");
		}

		[Test]
		public void ConversionFromNullable() {
			AssertCorrect(
@"public void M() {
	int? i1 = null;
	int i2 = (int)i1;

	double? d1 = null;
	double d2 = (double)d1;

	int i3 = (int)d1;
	double d3 = (double)i1;

	bool? b1 = false;
	bool b2 = (bool)b1;
}",
@"function() {
	var $i1 = null;
	var $i2 = $FromNullable($i1);
	var $d1 = null;
	var $d2 = $FromNullable($d1);
	var $i3 = $FloatToInt($FromNullable($d1), {ct_Int32});
	var $d3 = $FromNullable($i1);
	var $b1 = false;
	var $b2 = $FromNullable($b1);
}");
		}

		[Test]
		public void BoxingIsUpcast() {
			AssertCorrect(
@"public void M() {
	int x1 = 0;
	int? x2 = 0;
	double x3 = 0;
	double? x4 = 0;
	bool x5 = false;
	bool? x6 = false;

	object o1 = x1;
	object o2 = x2;
	object o3 = x3;
	object o4 = x4;
	object o5 = x5;
	object o6 = x6;
}",
@"function() {
	var $x1 = 0;
	var $x2 = 0;
	var $x3 = 0;
	var $x4 = 0;
	var $x5 = false;
	var $x6 = false;
	var $o1 = $Upcast($x1, {ct_Object});
	var $o2 = $Upcast($x2, {ct_Object});
	var $o3 = $Upcast($x3, {ct_Object});
	var $o4 = $Upcast($x4, {ct_Object});
	var $o5 = $Upcast($x5, {ct_Object});
	var $o6 = $Upcast($x6, {ct_Object});
}");
		}

		[Test]
		public void UnboxingWorks() {
			AssertCorrect(
@"public void M() {
	object o = null;

	int x1 = (int)o;
	int? x2 = (int?)o;
	double x3 = (double)o;
	double? x4 = (double?)o;
	bool x5 = (bool)o;
	bool? x6 = (bool?)o;
}",
@"function() {
	var $o = null;
	var $x1 = $FromNullable($Cast($o, {ct_Int32}));
	var $x2 = $Cast($o, {ct_Int32});
	var $x3 = $FromNullable($Cast($o, {ct_Double}));
	var $x4 = $Cast($o, {ct_Double});
	var $x5 = $FromNullable($Cast($o, {ct_Boolean}));
	var $x6 = $Cast($o, {ct_Boolean});
}");
		}

		[Test]
		public void DowncastingToDerivedClassWorks() {
			AssertCorrect(
@"class B {}
class D : B {}
public void M() {
	B b = null;
	// BEGIN
	D d = (D)b;
	// END
}",
@"	var $d = $Cast($b, {ct_D});
");
		}

		[Test]
		public void DowncastingToUnimplementedInterfaceWorks() {
			AssertCorrect(
@"public class B {}
public void M() {
	B b = null;
	// BEGIN
	var d = (System.Collections.Generic.IEnumerable<object>)b;
	// END
}",
@"	var $d = $Cast($b, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
");
		}

		[Test]
		public void CastingBetweenUnrelatedInterfacesWorks() {
			AssertCorrect(
@"public interface I {}
public void M() {
	I i = null;
	// BEGIN
	var i2 = (System.Collections.Generic.IEnumerable<object>)i;
	// END
}",
@"	var $i2 = $Cast($i, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
");
		}

		[Test]
		public void UpcastingWorks() {
			AssertCorrect(
@"class B {}
class D : B {}
public void M() {
	D d = null;
	// BEGIN
	B b1 = (B)d;
	B b2 = (B)d;
	// END
}",
@"	var $b1 = $Upcast($d, {ct_B});
	var $b2 = $Upcast($d, {ct_B});
");
		}

		[Test]
		public void CastingToImplementedInterfaceWorks() {
			AssertCorrect(
@"public class C1 : System.Collections.Generic.IEnumerable<object> {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return null; }
}
public void M() {
	C1 c = null;
	// BEGIN
	System.Collections.Generic.IEnumerable<object> i1 = (System.Collections.Generic.IEnumerable<object>)c;
	System.Collections.Generic.IEnumerable<object> i2 = c;
	// END
}",
@"	var $i1 = $Upcast($c, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
	var $i2 = $Upcast($c, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
");
		}

		[Test]
		public void CastingInterfaceToUnrelatedTypeWorks() {
			AssertCorrect(
@"public class D : System.Collections.Generic.IEnumerable<object> {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return null; }
}
public void M() {
	System.Collections.Generic.IEnumerable<object> i = null;
	// BEGIN
	D d = (D)i;
	// END
}",
@"	var $d = $Cast($i, {ct_D});
");
		}

		[Test]
		public void CastingInterfaceToTypeThatImplementsTheInterfaceWorks() {
			AssertCorrect(
@"public class D {}
public void M() {
	System.Collections.Generic.IEnumerable<object> i = null;
	// BEGIN
	D d = (D)i;
	// END
}",
@"	var $d = $Cast($i, {ct_D});
");
		}

		[Test]
		public void CastingToObjectWorks() {
			AssertCorrect(
@"public class C1 {}
public void M() {
	C1 c = null;
	object[] arr = null;
	// BEGIN
	object o1 = c;
	object o2 = (object)c;
	object o3 = arr;
	// END
}",
@"	var $o1 = $Upcast($c, {ct_Object});
	var $o2 = $Upcast($c, {ct_Object});
	var $o3 = $Upcast($arr, {ct_Object});
");
		}

		[Test]
		public void CastingFromObjectWorks() {
			AssertCorrect(
@"public class D {}
public void M() {
	object o = null;
	// BEGIN
	D d = (D)o;
	object[] arr = (object[])o;
	// END
}",
@"	var $d = $Cast($o, {ct_D});
	var $arr = $Cast($o, ct_$Array({ga_Object}));
");
		}

		[Test]
		public void CastingToDynamicWorks() {
			AssertCorrect(
@"public class C1 {}
public void M() {
	C1 c = null;
	// BEGIN
	dynamic d1 = c;
	dynamic d2 = (dynamic)c;
	// END
}",
@"	var $d1 = $c;
	var $d2 = $c;
");
		}

		[Test]
		public void ArrayCovarianceIsANoOp() {
			AssertCorrect(
@"public class B {}
public class D : B {}
public void M() {
	D[] d = null;
	// BEGIN
	B[] b1 = d;
	B[] b2 = (B[])d;
	// END
}",
@"	var $b1 = $d;
	var $b2 = $d;
");
		}

		[Test]
		public void ArrayContravarianceIsANoOp() {
			AssertCorrect(
@"public class B {}
public class D : B {}
public void M() {
	B[] b = null;
	// BEGIN
	D[] d = (D[])b;
	// END
}",
@"	var $d = $b;
");
		}

		[Test]
		public void ConvertingArrayTypeToSystemArrayIsAnUpcast() {
			AssertCorrect(
@"public class C1 {}
public void M() {
	C1[] c = null;
	// BEGIN
	Array a1 = (Array)c;
	Array a2 = (Array)c;
	// END
}",
@"	var $a1 = $Upcast($c, {ct_Array});
	var $a2 = $Upcast($c, {ct_Array});
");
		}

		[Test]
		public void ConvertingSystemArrayToArrayTypeIsADowncast() {
			AssertCorrect(
@"public class D {}
public void M() {
	Array a = null;
	// BEGIN
	D[] d = (D[])a;
	// END
}",
@"	var $d = $Cast($a, ct_$Array({ga_D}));
");
		}

		[Test]
		public void GenericVarianceConversionIsAnUpcast() {
			AssertCorrect(
@"class D {}
public void M() {
	System.Collections.Generic.IEnumerable<C> c1 = null;
	System.Collections.Generic.List<C> c2 = null;
	// BEGIN
	System.Collections.Generic.IEnumerable<object> o1 = c1;
	System.Collections.Generic.IEnumerable<object> o2 = c2;
	// END
}",
@"	var $o1 = $Upcast($c1, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
	var $o2 = $Upcast($c2, ct_$InstantiateGenericType({IEnumerable}, {ga_Object}));
");
		}

		[Test]
		public void NullLiteralToReferenceTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	object o1 = null;
	System.Collections.Generic.IEnumerable<object> o2 = null;
	System.Collections.Generic.List<object> o3 = null;
	// END
}",
@"	var $o1 = null;
	var $o2 = null;
	var $o3 = null;
");
		}

		[Test]
		public void CastingDynamicToObjectIsANoOp() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	object o = d;
	// END
}",
@"	var $o = $d;
");
		}

		[Test]
		public void CastingDynamicToReferenceTypeIsADowncast() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	System.Collections.Generic.List<object> l = (System.Collections.Generic.List<object>)d;
	// END
}",
@"	var $l = $Cast($d, ct_$InstantiateGenericType({List}, {ga_Object}));
");
		}

		[Test]
		public void CastingDynamicToNullableValueTypeIsADowncast() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;

	// BEGIN
	int? x1 = (int?)d;
	double? x2 = (double?)d;
	bool? x3 = (bool?)d;
	// END
}",
@"	var $x1 = $Cast($d, {ct_Int32});
	var $x2 = $Cast($d, {ct_Double});
	var $x3 = $Cast($d, {ct_Boolean});
");
		}

		[Test]
		public void CastingDynamicToValueTypeIsADowncast() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;

	// BEGIN
	int x1 = (int)d;
	double x2 = (double)d;
	bool x3 = (bool)d;
	// END
}",
@"	var $x1 = $FromNullable($Cast($d, {ct_Int32}));
	var $x2 = $FromNullable($Cast($d, {ct_Double}));
	var $x3 = $FromNullable($Cast($d, {ct_Boolean}));
");
		}

		[Test]
		public void ConvertingNumberToSystemValueTypeIsAnUpcast() {
			AssertCorrect(
@"public void M() {
	int i = 0;

	// BEGIN
	ValueType v1 = i;
	ValueType v2 = (ValueType)i;
	// END
}",
@"	var $v1 = $Upcast($i, {ct_ValueType});
	var $v2 = $Upcast($i, {ct_ValueType});
");
		}

		[Test]
		public void ConvertingSystemValueTypeToNumberIsADowncast() {
			AssertCorrect(
@"public void M() {
	ValueType v = null;

	// BEGIN
	int i1 = (int)v;
	int? i2 = (int?)v;
	// END
}",
@"	var $i1 = $FromNullable($Cast($v, {ct_Int32}));
	var $i2 = $Cast($v, {ct_Int32});
");
		}

		[Test]
		public void LiteralZeroCanBeConvertedToEnum() {
			AssertCorrect(
@"enum E { A, B, C }
public void M() {
	// BEGIN
	E e1 = 0;
	E? e2 = 0;
	// END
}",
@"	var $e1 = $Default({def_E});
	var $e2 = $Default({def_E});
");
		}

		[Test]
		public void ImplicitArrayToGenericIList() {
			AssertCorrect(
@"class B {}
class D : B {}

public void M() {
	D[] src = null;
	// BEGIN
	System.Collections.Generic.IList<B> l1 = src;
	System.Collections.Generic.IList<B> l2 = (System.Collections.Generic.IList<B>)src;
	System.Collections.Generic.IList<D> l3 = src;
	System.Collections.Generic.IList<D> l4 = (System.Collections.Generic.IList<D>)src;
	// END
}",
@"	var $l1 = $Upcast($src, ct_$InstantiateGenericType({IList}, {ga_B}));
	var $l2 = $Upcast($src, ct_$InstantiateGenericType({IList}, {ga_B}));
	var $l3 = $Upcast($src, ct_$InstantiateGenericType({IList}, {ga_D}));
	var $l4 = $Upcast($src, ct_$InstantiateGenericType({IList}, {ga_D}));
");
		}

		[Test]
		public void ExplicitArrayToGenericIList() {
			AssertCorrect(
@"class B {}
class D : B {}

public void M() {
	B[] src = null;
	// BEGIN
	System.Collections.Generic.IList<D> l = (System.Collections.Generic.IList<D>)src;
	// END
}",
@"	var $l = $Cast($src, ct_$InstantiateGenericType({IList}, {ga_D}));
");
		}

		[Test]
		public void DelegateTypeCanBeConvertedToSystemDelegate() {
			AssertCorrect(
@"public void M() {
	Func<int> f = null;
	// BEGIN
	Delegate d1 = (Delegate)f;
	Delegate d2 = f;
	// END
}",
@"	var $d1 = $Upcast($f, {ct_Delegate});
	var $d2 = $Upcast($f, {ct_Delegate});
");
		}

		[Test]
		public void DelegateTypeCanBeVarianceConvertedToOtherDelegateType() {
			AssertCorrect(
@"class B {}
class D : B {}

public void M() {
	Func<B, D> f = null;
	// BEGIN
	Func<D, B> f2 = f;
	// END
}",
@"	var $f2 = $f;
");
		}

		[Test]
		public void SystemDelegateCanBeConvertedToDelegateTypeDelegateType() {
			AssertCorrect(
@"public void M() {
	Delegate d = null;
	
	// BEGIN
	var f = (Func<int>)d;
	// END
}",
@"	var $f = $Cast($d, ct_$InstantiateGenericType({Func}, {ga_Int32}));
");
		}

		[Test]
		public void TypeParameterCanBeConvertedToConcreteBaseType() {
			AssertCorrect(
@"public class D {}
public interface I {}
public void M<T>() where T : D, I {
	T t = default(T);
	// BEGIN
	object o1 = (object)t;
	object o2 = t;
	D d1 = (D)t;
	D d2 = t;
	I i1 = (I)t;
	I i2 = t;
	// END
}",
@"	var $o1 = $Upcast($t, {ct_Object});
	var $o2 = $Upcast($t, {ct_Object});
	var $d1 = $Upcast($t, {ct_D});
	var $d2 = $Upcast($t, {ct_D});
	var $i1 = $Upcast($t, {ct_I});
	var $i2 = $Upcast($t, {ct_I});
");

			AssertCorrect(
@"public class D {}
public interface I {}
public void M<T>() where T : class, I {
	T t = default(T);
	// BEGIN
	object o1 = (object)t;
	object o2 = t;
	I i1 = (I)t;
	I i2 = t;
	// END
}",
@"	var $o1 = $Upcast($t, {ct_Object});
	var $o2 = $Upcast($t, {ct_Object});
	var $i1 = $Upcast($t, {ct_I});
	var $i2 = $Upcast($t, {ct_I});
");
		}

		[Test]
		public void TypeParameterCanBeConvertedToOtherTypeParameterOnWhichItDepends() {
			AssertCorrect(
@"public void M<T, U>() where T : U {
	T t = default(T);
	// BEGIN
	U u1 = (U)t;
	U u2 = t;
	// END
}",
@"	var $u1 = $Upcast($t, $U);
	var $u2 = $Upcast($t, $U);
");

			AssertCorrect(
@"public void M<T, U>() where T : class, U {
	T t = default(T);
	// BEGIN
	U u1 = (U)t;
	U u2 = t;
	// END
}",
@"	var $u1 = $Upcast($t, $U);
	var $u2 = $Upcast($t, $U);
");
		}

		[Test]
		public void ExplicitConversionFromBaseClassToTypeParameterWorks() {
			AssertCorrect(
@"public class B {}
public class D : B {}
public void M<T>() where T : D {
	object o = null;
	B b = null;
	D d = null;
	// BEGIN
	T t1 = (T)o;
	T t2 = (T)b;
	T t3 = (T)d;
	// END
}",
@"	var $t1 = $Cast($o, $T);
	var $t2 = $Cast($b, $T);
	var $t3 = $Cast($d, $T);
");
		}

		[Test]
		public void ExplicitConversionFromInterfaceToTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() {
	I i = null;
	// BEGIN
	T t = (T)i;
	// END
}",
@"	var $t = $Cast($i, $T);
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	I i = null;
	// BEGIN
	T t = (T)i;
	// END
}",
@"	var $t = $Cast($i, $T);
");
		}

		[Test]
		public void ExplicitConversionToInterfaceFromTypeParameterWorks() {
			AssertCorrect(
@"public interface I {}
public void M<T>() {
	T t = default(T);
	// BEGIN
	I i = (I)t;
	// END
}",
@"	var $i = $Cast($t, {ct_I});
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	T t = default(T);
	// BEGIN
	I i = (I)t;
	// END
}",
@"	var $i = $Cast($t, {ct_I});
");
		}

		[Test]
		public void TypeParameterCanBeConvertedToOtherTypeParameterWhichDependsOnIt() {
			AssertCorrect(
@"public void M<T, U>() where U : T {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, $U);
");

			AssertCorrect(
@"public void M<T, U>() where U : class, T {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, $U);
");

			AssertCorrect(
@"public void M<T, U>() where U : T where T : class {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, $U);
");
		}

		[Test]
		public void UsingOpImplicitWorks() {
			AssertCorrect(@"
class C1 {}
class C2 {
	public static implicit operator C1(C2 c2) {
		return null;
	}
}
public void M() {
	var c2 = new C2();
	// BEGIN
	C1 c11 = c2;
	C1 c12 = (C1)c2;
	// END
}",
@"	var $c11 = {sm_C2}.$op_Implicit($c2);
	var $c12 = {sm_C2}.$op_Implicit($c2);
");
		}

		[Test]
		public void UsingOpExplicitWorks() {
			AssertCorrect(@"
class C1 {}
class C2 {
	public static explicit operator C1(C2 c2) {
		return null;
	}
}
public void M() {
	var c2 = new C2();
	// BEGIN
	C1 c1 = (C1)c2;
	// END
}",
@"	var $c1 = {sm_C2}.$op_Explicit($c2);
");
		}

		[Test]
		public void NullableConversionsWorkWithOpImplicit() {
			AssertCorrect(@"
class C1 {
	public static implicit operator C1(int? i) {
		return null;
	}

	public static implicit operator int(C1 c) {
		return 0;
	}
}

public void M() {
	C1 c1 = null;
	int i = 0;
	// BEGIN
	int? ni1 = c1;
	int? ni2 = (int?)c1;
	C1 c2 = i;
	C1 c3 = (C1)i;
	// END
}",
@"	var $ni1 = {sm_C1}.$op_Implicit($c1);
	var $ni2 = {sm_C1}.$op_Implicit($c1);
	var $c2 = {sm_C1}.$op_Implicit($i);
	var $c3 = {sm_C1}.$op_Implicit($i);
");
		}

		[Test]
		public void NullableConversionsWorkWithOpExplicit() {
			AssertCorrect(@"
class C1 {
	public static explicit operator C1(int? i) {
		return null;
	}

	public static explicit operator int(C1 c) {
		return 0;
	}
}

public void M() {
	C1 c1 = null;
	int i = 0;
	// BEGIN
	int? ni = (int?)c1;
	C1 c2 = (C1)i;
	// END
}",
@"	var $ni = {sm_C1}.$op_Explicit($c1);
	var $c2 = {sm_C1}.$op_Explicit($i);
");
		}

		[Test]
		public void ExplicitEnumerationConversion() {
			AssertCorrect(@"
enum E1 { X }
enum E2 { Y }

public void M() {
	int i = 0;
	E1 e = E1.X;
	// BEGIN
	unchecked {
		int i21 = (int)e;
		E2 e21 = (E2)e;
		E1 e11 = (E1)i;
	}
	checked {
		int i22 = (int)e;
		E2 e22 = (E2)e;
		E1 e12 = (E1)i;
	}
	// END
}",
@"	{
		var $i21 = $EnumConvert($e, {ct_Int32});
		var $e21 = $EnumConvert($e, {ct_E2});
		var $e11 = $EnumConvert($i, {ct_E1});
	}
	{
		var $i22 = $EnumConvertChecked($e, {ct_Int32});
		var $e22 = $EnumConvertChecked($e, {ct_E2});
		var $e12 = $EnumConvertChecked($i, {ct_E1});
	}
");
		}

		[Test]
		public void ExplicitEnumerationConversionFromDouble() {
			AssertCorrect(@"
enum E { X }

public void M() {
	double  d1 = 0;
	double? d2 = 0;
	// BEGIN
	unchecked {
		E  e11 = (E)d1;
		E? e12 = (E?)d2;
		E  e13 = (E)d2;
		E? e14 = (E?)d2;
	}
	checked {
		E  e21 = (E)d1;
		E? e22 = (E?)d2;
		E  e23 = (E)d2;
		E? e24 = (E?)d2;
	}
	// END
}",
@"	{
		var $e11 = $EnumConvert($d1, {ct_E});
		var $e12 = $EnumConvert($d2, ct_$InstantiateGenericType({Nullable}, {ga_E}));
		var $e13 = $EnumConvert($FromNullable($d2), {ct_E});
		var $e14 = $EnumConvert($d2, ct_$InstantiateGenericType({Nullable}, {ga_E}));
	}
	{
		var $e21 = $EnumConvertChecked($d1, {ct_E});
		var $e22 = $EnumConvertChecked($d2, ct_$InstantiateGenericType({Nullable}, {ga_E}));
		var $e23 = $EnumConvertChecked($FromNullable($d2), {ct_E});
		var $e24 = $EnumConvertChecked($d2, ct_$InstantiateGenericType({Nullable}, {ga_E}));
	}
");
		}

		[Test]
		public void LiftedExplicitEnumerationConversions() {
			AssertCorrect(@"
enum E1 { X }
enum E2 { Y }

public void M() {
	int i = 0;
	E1 e = E1.X;
	int? ix = 0;
	E1? ex = E1.X;
	// BEGIN
	unchecked {
		int? i21 = (int?)e;
		E2? e21 = (E2?)e;
		E1? e11 = (E1?)i;
		int? i211 = (int?)ex;
		E2? e211 = (E2?)ex;
		E1? e111 = (E1?)ix;
		int i221 = (int)ex;
		E2 e221 = (E2)ex;
		E1 e121 = (E1)ix;
	}
	checked {
		int? i22 = (int?)e;
		E2? e22 = (E2?)e;
		E1? e12 = (E1?)i;
		int? i212 = (int?)ex;
		E2? e212 = (E2?)ex;
		E1? e112 = (E1?)ix;
		int i222 = (int)ex;
		E2 e222 = (E2)ex;
		E1 e122 = (E1)ix;
	}
	// END
}",
@"	{
		var $i21 = $EnumConvert($e, {ct_Int32});
		var $e21 = $EnumConvert($e, {ct_E2});
		var $e11 = $EnumConvert($i, {ct_E1});
		var $i211 = $EnumConvert($ex, ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		var $e211 = $EnumConvert($ex, ct_$InstantiateGenericType({Nullable}, {ga_E2}));
		var $e111 = $EnumConvert($ix, ct_$InstantiateGenericType({Nullable}, {ga_E1}));
		var $i221 = $EnumConvert($FromNullable($ex), {ct_Int32});
		var $e221 = $EnumConvert($FromNullable($ex), {ct_E2});
		var $e121 = $EnumConvert($FromNullable($ix), {ct_E1});
	}
	{
		var $i22 = $EnumConvertChecked($e, {ct_Int32});
		var $e22 = $EnumConvertChecked($e, {ct_E2});
		var $e12 = $EnumConvertChecked($i, {ct_E1});
		var $i212 = $EnumConvertChecked($ex, ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		var $e212 = $EnumConvertChecked($ex, ct_$InstantiateGenericType({Nullable}, {ga_E2}));
		var $e112 = $EnumConvertChecked($ix, ct_$InstantiateGenericType({Nullable}, {ga_E1}));
		var $i222 = $EnumConvertChecked($FromNullable($ex), {ct_Int32});
		var $e222 = $EnumConvertChecked($FromNullable($ex), {ct_E2});
		var $e122 = $EnumConvertChecked($FromNullable($ix), {ct_E1});
	}
");
		}

		[Test]
		public void StandardConversionBeforeUserDefinedConversion() {
			AssertCorrect(@"
class MyConvertible {
	public static explicit operator MyConvertible(int i) { return null; }
}
void M() {
	// BEGIN
	var c = (MyConvertible)3.14;
	// END
}",
@"	var $c = {sm_MyConvertible}.$op_Explicit($FloatToInt(3.14, {ct_Int32}));
");
		}

		[Test]
		public void StandardConversionAfterUserDefinedConversion() {
			AssertCorrect(@"
class MyConvertible {
	public static explicit operator double(MyConvertible c) { return 0; }
}
void M() {
	// BEGIN
	var c = (int)new MyConvertible();
	// END
}",
@"	var $c = $FloatToInt({sm_MyConvertible}.$op_Explicit(new {sm_MyConvertible}()), {ct_Int32});
");
		}

		[Test]
		public void LiftedUserDefinedConversion() {
			AssertCorrect(@"
struct S { public static implicit operator int(S s) { return 0; } }
void M() {
	S? s = null;
	// BEGIN
	int? i = s;
	// END
}
",
@"	var $i = $Lift({sm_S}.$op_Implicit($s));
");
		}

		[Test]
		public void BoxingValueTypeCreatesCopy() {
			AssertCorrect(@"
struct S : IDisposable { public void Dispose() {} }
void M() {
	S s;
	object o;
	IDisposable d;
	// BEGIN
	o = s;
	d = s;
	// END;
}",
@"	$o = $Upcast($Clone($s, {to_S}), {ct_Object});
	$d = $Upcast($Clone($s, {to_S}), {ct_IDisposable});
", mutableValueTypes: true);
		}

		[Test]
		public void UnboxingValueTypeCreatesCopy() {
			AssertCorrect(@"
struct S : IDisposable { public void Dispose() {} }
void M() {
	S s;
	object o = null;
	IDisposable d = null;
	// BEGIN
	s = (S)o;
	s = (S)d;
	// END;
}",
@"	$s = $Clone($FromNullable($Cast($o, {ct_S})), {to_S});
	$s = $Clone($FromNullable($Cast($d, {ct_S})), {to_S});
", mutableValueTypes: true);
		}

		[Test]
		public void ConvertingValueTypeToDynamicCreatesCopy() {
			AssertCorrect(@"
void M() {
	int i = 0;
	dynamic d;
	// BEGIN
	d = i;
	// END;
}",
@"	$d = $Clone($i, {to_Int32});
", mutableValueTypes: true);
		}

		[Test]
		public void ConvertingDynamicToValueTypeCreatesCopy() {
			AssertCorrect(@"
void M() {
	int i;
	dynamic d = null;
	// BEGIN
	i = d;
	// END;
}",
@"	$i = $Clone($FromNullable($Cast($d, {ct_Int32})), {to_Int32});
", mutableValueTypes: true);
		}
	}
}
