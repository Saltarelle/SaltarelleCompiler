using System;
using QUnit;

#pragma warning disable 219

namespace CoreLib.TestScript {
	[TestFixture]
	public class NullableTests {
		private bool IsOfType<T>(object value) {
			return value is T;
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			int? a = 3, b = null;
			Assert.AreEqual(typeof(Nullable<>).FullName, "ss.Nullable$1", "Open FullName");
			Assert.AreEqual(typeof(int?).FullName, "ss.Nullable$1[[ss.Int32, mscorlib]]", "Instantiated FullName");
			Assert.IsTrue(typeof(Nullable<>).IsGenericTypeDefinition, "IsGenericTypeDefinition");
			Assert.AreEqual(typeof(int?).GetGenericTypeDefinition(), typeof(Nullable<>), "GetGenericTypeDefinition");
			Assert.IsTrue(typeof(int?).GetGenericArguments()[0] == typeof(int), "GenericArguments");
			Assert.IsTrue((object)a is int?, "is int? #1");
			Assert.IsFalse((object)b is int?, "is int? #2");

			Assert.IsTrue (IsOfType<int?>(3), "IsOfType #1");
			Assert.IsFalse(IsOfType<int?>(3.14), "IsOfType #2");
			Assert.IsTrue (IsOfType<TimeSpan?>(new TimeSpan(1)), "IsOfType #3");
			Assert.IsFalse(IsOfType<TimeSpan?>(3.14), "IsOfType #4");
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
				int x = (int)b;
				Assert.Fail("Unboxing null should have thrown an exception");
			}
			catch (InvalidOperationException) {
			}
		}

		[Test]
		public void ValueWorks() {
			int? a = 3, b = null;
			Assert.AreEqual(a.Value, 3);
			try {
				int x = b.Value;
				Assert.Fail("null.Value should have thrown an exception");
			}
			catch (InvalidOperationException) {
			}
		}

		[Test]
		public void UnboxingValueOfWrongTypeThrowsAnException() {
			Assert.Throws(() => {
				object o = "x";
				int x = (int)o;
			});
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
		public void LiftedBooleanXorWorks() {
			bool? a = true, b = true, c = false, d = false, e = null, f = null;
			Assert.AreStrictEqual(a ^ b, false);
			Assert.AreStrictEqual(a ^ c, true);
			Assert.AreStrictEqual(a ^ e, null);
			Assert.AreStrictEqual(c ^ a, true);
			Assert.AreStrictEqual(c ^ d, false);
			Assert.AreStrictEqual(c ^ e, null);
			Assert.AreStrictEqual(e ^ a, null);
			Assert.AreStrictEqual(e ^ c, null);
			Assert.AreStrictEqual(e ^ f, null);
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

		[Test]
		public void CoalesceWorks() {
			int? v1 = null, v2 = 1, v3 = 0, v4 = 2;
			string s1 = null, s2 = "x";
			Assert.AreStrictEqual(v1 ?? v1, null);
			Assert.AreStrictEqual(v1 ?? v2, 1);
			Assert.AreStrictEqual(v3 ?? v4, 0);
			Assert.AreStrictEqual(s1 ?? s1, null);
			Assert.AreStrictEqual(s1 ?? s2, "x");
		}
	}
}
