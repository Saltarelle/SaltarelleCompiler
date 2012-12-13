using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class NullableTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			int? a = 3, b = null;
			Assert.AreEqual(typeof(int?).FullName, "ss.Nullable");
			Assert.IsTrue((object)a is int?);
			Assert.IsFalse((object)b is int?);
		}

		[Test]
		public void ConvertingToNullableWorks() {
			int i = 3;
			int? i1 = new int?(i);
			int? i2 = i;
			Assert.AreEqual(i1, 3);
			Assert.AreEqual(i2, 3);
		}

		[Test]
		public void HasValueWorks() {
			int? a = 3, b = null;
			Assert.IsTrue(a.HasValue);
			Assert.IsFalse(b.HasValue);
		}

		[Test]
		public void BoxingWorks() {
			int? a = 3, b = null;
			Assert.IsTrue((object)a != null);
			Assert.IsFalse((object)b != null);
		}

		[Test]
		public void UnboxingWorks() {
			int? a = 3, b = null;
			Assert.AreEqual((int)a, 3);
			try {
#pragma warning disable 219
				int x = (int)b;
#pragma warning restore 219
				Assert.IsTrue(false, "Unboxing null should have thrown an exception");
			}
			catch (Exception) {
			}
		}

		[Test]
		public void ValueWorks() {
			int? a = 3, b = null;
			Assert.AreEqual(a.Value, 3);
			try {
#pragma warning disable 219
				int x = b.Value;
#pragma warning restore 219
				Assert.IsTrue(false, "null.Value should have thrown an exception");
			}
			catch (Exception) {
			}
		}

		[Test]
		public void GetValueOrDefaultWithArgWorks() {
			int? a = 3, b = null;
			Assert.AreEqual(a.GetValueOrDefault(1), 3);
			Assert.AreEqual(b.GetValueOrDefault(1), 1);
		}

		[Test]
		public void LiftedEqualityWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a == b, true);
			Assert.AreStrictEqual(a == c, false);
			Assert.AreStrictEqual(a == d, false);
			Assert.AreStrictEqual(d == e, true);
		}

		[Test]
		public void LiftedInequalityWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a != b, false);
			Assert.AreStrictEqual(a != c, true);
			Assert.AreStrictEqual(a != d, true);
			Assert.AreStrictEqual(d != e, false);
		}

		[Test]
		public void LiftedLessThanWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a < b, false);
			Assert.AreStrictEqual(a < c, true);
			Assert.AreStrictEqual(a < d, false);
			Assert.AreStrictEqual(d < e, false);
		}

		[Test]
		public void LiftedGreaterThanWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a > b, false);
			Assert.AreStrictEqual(c > a, true);
			Assert.AreStrictEqual(a > d, false);
			Assert.AreStrictEqual(d > e, false);
		}

		[Test]
		public void LiftedLessThanOrEqualWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a <= b, true);
			Assert.AreStrictEqual(c <= a, false);
			Assert.AreStrictEqual(a <= d, false);
			Assert.AreStrictEqual(d <= e, false);
		}

		[Test]
		public void LiftedGreaterThanOrEqualWorks() {
			int? a = 1, b = 1, c = 2, d = null, e = null;
			Assert.AreStrictEqual(a >= b, true);
			Assert.AreStrictEqual(a >= c, false);
			Assert.AreStrictEqual(a >= d, false);
			Assert.AreStrictEqual(d >= e, false);
		}

		[Test]
		public void LiftedSubtractionWorks() {
			int? a = 2, b = 3, c = null;
			Assert.AreStrictEqual(a - b, -1);
			Assert.AreStrictEqual(a - c, null);
		}

		[Test]
		public void LiftedAdditionWorks() {
			int? a = 2, b = 3, c = null;
			Assert.AreStrictEqual(a + b, 5);
			Assert.AreStrictEqual(a + c, null);
		}

		[Test]
		public void LiftedModWorks() {
			int? a = 14, b = 3, c = null;
			Assert.AreStrictEqual(a % b, 2);
			Assert.AreStrictEqual(a % c, null);
		}

		[Test]
		public void LiftedFloatingPointDivisionWorks() {
			double? a = 15, b = 3, c = null;
			Assert.AreStrictEqual(a / b, 5);
			Assert.AreStrictEqual(a / c, null);
		}

		[Test]
		public void LiftedIntegerDivisionWorks() {
			int? a = 16, b = 3, c = null;
			Assert.AreStrictEqual(a / b, 5);
			Assert.AreStrictEqual(a / c, null);
		}

		[Test]
		public void LiftedMultiplicationWorks() {
			int? a = 2, b = 3, c = null;
			Assert.AreStrictEqual(a * b, 6);
			Assert.AreStrictEqual(a * c, null);
		}

		[Test]
		public void LiftedBitwiseAndWorks() {
			int? a = 6, b = 3, c = null;
			Assert.AreStrictEqual(a & b, 2);
			Assert.AreStrictEqual(a & c, null);
		}

		[Test]
		public void LiftedBitwiseOrWorks() {
			int? a = 6, b = 3, c = null;
			Assert.AreStrictEqual(a | b, 7);
			Assert.AreStrictEqual(a | c, null);
		}

		[Test]
		public void LiftedBitwiseXorWorks() {
			int? a = 6, b = 3, c = null;
			Assert.AreStrictEqual(a ^ b, 5);
			Assert.AreStrictEqual(a ^ c, null);
		}

		[Test]
		public void LiftedLeftShiftWorks() {
			int? a = 6, b = 3, c = null;
			Assert.AreStrictEqual(a << b, 48);
			Assert.AreStrictEqual(a << c, null);
		}

		[Test]
		public void LiftedSignedRightShiftWorks() {
			int? a = 48, b = 3, c = null;
			Assert.AreStrictEqual(a >> b, 6);
			Assert.AreStrictEqual(a >> c, null);
		}

		[Test]
		public void LiftedUnsignedRightShiftWorks() {
			int? a = -48, b = 3, c = null;
			Assert.AreStrictEqual(a >> b, -6);
			Assert.AreStrictEqual(a >> c, null);
		}

		[Test]
		public void LiftedBooleanAndWorks() {
			bool? a = true, b = true, c = false, d = false, e = null, f = null;
			Assert.AreStrictEqual(a & b, true);
			Assert.AreStrictEqual(a & c, false);
			Assert.AreStrictEqual(a & e, null);
			Assert.AreStrictEqual(c & a, false);
			Assert.AreStrictEqual(c & d, false);
			Assert.AreStrictEqual(c & e, false);
			Assert.AreStrictEqual(e & a, null);
			Assert.AreStrictEqual(e & c, false);
			Assert.AreStrictEqual(e & f, null);
		}

		[Test]
		public void LiftedBooleanOrWorks() {
			bool? a = true, b = true, c = false, d = false, e = null, f = null;
			Assert.AreStrictEqual(a | b, true);
			Assert.AreStrictEqual(a | c, true);
			Assert.AreStrictEqual(a | e, true);
			Assert.AreStrictEqual(c | a, true);
			Assert.AreStrictEqual(c | d, false);
			Assert.AreStrictEqual(c | e, null);
			Assert.AreStrictEqual(e | a, true);
			Assert.AreStrictEqual(e | c, null);
			Assert.AreStrictEqual(e | f, null);
		}

		[Test]
		public void LiftedBooleanNotWorks() {
			bool? a = true, b = false, c = null;
			Assert.AreStrictEqual(!a, false);
			Assert.AreStrictEqual(!b, true);
			Assert.AreStrictEqual(!c, null);
		}

		[Test]
		public void LiftedNegationWorks() {
			int? a = 3, b = null;
			Assert.AreStrictEqual(-a, -3);
			Assert.AreStrictEqual(-b, null);
		}

		[Test]
		public void LiftedUnaryPlusWorks() {
			int? a = 3, b = null;
			Assert.AreStrictEqual(+a, +3);
			Assert.AreStrictEqual(+b, null);
		}

		[Test]
		public void LiftedOnesComplementWorks() {
			int? a = 3, b = null;
			Assert.AreStrictEqual(~a, -4);
			Assert.AreStrictEqual(~b, null);
		}
	}
}
