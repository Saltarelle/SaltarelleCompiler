using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	// DONE
	// Implicit numeric
	// Explicit numeric

	// Conversions:
	// Identity
	// Implicit enum
	// Implicit nullable
	// Null literal to Nullable<T>
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
	// Boxing
	// Implicit dynamic
	// Implicit type param to base
	// op_Implicit
	// Explicit nullable
	// Explicit reference
	// Explicit interface
	// Unboxing
	// Explicit dynamic
	// op_Explicit
	// Explicit nullable
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
	}
}
