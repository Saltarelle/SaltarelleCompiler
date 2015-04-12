using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace CoreLib.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class EnumerationConversionTests {
		const string Enums = "enum ESB : sbyte {} enum EB : byte {} enum ES : short {} enum EUS : ushort {} enum EI : int {} enum EUI : uint {} enum EL : long {} enum EUL : ulong {}";

		[Test]
		public void UncheckedTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		ulong src = 0;
		long src2 = 0;

		unchecked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src2;
			// END
		}
	}
}",
@"				var sb = ss.sxb(src & 255);
				var b = src & 255;
				var s = ss.sxs(src & 65535);
				var us = src & 65535;
				var i = src | 0;
				var ui = src >>> 0;
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedNonTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		byte src = 0;
		sbyte src2 = 0;

		unchecked {
			// BEGIN
			var sb = (ESB)src2;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void UncheckedFloatToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		double src = 0;

		unchecked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = ss.sxb(src & 255);
				var b = src & 255;
				var s = ss.sxs(src & 65535);
				var us = src & 65535;
				var i = src | 0;
				var ui = src >>> 0;
				var l = ss.clip64(src);
				var ul = ss.clipu64(src);
");
		}

		[Test]
		public void UncheckedLiftedTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		ulong? src = 0;
		long? src2 = 0;

		unchecked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src2;
			// END
		}
	}
}",
@"				var sb = ss.clip8(src);
				var b = ss.clipu8(src);
				var s = ss.clip16(src);
				var us = ss.clipu16(src);
				var i = ss.clip32(src);
				var ui = ss.clipu32(src);
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedLiftedNonTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		byte? src = 0;
		sbyte? src2 = 0;

		unchecked {
			// BEGIN
			var sb = (ESB?)src2;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void UncheckedLiftedFloatToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		double? src = 0;

		unchecked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = ss.clip8(src);
				var b = ss.clipu8(src);
				var s = ss.clip16(src);
				var us = ss.clipu16(src);
				var i = ss.clip32(src);
				var ui = ss.clipu32(src);
				var l = ss.clip64(src);
				var ul = ss.clipu64(src);
");
		}

		[Test]
		public void UncheckedTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL src = default(EUL);
		EL src2 = default(EL);

		unchecked {
			// BEGIN
			var sb = (sbyte)src;
			var b  = (byte)src;
			var s  = (short)src;
			var us = (ushort)src;
			var i  = (int)src;
			var ui = (uint)src;
			var l  = (long)src;
			var ul = (ulong)src2;
			// END
		}
	}
}",
@"				var sb = ss.sxb(src & 255);
				var b = src & 255;
				var s = ss.sxs(src & 65535);
				var us = src & 65535;
				var i = src | 0;
				var ui = src >>> 0;
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedNonTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EB src = default(EB);
		ESB src2 = default(ESB);

		unchecked {
			// BEGIN
			var sb = (sbyte)src2;
			var b  = (byte)src;
			var s  = (short)src;
			var us = (ushort)src;
			var i  = (int)src;
			var ui = (uint)src;
			var l  = (long)src;
			var ul = (ulong)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void UncheckedEnumToFloat() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EL src = 0;

		unchecked {
			// BEGIN
			var f = (float)src;
			var d  = (double)src;
			var m  = (decimal)src;
			// END
		}
	}
}",
@"				var f = src;
				var d = src;
				var m = src;
");
		}

		[Test]
		public void UncheckedLiftedTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL? src = null;
		EL? src2 = null;

		unchecked {
			// BEGIN
			var sb = (sbyte?)src;
			var b  = (byte?)src;
			var s  = (short?)src;
			var us = (ushort?)src;
			var i  = (int?)src;
			var ui = (uint?)src;
			var l  = (long?)src;
			var ul = (ulong?)src2;
			// END
		}
	}
}",
@"				var sb = ss.clip8(src);
				var b = ss.clipu8(src);
				var s = ss.clip16(src);
				var us = ss.clipu16(src);
				var i = ss.clip32(src);
				var ui = ss.clipu32(src);
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedLiftedNonTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EB? src = null;
		ESB? src2 = null;

		unchecked {
			// BEGIN
			var sb = (sbyte?)src2;
			var b  = (byte?)src;
			var s  = (short?)src;
			var us = (ushort?)src;
			var i  = (int?)src;
			var ui = (uint?)src;
			var l  = (long?)src;
			var ul = (ulong?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void UncheckedLiftedEnumToFloat() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EL? src = 0;

		unchecked {
			// BEGIN
			var f = (float?)src;
			var d  = (double?)src;
			var m  = (decimal?)src;
			// END
		}
	}
}",
@"				var f = src;
				var d = src;
				var m = src;
