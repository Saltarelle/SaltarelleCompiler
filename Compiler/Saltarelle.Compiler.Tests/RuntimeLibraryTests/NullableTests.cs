using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class NullableTests : RuntimeLibraryTestBase {
		[Test]
		public void CastingNonNullValueToUnderlyingTypeWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static int M() {
		int? value = 3;
		return (int)value;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(3));
		}

		[Test]
		public void AccessingValuePropertyOnNonNullValueWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static int M() {
		int? value = 4;
		return value.Value;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(4));
		}

		[Test]
		public void HasValueReturnsTrueWhenTheValueIsNotNull() {
			var result = ExecuteCSharp(@"
public class C {
	public static bool M() {
		int? value = 4;
		return value.HasValue;
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void CastingNullValueToUnderlyingTypeThrowsAnException() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool M() {
		return DoesItThrow(() => {
			int? v = null;
			int x = (int)v;
		});
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void AccessingValuePropertyOnNullNullableThrowsAnException() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool M() {
		return DoesItThrow(() => {
			int? v = null;
			int x = v.Value;
		});
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void HasValueReturnsFalseWhenTheValueIsNull() {
			var result = ExecuteCSharp(@"
public class C {
	public static bool M() {
		int? value = null;
		return value.HasValue;
	}
}", "C.M");
			Assert.That(result, Is.False);
		}

		[Test]
		public void UnbokingIntegerWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static int M() {
		object value = 5;
		return (int)value;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(5));
		}

		[Test]
		public void UnboxingValueOfWrongTypeThrowsAnException() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool M() {
		return DoesItThrow(() => {
			object o = ""x"";
			int x = (int)o;
		});
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void UnboxingNullThrowsAnException() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool M() {
		return DoesItThrow(() => {
			object o = null;
			int x = (int)o;
		});
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void NullableConstructorCanBeUsed() {
			var result = ExecuteCSharp(@"
public class C {
	public static int? M() {
		var v = new System.Nullable<int>(7);
		return v;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(7));
		}

		[Test]
		public void LiftedOperatorsWork() {
			var result = ExecuteCSharp(@"
public class C {
	public static object[] M() {
		int? i1 = null, i2 = 17, i3 = 4;
		uint? u1 = null, u2 = 17000;
		double? d1 = null, d2 = 3, d3 = 2;
		int? x1 = null, x2 = 10, x3 = null, x4 = 20, x5 = null, x6 = 30, x7 = null, x8 = 40;
		bool? b1 = null, b2 = true, b3 = false;
		return new object[] {
			+i1,             // null
			+i2,             // 17
			-i1,             // null
			-i2,             // -17
			++x1,            // null
			++x2,            // 11
			x3++,            // null
			x4++,            // 20
			--x5,            // null
			--x6,            // 29
			x7--,            // null
			x8--,            // 40
			!b1,             // null
			!b2,             // false
			~i1,             // null
			~i2,             // -18
			(i1 + i2),       // null
			(i2 + i3),       // 21
			(i1 - i2),       // null
			(i2 - i3),       // 13
			(i1 * i2),       // null
			(i2 * i3),       // 68
			(i1 / i2),       // null
			(i2 / i3),       // 4
			(d1 / d2),       // null
			(d2 / d3),       // 1.5
			(i1 % i2),       // null
			(i2 % i3),       // 1
			(i1 & i2),       // null
			(i2 & i3),       // 0
			(i1 | i2),       // null
			(i2 | i3),       // 21
			(i1 ^ i2),	     // null
			(i2 ^ i3),	     // 21
			(i1 << i2),	     // null
			(i2 << i3),	     // 272
			(i1 >> i2),	     // null
			(i2 >> i3),	     // 1
			(u1 >> i2),	     // null
			(u2 >> i3),	     // 1062
			(i1 == i2),	     // false
			(i2 == i3),	     // false
			(i1 != i2),	     // true
			(i2 != i3),	     // true
			(i1 < i2),	     // false
			(i2 < i3),	     // false
			(i1 <= i2),	     // false
			(i2 <= i3),	     // false
			(i1 > i2),	     // false
			(i2 > i3),	     // true
			(i1 >= i2),	     // false
			(i2 >= i3),	     // true
			b1 & b2,	     // null
			b1 & b3,	     // false
			b1 | b2,	     // true
			b1 | b3,	     // null
			x1,              // null
			x2,              // 11
			x3,              // null
			x4,              // 21
			x5,              // null
			x6,              // 29
			x7,              // null
			x8,              // 39
		};
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object[] {
				null,
				17,
				null,
				-17,
				null,
				11,
				null,
				20,
				null,
				29,
				null,
				40,
				null,
				false,
				null,
				-18,
				null,
				21,
				null,
				13,
				null,
				68,
				null,
				4,
				null,
				1.5,
				null,
				1,
				null,
				0,
				null,
				21,
				null,
				21,
				null,
				272,
				null,
				1,
				null,
				1062,
				false,
				false,
				true,
				true,
				false,
				false,
				false,
				false,
				false,
				true,
				false,
				true,
				null,
				false,
				true,
				null,
				null,
				11,
				null,
				21,
				null,
				29,
				null,
				39,
			}));
		}
	}
}
