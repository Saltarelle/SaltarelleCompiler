using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class ConversionTests  : MethodCompilerTestBase {
		[Test]
		public void IdentityConversionDoesNotProduceAnyOutput() {
			AssertCorrect(
@"public class C {}
public void M() {
	int si = 0;
	int di = (int)si;

	C sc = null;
	C dc = (C)sc;
}",
@"function() {
	var $si = 0;
	var $di = $si;
	var $sc = null;
	var $dc = $sc;
}");
		}

		[Test]
		public void ImplicitNumericFromSByte() {
			AssertCorrect(
@"public void M() {
	sbyte src = 0;

	short   s  = src;
	int     i  = src;
	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $s = $src;
	var $i = $src;
	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromByte() {
			AssertCorrect(
@"public void M() {
	byte src = 0;

	short   s  = src;
	ushort  us = src;
	int     i  = src;
	uint    ui = src;
	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromShort() {
			AssertCorrect(
@"public void M() {
	short src = 0;

	int     i  = src;
	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $i = $src;
	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromUShort() {
			AssertCorrect(
@"public void M() {
	ushort src = 0;

	int     i  = src;
	uint    ui = src;
	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromInt() {
			AssertCorrect(
@"public void M() {
	int src = 0;

	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromUInt() {
			AssertCorrect(
@"public void M() {
	uint src = 0;

	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromLong() {
			AssertCorrect(
@"public void M() {
	long src = 0;

	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromULong() {
			AssertCorrect(
@"public void M() {
	ulong src = 0;

	float   fl = src;
	double  db = src;
	decimal dc = src;
}",
@"function() {
	var $src = 0;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ImplicitNumericFromFloat() {
			AssertCorrect(
@"public void M() {
	float src = 0;

	double  db = src;
}",
@"function() {
	var $src = 0;
	var $db = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromSByte() {
			AssertCorrect(
@"public void M() {
	sbyte src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromByte() {
			AssertCorrect(
@"public void M() {
	byte src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromShort() {
			AssertCorrect(
@"public void M() {
	short src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromUShort() {
			AssertCorrect(
@"public void M() {
	ushort src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromInt() {
			AssertCorrect(
@"public void M() {
	int src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromUInt() {
			AssertCorrect(
@"public void M() {
	uint src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromLong() {
			AssertCorrect(
@"public void M() {
	long src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromULong() {
			AssertCorrect(
@"public void M() {
	ulong src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromChar() {
			AssertCorrect(
@"public void M() {
	char src = '\0';

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $src;
	var $b = $src;
	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $c = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromFloat() {
			AssertCorrect(
@"public void M() {
	float src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $Truncate($src);
	var $b = $Truncate($src);
	var $s = $Truncate($src);
	var $us = $Truncate($src);
	var $i = $Truncate($src);
	var $ui = $Truncate($src);
	var $l = $Truncate($src);
	var $ul = $Truncate($src);
	var $c = $Truncate($src);
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromDouble() {
			AssertCorrect(
@"public void M() {
	double src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $Truncate($src);
	var $b = $Truncate($src);
	var $s = $Truncate($src);
	var $us = $Truncate($src);
	var $i = $Truncate($src);
	var $ui = $Truncate($src);
	var $l = $Truncate($src);
	var $ul = $Truncate($src);
	var $c = $Truncate($src);
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ExplicitNumericFromDecimal() {
			AssertCorrect(
@"public void M() {
	decimal src = 0;

	sbyte   sb = (sbyte)src;
	byte    b  = (byte)src;
	short   s  = (short)src;
	ushort  us = (ushort)src;
	int     i  = (int)src;
	uint    ui = (uint)src;
	long    l  = (long)src;
	ulong   ul = (ulong)src;
	char    c  = (char)src;
	float   fl = (float)src;
	double  db = (double)src;
	decimal dc = (decimal)src;
}",
@"function() {
	var $src = 0;
	var $sb = $Truncate($src);
	var $b = $Truncate($src);
	var $s = $Truncate($src);
	var $us = $Truncate($src);
	var $i = $Truncate($src);
	var $ui = $Truncate($src);
	var $l = $Truncate($src);
	var $ul = $Truncate($src);
	var $c = $Truncate($src);
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
}");
		}

		[Test]
		public void ConversionToNullable() {
			AssertCorrect(
@"public void M() {
	int  i1 = 0;
	int? i2 = i1;
	int? i3 = (int?)i1;

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
	var $d1 = 0;
	var $d2 = $d1;
	var $d3 = $d1;
	var $f1 = 0;
	var $f2 = $f1;
	var $f3 = $f1;
	var $f4 = $i1;
	var $i4 = $Truncate($f1);
	var $i5 = $Lift($Truncate($f2));
}");
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
	var $i3 = $Truncate($FromNullable($d1));
	var $d3 = $FromNullable($i1);
	var $b1 = false;
	var $b2 = $FromNullable($b1);
}");
		}

		[Test]
		public void BoxingDoesNothing() {
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
	var $o1 = $x1;
	var $o2 = $x2;
	var $o3 = $x3;
	var $o4 = $x4;
	var $o5 = $x5;
	var $o6 = $x6;
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
	var $x1 = $FromNullable($Cast($o, {Int32}));
	var $x2 = $Cast($o, {Int32});
	var $x3 = $FromNullable($Cast($o, {Double}));
	var $x4 = $Cast($o, {Double});
	var $x5 = $FromNullable($Cast($o, {Boolean}));
	var $x6 = $Cast($o, {Boolean});
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
@"	var $d = $Cast($b, {D});
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
@"	var $d = $Cast($b, $InstantiateGenericType({IEnumerable}, {Object}));
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
@"	var $i2 = $Cast($i, $InstantiateGenericType({IEnumerable}, {Object}));
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
@"	var $b1 = $Upcast($d, {B});
	var $b2 = $Upcast($d, {B});
");
		}

		[Test]
		public void CastingToImplementedInterfaceWorks() {
			AssertCorrect(
@"public class C : System.Collections.Generic.IEnumerable<object> {}
public void M() {
	C c = null;
	// BEGIN
	System.Collections.Generic.IEnumerable<object> i1 = (System.Collections.Generic.IEnumerable<object>)c;
	System.Collections.Generic.IEnumerable<object> i2 = c;
	// END
}",
@"	var $i1 = $Upcast($c, $InstantiateGenericType({IEnumerable}, {Object}));
	var $i2 = $Upcast($c, $InstantiateGenericType({IEnumerable}, {Object}));
");
		}

		[Test]
		public void CastingInterfaceToUnrelatedTypeWorks() {
			AssertCorrect(
@"public class D : System.Collections.Generic.IEnumerable<object> {}
public void M() {
	System.Collections.Generic.IEnumerable<object> i = null;
	// BEGIN
	D d = (D)i;
	// END
}",
@"	var $d = $Cast($i, {D});
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
@"	var $d = $Cast($i, {D});
");
		}

		[Test]
		public void CastingToObjectWorks() {
			AssertCorrect(
@"public class C {}
public void M() {
	C c = null;
	object[] arr = null;
	// BEGIN
	object o1 = c;
	object o2 = (object)c;
	object o3 = arr;
	// END
}",
@"	var $o1 = $Upcast($c, {Object});
	var $o2 = $Upcast($c, {Object});
	var $o3 = $Upcast($arr, {Object});
");
		}

		[Test]
		public void CastingFromObjectWorks() {
			AssertCorrect(
@"public class D {}
public void M() {
	object o;
	// BEGIN
	D d = (D)o;
	object[] arr = (object[])o;
	// END
}",
@"	var $d = $Cast($o, {D});
	var $arr = $Cast($o, $Array({Object}));
");
		}

		[Test]
		public void CastingToDynamicWorks() {
			AssertCorrect(
@"public class C {}
public void M() {
	C c = null;
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
@"public class C {}
public void M() {
	C[] c = null;
	// BEGIN
	Array a1 = (Array)c;
	Array a2 = (Array)c;
	// END
}",
@"	var $a1 = $Upcast($c, {Array});
	var $a2 = $Upcast($c, {Array});
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
@"	var $d = $Cast($a, $Array({D}));
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
@"	var $o1 = $Upcast($c1, $InstantiateGenericType({IEnumerable}, {Object}));
	var $o2 = $Upcast($c2, $InstantiateGenericType({IEnumerable}, {Object}));
");
		}

		[Test]
		public void NullLiteralToReferenceTypeWorks() {
			AssertCorrect(
@"class C {}
public void M() {
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
@"class C {}
public void M() {
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
@"class C {}
public void M() {
	dynamic d = null;
	// BEGIN
	System.Collections.Generic.List<object> l = (System.Collections.Generic.List<object>)d;
	// END
}",
@"	var $l = $Cast($d, $InstantiateGenericType({List}, {Object}));
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
@"	var $x1 = $Cast($d, {Int32});
	var $x2 = $Cast($d, {Double});
	var $x3 = $Cast($d, {Boolean});
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
@"	var $x1 = $FromNullable($Cast($d, {Int32}));
	var $x2 = $FromNullable($Cast($d, {Double}));
	var $x3 = $FromNullable($Cast($d, {Boolean}));
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
@"	var $v1 = $Upcast($i, {ValueType});
	var $v2 = $Upcast($i, {ValueType});
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
@"	var $i1 = $FromNullable($Cast($v, {Int32}));
	var $i2 = $Cast($v, {Int32});
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
@"	var $e1 = 0;
	var $e2 = 0;
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
@"	var $l1 = $Upcast($src, $InstantiateGenericType({IList}, {B}));
	var $l2 = $Upcast($src, $InstantiateGenericType({IList}, {B}));
	var $l3 = $Upcast($src, $InstantiateGenericType({IList}, {D}));
	var $l4 = $Upcast($src, $InstantiateGenericType({IList}, {D}));
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
@"	var $l = $Cast($src, $InstantiateGenericType({IList}, {D}));
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
@"	var $d1 = $Upcast($f, {Delegate});
	var $d2 = $Upcast($f, {Delegate});
");
		}

		[Test]
		public void DelegateTypeCanBeVarianceConvertedToOtherDelegateType() {
			AssertCorrect(
@"class B {}
class D : B {}

public void M() {
	Func<B, D> f;
	// BEGIN
	Func<D, B> f2 = f;
	// END
}",
@"	var $f2 = $Upcast($f, $InstantiateGenericType({Func}, {D}, {B}));
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
@"	var $f = $Cast($d, $InstantiateGenericType({Func}, {Int32}));
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
@"	var $o1 = $Upcast($t, {Object});
	var $o2 = $Upcast($t, {Object});
	var $d1 = $Upcast($t, {D});
	var $d2 = $Upcast($t, {D});
	var $i1 = $Upcast($t, {I});
	var $i2 = $Upcast($t, {I});
");

			AssertCorrect(
@"public class D {}
public interface I {}
public void M<T>() where T : class, D, I {
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
@"	var $o1 = $Upcast($t, {Object});
	var $o2 = $Upcast($t, {Object});
	var $d1 = $Upcast($t, {D});
	var $d2 = $Upcast($t, {D});
	var $i1 = $Upcast($t, {I});
	var $i2 = $Upcast($t, {I});
");
		}

		[Test, Ignore("NRefactory bug")]
		public void TypeParameterCanBeConvertedToOtherTypeParameterOnWhichItDepends() {
			AssertCorrect(
@"public void M<T, U>() where T : U {
	T t = default(T);
	// BEGIN
	U u1 = (U)t;
	U u2 = t;
	// END
}",
@"	var $u1 = $Upcast($t, {U});
	var $u2 = $Upcast($t, {U});
");

			AssertCorrect(
@"public void M<T, U>() where T : class, U {
	T t = default(T);
	// BEGIN
	U u1 = (U)t;
	U u2 = t;
	// END
}",
@"	var $u1 = $Upcast($t, {U});
	var $u2 = $Upcast($t, {U});
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

			AssertCorrect(
@"public class B {}
public class D : B {}
public void M<T>() where T : class, D {
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
@"	var $i = $Cast($t, {I});
");

			AssertCorrect(
@"public interface I {}
public void M<T>() where T : class {
	T t = default(T);
	// BEGIN
	I i = (I)t;
	// END
}",
@"	var $i = $Cast($t, {I});
");
		}

		[Test, Ignore("NRefactory bug")]
		public void TypeParameterCanBeConvertedToOtherTypeParameterWhichDependsOnIt() {
			AssertCorrect(
@"public void M<T, U>() where U : T {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, {U});
");

			AssertCorrect(
@"public void M<T, U>() where U : class, T {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, {U});
");

			AssertCorrect(
@"public void M<T, U>() where U : T where T : class {
	T t = default(T);
	// BEGIN
	U u = (U)t;
	// END
}",
@"	var $u = $Cast($t, {U});
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
@"	var $c11 = {C2}.$op_Implicit($c2);
	var $c12 = {C2}.$op_Implicit($c2);
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
@"	var $c1 = {C2}.$op_Explicit($c2);
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
@"	var $ni1 = {C1}.$op_Implicit($c1);
	var $ni2 = {C1}.$op_Implicit($c1);
	var $c2 = {C1}.$op_Implicit($i);
	var $c3 = {C1}.$op_Implicit($i);
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
@"	var $ni = {C1}.$op_Explicit($c1);
	var $c2 = {C1}.$op_Explicit($i);
");
		}
	}
}