");
		}

		[Test]
		public void UncheckedTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL src = default(EUL);
		EL src2 = default(EL);

		unchecked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src2;
			// END
		}
	}
}",
@"				var sb = ss.sxb(src & 255);
				var b = src & 255;
				var s = ss.sxs(src & 65535);
				var us = src & 65535;
				var i = src | 0;
				var ui = src >>> 0;
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedNonTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
enum ESB2 : sbyte {} enum EB2 : byte {}
class C {
	void M() {
		EB2 src = default(EB2);
		ESB2 src2 = default(ESB2);

		unchecked {
			// BEGIN
			var sb = (ESB)src2;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void UncheckedLiftedTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL? src = null;
		EL? src2 = null;

		unchecked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src2;
			// END
		}
	}
}",
@"				var sb = ss.clip8(src);
				var b = ss.clipu8(src);
				var s = ss.clip16(src);
				var us = ss.clipu16(src);
				var i = ss.clip32(src);
				var ui = ss.clipu32(src);
				var l = ss.clip64(src);
				var ul = ss.clipu64(src2);
");
		}

		[Test]
		public void UncheckedLiftedNonTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
enum ESB2 : sbyte {} enum EB2 : byte {}
class C {
	void M() {
		EB2? src = null;
		ESB2? src2 = null;

		unchecked {
			// BEGIN
			var sb = (ESB?)src2;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		ulong src = 0;
		long src2 = 0;

		checked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedNonTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		byte src = 0;
		sbyte src2 = 0;

		checked {
			// BEGIN
			var sb = (ESB)src2;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedFloatToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		double src = 0;

		checked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = ss.ck(ss.trunc(src), ss.SByte);
				var b = ss.ck(ss.trunc(src), ss.Byte);
				var s = ss.ck(ss.trunc(src), ss.Int16);
				var us = ss.ck(ss.trunc(src), ss.UInt16);
				var i = ss.ck(ss.trunc(src), ss.Int32);
				var ui = ss.ck(ss.trunc(src), ss.UInt32);
				var l = ss.ck(ss.trunc(src), ss.Int64);
				var ul = ss.ck(ss.trunc(src), ss.UInt64);
");
		}

		[Test]
		public void CheckedLiftedTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		ulong? src = 0;
		long? src2 = 0;

		checked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedLiftedNonTruncatingIntToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		byte? src = 0;
		sbyte? src2 = 0;

		checked {
			// BEGIN
			var sb = (ESB?)src2;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedLiftedFloatToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		double? src = 0;

		checked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = ss.ck(ss.trunc(src), ss.SByte);
				var b = ss.ck(ss.trunc(src), ss.Byte);
				var s = ss.ck(ss.trunc(src), ss.Int16);
				var us = ss.ck(ss.trunc(src), ss.UInt16);
				var i = ss.ck(ss.trunc(src), ss.Int32);
				var ui = ss.ck(ss.trunc(src), ss.UInt32);
				var l = ss.ck(ss.trunc(src), ss.Int64);
				var ul = ss.ck(ss.trunc(src), ss.UInt64);
");
		}

		[Test]
		public void CheckedTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL src = default(EUL);
		EL src2 = default(EL);

		checked {
			// BEGIN
			var sb = (sbyte)src;
			var b  = (byte)src;
			var s  = (short)src;
			var us = (ushort)src;
			var i  = (int)src;
			var ui = (uint)src;
			var l  = (long)src;
			var ul = (ulong)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedNonTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EB src = default(EB);
		ESB src2 = default(ESB);

		checked {
			// BEGIN
			var sb = (sbyte)src2;
			var b  = (byte)src;
			var s  = (short)src;
			var us = (ushort)src;
			var i  = (int)src;
			var ui = (uint)src;
			var l  = (long)src;
			var ul = (ulong)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedEnumToFloat() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EL src = 0;

		checked {
			// BEGIN
			var f = (float)src;
			var d  = (double)src;
			var m  = (decimal)src;
			// END
		}
	}
}",
@"				var f = src;
				var d = src;
				var m = src;
");
		}

		[Test]
		public void CheckedLiftedTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL? src = null;
		EL? src2 = null;

		checked {
			// BEGIN
			var sb = (sbyte?)src;
			var b  = (byte?)src;
			var s  = (short?)src;
			var us = (ushort?)src;
			var i  = (int?)src;
			var ui = (uint?)src;
			var l  = (long?)src;
			var ul = (ulong?)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedLiftedNonTruncatingEnumToInt() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EB? src = null;
		ESB? src2 = null;

		checked {
			// BEGIN
			var sb = (sbyte?)src2;
			var b  = (byte?)src;
			var s  = (short?)src;
			var us = (ushort?)src;
			var i  = (int?)src;
			var ui = (uint?)src;
			var l  = (long?)src;
			var ul = (ulong?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedLiftedEnumToFloat() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EL? src = 0;

		checked {
			// BEGIN
			var f = (float?)src;
			var d  = (double?)src;
			var m  = (decimal?)src;
			// END
		}
	}
}",
@"				var f = src;
				var d = src;
				var m = src;
");
		}

		[Test]
		public void CheckedTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL src = default(EUL);
		EL src2 = default(EL);

		checked {
			// BEGIN
			var sb = (ESB)src;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedNonTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
enum ESB2 : sbyte {} enum EB2 : byte {}
class C {
	void M() {
		EB2 src = default(EB2);
		ESB2 src2 = default(ESB2);

		checked {
			// BEGIN
			var sb = (ESB)src2;
			var b  = (EB)src;
			var s  = (ES)src;
			var us = (EUS)src;
			var i  = (EI)src;
			var ui = (EUI)src;
			var l  = (EL)src;
			var ul = (EUL)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void CheckedLiftedTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
class C {
	void M() {
		EUL? src = null;
		EL? src2 = null;

		checked {
			// BEGIN
			var sb = (ESB?)src;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src2;
			// END
		}
	}
}",
@"				var sb = ss.ck(src, ss.SByte);
				var b = ss.ck(src, ss.Byte);
				var s = ss.ck(src, ss.Int16);
				var us = ss.ck(src, ss.UInt16);
				var i = ss.ck(src, ss.Int32);
				var ui = ss.ck(src, ss.UInt32);
				var l = ss.ck(src, ss.Int64);
				var ul = ss.ck(src2, ss.UInt64);
");
		}

		[Test]
		public void CheckedLiftedNonTruncatingEnumToEnum() {
			SourceVerifier.AssertSourceCorrect(Enums + @"
enum ESB2 : sbyte {} enum EB2 : byte {}
class C {
	void M() {
		EB2? src = null;
		ESB2? src2 = null;

		checked {
			// BEGIN
			var sb = (ESB?)src2;
			var b  = (EB?)src;
			var s  = (ES?)src;
			var us = (EUS?)src;
			var i  = (EI?)src;
			var ui = (EUI?)src;
			var l  = (EL?)src;
			var ul = (EUL?)src;
			// END
		}
	}
}",
@"				var sb = src2;
				var b = src;
				var s = src;
				var us = src;
				var i = src;
				var ui = src;
				var l = src;
				var ul = src;
");
		}

		[Test]
		public void NamedValuesEnumToOtherNamedValuesEnumIsAnNop() {
			SourceVerifier.AssertSourceCorrect(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 : byte {}
[NamedValues] enum E2 : int {}

class C {
	void M() {
		E1 e1 = default(E1);
		E2 e2 = default(E2);

		// BEGIN
		unchecked {
			var x1 = (E2)e1;
			var x2 = (E1)e2;
		}
		checked {
			var x3 = (E2)e1;
			var x4 = (E1)e2;
		}
		// END
	}
}",
@"			{
				var x1 = e1;
				var x2 = e2;
			}
			{
				var x3 = e1;
				var x4 = e2;
			}
");
		}

		[Test]
		public void LiftedNamedValuesEnumToOtherNamedValuesEnumIsAnNop() {
			SourceVerifier.AssertSourceCorrect(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 : byte {}
[NamedValues] enum E2 : int {}

class C {
	void M() {
		E1? e1 = null;
		E2? e2 = null;

		// BEGIN
		unchecked {
			var x1 = (E2?)e1;
			var x2 = (E1?)e2;
		}
		checked {
			var x3 = (E2?)e1;
			var x4 = (E1?)e2;
		}
		// END
	}
}",
@"			{
				var x1 = e1;
				var x2 = e2;
			}
			{
				var x3 = e1;
				var x4 = e2;
			}
");
		}

		[Test]
		public void NumberToNamedValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}

class C {
	public void M() {
		int i = 1;
		var e = (E1)i;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("Int32")));
		}

		[Test]
		public void LiftedNumberToNamedValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}

class C {
	public void M() {
		int? i = 1;
		var e = (E1?)i;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("Int32")));
		}

		[Test]
		public void NamedValuesEnumToNumberIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}

class C {
	public void M() {
		var e = default(E1);
		var i = (int)e;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("Int32")));
		}

		[Test]
		public void LiftedNamedValuesEnumToNumberIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}

class C {
	public void M() {
		var e = default(E1?);
		var i = (int?)e;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("Int32")));
		}

		[Test]
		public void NamedValuesEnumToNumericValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}
enum E2 {}

class C {
	public void M() {
		var e1 = default(E1);
		var e2 = (E2)e1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("E2")));
		}

		[Test]
		public void LiftedNamedValuesEnumToNumericValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
[NamedValues] enum E1 {}
enum E2 {}

class C {
	public void M() {
		var e1 = default(E1?);
		var e2 = (E2?)e1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("E2")));
		}

		[Test]
		public void NumericValuesEnumToNamedValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
enum E1 {}
[NamedValues] enum E2 {}

class C {
	public void M() {
		var e1 = default(E1);
		var e2 = (E2)e1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("E2")));
		}

		[Test]
		public void LiftedNumericValuesEnumToNamedValuesEnumIsAnError() {
			var actual = SourceVerifier.Compile(@"
using System.Runtime.CompilerServices;
enum E1 {}
[NamedValues] enum E2 {}

class C {
	public void M() {
		var e1 = default(E1?);
		var e2 = (E2?)e1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7703 && m.FormattedMessage.Contains("E1") && m.FormattedMessage.Contains("E2")));
		}
	}
}
