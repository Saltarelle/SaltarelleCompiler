using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class NumericConversionTests  : MethodCompilerTestBase {
		[Test]
		public void ImplicitFromSByte() {
			AssertCorrect(
@"public void M() {
	sbyte src = 0;

	// BEGIN
	short   s  = src;
	int     i  = src;
	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $s = $src;
	var $i = $src;
	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromByte() {
			AssertCorrect(
@"public void M() {
	byte src = 0;

	// BEGIN
	short   s  = src;
	ushort  us = src;
	int     i  = src;
	uint    ui = src;
	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $s = $src;
	var $us = $src;
	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromShort() {
			AssertCorrect(
@"public void M() {
	short src = 0;

	// BEGIN
	int     i  = src;
	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $i = $src;
	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromUShort() {
			AssertCorrect(
@"public void M() {
	ushort src = 0;

	// BEGIN
	int     i  = src;
	uint    ui = src;
	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $i = $src;
	var $ui = $src;
	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromInt() {
			AssertCorrect(
@"public void M() {
	int src = 0;

	// BEGIN
	long    l  = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $l = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromUInt() {
			AssertCorrect(
@"public void M() {
	uint src = 0;

	// BEGIN
	long    l  = src;
	ulong   ul = src;
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $l = $src;
	var $ul = $src;
	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromLong() {
			AssertCorrect(
@"public void M() {
	long src = 0;

	// BEGIN
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromULong() {
			AssertCorrect(
@"public void M() {
	ulong src = 0;

	// BEGIN
	float   fl = src;
	double  db = src;
	decimal dc = src;
	// END
}",
@"	var $fl = $src;
	var $db = $src;
	var $dc = $src;
");
		}

		[Test]
		public void ImplicitFromFloat() {
			AssertCorrect(
@"public void M() {
	float src = 0;

	// BEGIN
	double db = src;
	// END
}",
@"	var $db = $src;
");
		}

		[Test]
		public void ExplicitFromSByte() {
			AssertCorrect(
@"public void M() {
	sbyte src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}
	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $src;
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $src;
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $src;
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $src;
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $src;
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $src;
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $src;
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $src;
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromByte() {
			AssertCorrect(
@"public void M() {
	byte src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $src;
		var $s1 = $src;
		var $us1 = $src;
		var $i1 = $src;
		var $ui1 = $src;
		var $l1 = $src;
		var $ul1 = $src;
		var $c1 = $src;
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $src;
		var $s2 = $src;
		var $us2 = $src;
		var $i2 = $src;
		var $ui2 = $src;
		var $l2 = $src;
		var $ul2 = $src;
		var $c2 = $src;
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromShort() {
			AssertCorrect(
@"public void M() {
	short src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $src;
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $src;
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $src;
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $src;
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $src;
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $src;
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromUShort() {
			AssertCorrect(
@"public void M() {
	ushort src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $src;
		var $i1 = $src;
		var $ui1 = $src;
		var $l1 = $src;
		var $ul1 = $src;
		var $c1 = $src;
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $src;
		var $i2 = $src;
		var $ui2 = $src;
		var $l2 = $src;
		var $ul2 = $src;
		var $c2 = $src;
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromInt() {
			AssertCorrect(
@"public void M() {
	int src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $src;
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $src;
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $src;
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $src;
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromUInt() {
			AssertCorrect(
@"public void M() {
	uint src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $src;
		var $l1 = $src;
		var $ul1 = $src;
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $src;
		var $l2 = $src;
		var $ul2 = $src;
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromLong() {
			AssertCorrect(
@"public void M() {
	long src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $src;
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $src;
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromULong() {
			AssertCorrect(
@"public void M() {
	ulong src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $Narrow($src, {ct_Int64});
		var $ul1 = $src;
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $NarrowChecked($src, {ct_Int64});
		var $ul2 = $src;
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromChar() {
			AssertCorrect(
@"public void M() {
	char src = '\0';

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $src;
		var $i1 = $src;
		var $ui1 = $src;
		var $l1 = $src;
		var $ul1 = $src;
		var $c1 = $src;
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $src;
		var $i2 = $src;
		var $ui2 = $src;
		var $l2 = $src;
		var $ul2 = $src;
		var $c2 = $src;
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromFloat() {
			AssertCorrect(
@"public void M() {
	float src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $Narrow($src, {ct_Int64});
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $NarrowChecked($src, {ct_Int64});
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromDouble() {
			AssertCorrect(
@"public void M() {
	double src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $Narrow($src, {ct_Int64});
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $NarrowChecked($src, {ct_Int64});
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}

		[Test]
		public void ExplicitFromDecimal() {
			AssertCorrect(
@"public void M() {
	decimal src = 0;

	// BEGIN
	unchecked {
		sbyte   sb1 = (sbyte)src;
		byte    b1  = (byte)src;
		short   s1  = (short)src;
		ushort  us1 = (ushort)src;
		int     i1  = (int)src;
		uint    ui1 = (uint)src;
		long    l1  = (long)src;
		ulong   ul1 = (ulong)src;
		char    c1  = (char)src;
		float   fl1 = (float)src;
		double  db1 = (double)src;
		decimal dc1 = (decimal)src;
	}

	checked {
		sbyte   sb2 = (sbyte)src;
		byte    b2  = (byte)src;
		short   s2  = (short)src;
		ushort  us2 = (ushort)src;
		int     i2  = (int)src;
		uint    ui2 = (uint)src;
		long    l2  = (long)src;
		ulong   ul2 = (ulong)src;
		char    c2  = (char)src;
		float   fl2 = (float)src;
		double  db2 = (double)src;
		decimal dc2 = (decimal)src;
	}
	// END
}",
@"	{
		var $sb1 = $Narrow($src, {ct_SByte});
		var $b1 = $Narrow($src, {ct_Byte});
		var $s1 = $Narrow($src, {ct_Int16});
		var $us1 = $Narrow($src, {ct_UInt16});
		var $i1 = $Narrow($src, {ct_Int32});
		var $ui1 = $Narrow($src, {ct_UInt32});
		var $l1 = $Narrow($src, {ct_Int64});
		var $ul1 = $Narrow($src, {ct_UInt64});
		var $c1 = $Narrow($src, {ct_Char});
		var $fl1 = $src;
		var $db1 = $src;
		var $dc1 = $src;
	}
	{
		var $sb2 = $NarrowChecked($src, {ct_SByte});
		var $b2 = $NarrowChecked($src, {ct_Byte});
		var $s2 = $NarrowChecked($src, {ct_Int16});
		var $us2 = $NarrowChecked($src, {ct_UInt16});
		var $i2 = $NarrowChecked($src, {ct_Int32});
		var $ui2 = $NarrowChecked($src, {ct_UInt32});
		var $l2 = $NarrowChecked($src, {ct_Int64});
		var $ul2 = $NarrowChecked($src, {ct_UInt64});
		var $c2 = $NarrowChecked($src, {ct_Char});
		var $fl2 = $src;
		var $db2 = $src;
		var $dc2 = $src;
	}
");
		}
	}
}
