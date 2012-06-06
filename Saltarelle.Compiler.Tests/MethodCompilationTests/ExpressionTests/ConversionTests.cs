using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	// DONE
	// Identity
	// Implicit numeric
	// Explicit numeric
	// To nullable
	// From nullable
	// Null literal to Nullable<T>
	// Explicit nullable
	// Boxing
	// Unboxing
	// Explicit reference
	// Explicit interface

	// Conversions:
	// Implicit enum
	// Implicit reference:
	//   Implicit reference to base class
	//   Implicit reference to interface
	//   Implicit reference to dynamic
	//   T[] to S[] if T : S
	//   T[] to Array
	//   T[] to IList<S> if T : S
	//   delegate => System.Delegate
	//   Null literal to reference
	//   IEnumerable<S> to IEnumerable<T> if S : T (or List<S> to IEnumerable<T>)
	// Implicit dynamic
	// Implicit type param to base
	// op_Implicit
	// Explicit dynamic
	// op_Explicit
	// Explicit reference:
	//    object => T
	//    dynamic => T
	//    C => IF if C does not implement IF
	//    IF => T, if T is not sealed or T implements IF
	//    IF1 => IF2, if IF1 is not derived from IF2
	//    T[] to S[] if S : T
	//    Array to T[]
	//    T[] to IList<S> if explicit conv from S to T
	//    Delegate => delegate
	//    variance
	//    variance (delegate)
	//    Explicit type param conversions

	// TryCast (and TryUnbox)



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
	var $x1 = $FromNullable($Unbox($o, {Int32}));
	var $x2 = $Unbox($o, {Int32});
	var $x3 = $FromNullable($Unbox($o, {Double}));
	var $x4 = $Unbox($o, {Double});
	var $x5 = $FromNullable($Unbox($o, {Boolean}));
	var $x6 = $Unbox($o, {Boolean});
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
	}
}
