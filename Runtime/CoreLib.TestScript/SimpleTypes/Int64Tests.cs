using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class Int64Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(long)0 is long);
			Assert.IsFalse((object)0.5 is long);
			Assert.IsFalse((object)1e100 is long);
			Assert.AreEqual(typeof(long).FullName, "ss.Int64");
			Assert.IsFalse(typeof(long).IsClass);
			Assert.IsTrue(typeof(IComparable<long>).IsAssignableFrom(typeof(long)));
			Assert.IsTrue(typeof(IEquatable<long>).IsAssignableFrom(typeof(long)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(long)));
			object l = (long)0;
			Assert.IsTrue(l is long);
			Assert.IsTrue(l is IComparable<long>);
			Assert.IsTrue(l is IEquatable<long>);
			Assert.IsTrue(l is IFormattable);

			var interfaces = typeof(long).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<long>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<long>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[Test]
		public void CastsWork() {
			ulong i3 = 5754, i4 = 9223372036854775000, i5 = 16223372036854776000;
			ulong? ni3 = 5754, ni4 = 9223372036854775000, ni5 = 16223372036854776000, ni6 = null;

			unchecked {
				Assert.AreStrictEqual((long)i3, 5754, "5754 unchecked");
				Assert.AreStrictEqual((long)i4, 9223372036854775000, "9223372036854775000 unchecked");
				Assert.IsTrue((long)i5 < 0, "16223372036854776000 unchecked");

				Assert.AreStrictEqual((long?)ni3, 5754, "nullable 5754 unchecked");
				Assert.AreStrictEqual((long?)ni4, 9223372036854775000, "nullable 9223372036854775000 unchecked");
				Assert.IsTrue((long?)ni5 < 0, "nullable 16223372036854776000 unchecked");
				Assert.AreStrictEqual((long?)ni6, null, "null unchecked");
			}

			checked {
				Assert.AreStrictEqual((long)i3, 5754, "5754 checked");
				Assert.AreStrictEqual((long)i4, 9223372036854775000, "9223372036854775000 checked");
				Assert.Throws<OverflowException>(() => { var x = (long)i5; }, "16223372036854776000 checked");

				Assert.AreStrictEqual((long?)ni3, 5754, "nullable 5754 checked");
				Assert.AreStrictEqual((long?)ni4, 9223372036854775000, "nullable 9223372036854775000 checked");
				Assert.Throws<OverflowException>(() => { var x = (long?)ni5; }, "nullable 16223372036854776000 checked");
				Assert.AreStrictEqual((long?)ni6, null, "null checked");
			}
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<long>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new long(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<long>(), 0);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((long)0x123).Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((long)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((long)0x123)).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((long)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void TryParseWorks() {
			long numberResult;
			bool result = long.TryParse("57574", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 57574);

			result = long.TryParse("-14", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, -14);

			result = long.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("-10000000000000000000", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("10000000000000000000", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(long.Parse("13453634535"), 13453634535);
			Assert.AreEqual(long.Parse("-234253069384953"), -234253069384953);
			Assert.Throws<FormatException>(() => long.Parse(""));
			Assert.Throws<ArgumentNullException>(() => long.Parse(null));
			Assert.Throws<FormatException>(() => long.Parse("notanumber"));
			Assert.Throws<FormatException>(() => long.Parse("2.5"));
			Assert.Throws<OverflowException>(() => long.Parse("-10000000000000000000"));
			Assert.Throws<OverflowException>(() => long.Parse("10000000000000000000"));
		}

		[Test]
		public void CastingOfLargeDoublesToInt64Works() {
			double d1 = 5e9 + 0.5, d2 = -d1;
			Assert.AreEqual((long)d1, 5000000000, "Positive");
			Assert.AreEqual((long)d2, -5000000000, "Negative");
		}

		[Test]
		public void DivisionOfLargeInt64Works() {
			long v1 = 50000000000L, v2 = -v1, v3 = 3;
			Assert.AreEqual(v1 / v3,  16666666666, "Positive");
			Assert.AreEqual(v2 / v3, -16666666666, "Negative");
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((long)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((long)123).ToString(10), "123");
			Assert.AreEqual(((long)0x123).ToString(16), "123");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((long)0).GetHashCode(), ((long)0).GetHashCode());
			Assert.AreEqual   (((long)1).GetHashCode(), ((long)1).GetHashCode());
			Assert.AreNotEqual(((long)0).GetHashCode(), ((long)1).GetHashCode());
			Assert.IsTrue((long)0x100000000L.GetHashCode() <= 0xffffffffL);
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((long)0).Equals((object)(long)0));
			Assert.IsFalse(((long)1).Equals((object)(long)0));
			Assert.IsFalse(((long)0).Equals((object)(long)1));
			Assert.IsTrue (((long)1).Equals((object)(long)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((long)0).Equals((long)0));
			Assert.IsFalse(((long)1).Equals((long)0));
			Assert.IsFalse(((long)0).Equals((long)1));
			Assert.IsTrue (((long)1).Equals((long)1));

			Assert.IsTrue (((IEquatable<long>)((long)0)).Equals((long)0));
			Assert.IsFalse(((IEquatable<long>)((long)1)).Equals((long)0));
			Assert.IsFalse(((IEquatable<long>)((long)0)).Equals((long)1));
			Assert.IsTrue (((IEquatable<long>)((long)1)).Equals((long)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((long)0).CompareTo((long)0) == 0);
			Assert.IsTrue(((long)1).CompareTo((long)0) > 0);
			Assert.IsTrue(((long)0).CompareTo((long)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<long>)((long)0)).CompareTo((long)0) == 0);
			Assert.IsTrue(((IComparable<long>)((long)1)).CompareTo((long)0) > 0);
			Assert.IsTrue(((IComparable<long>)((long)0)).CompareTo((long)1) < 0);
		}
	}
}
