using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class SByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is sbyte);
			Assert.IsFalse((object)0.5 is sbyte);
			Assert.IsFalse((object)-129 is sbyte);
			Assert.IsFalse((object)128 is sbyte);
			Assert.AreEqual(typeof(sbyte).FullName, "ss.SByte");
			Assert.IsFalse(typeof(sbyte).IsClass);
			Assert.IsTrue(typeof(IComparable<sbyte>).IsAssignableFrom(typeof(sbyte)));
			Assert.IsTrue(typeof(IEquatable<sbyte>).IsAssignableFrom(typeof(sbyte)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(sbyte)));
			object b = (sbyte)0;
			Assert.IsTrue(b is sbyte);
			Assert.IsTrue(b is IComparable<sbyte>);
			Assert.IsTrue(b is IEquatable<sbyte>);
			Assert.IsTrue(b is IFormattable);

			var interfaces = typeof(sbyte).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<sbyte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<sbyte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[Test]
		public void CastsWork() {
			int i1 = -129, i2 = -128, i3 = 80, i4 = 127, i5 = 128;
			int? ni1 = -129, ni2 = -128, ni3 = 80, ni4 = 127, ni5 = 128, ni6 = null;

			unchecked {
				Assert.AreStrictEqual((sbyte)i1, 127, "-129 unchecked");
				Assert.AreStrictEqual((sbyte)i2, -128, "-128 unchecked");
				Assert.AreStrictEqual((sbyte)i3, 80, "80 unchecked");
				Assert.AreStrictEqual((sbyte)i4, 127, "127 unchecked");
				Assert.AreStrictEqual((sbyte)i5, -128, "128 unchecked");

				Assert.AreStrictEqual((sbyte?)ni1, 127, "nullable -129 unchecked");
				Assert.AreStrictEqual((sbyte?)ni2, -128, "nullable -128 unchecked");
				Assert.AreStrictEqual((sbyte?)ni3, 80, "nullable 80 unchecked");
				Assert.AreStrictEqual((sbyte?)ni4, 127, "nullable 127 unchecked");
				Assert.AreStrictEqual((sbyte?)ni5, -128, "nullable 128 unchecked");
				Assert.AreStrictEqual((sbyte?)ni6, null, "null unchecked");
			}

			checked {
				Assert.Throws<OverflowException>(() => { var x = (byte)i1; }, "-129 checked");
				Assert.AreStrictEqual((sbyte)i2, -128, "-128 checked");
				Assert.AreStrictEqual((sbyte)i3, 80, "80 checked");
				Assert.AreStrictEqual((sbyte)i4, 127, "127 checked");
				Assert.Throws<OverflowException>(() => { var x = (sbyte)i5; }, "-128 checked");

				Assert.Throws<OverflowException>(() => { var x = (sbyte?)ni1; }, "nullable -129 checked");
				Assert.AreStrictEqual((sbyte?)ni2, -128, "nullable -128 checked");
				Assert.AreStrictEqual((sbyte?)ni3, 80, "nullable 80 checked");
				Assert.AreStrictEqual((sbyte?)ni4, 127, "nullable 127 checked");
				Assert.Throws<OverflowException>(() => { var x = (sbyte?)ni5; }, "nullable 128 checked");
				Assert.AreStrictEqual((sbyte?)ni6, null, "null checked");
			}
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<sbyte>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new sbyte(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<sbyte>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(sbyte.MinValue, -128);
			Assert.AreEqual(sbyte.MaxValue, 127);
		}


		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((sbyte)0x12).Format("x"), "12");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((sbyte)0x12).ToString("x"), "12");
			Assert.AreEqual(((IFormattable)((sbyte)0x12)).ToString("x"), "12");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((sbyte)0x12).LocaleFormat("x"), "12");
		}

		[Test]
		public void TryParseWorks() {
			sbyte numberResult;
			bool result = sbyte.TryParse("124", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 124);

			result = sbyte.TryParse("-123", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, -123);

			result = sbyte.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("54768", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(sbyte.Parse("124"), 124);
			Assert.AreEqual(sbyte.Parse("-123"), -123);
			Assert.Throws<FormatException>(() => sbyte.Parse(""));
			Assert.Throws<ArgumentNullException>(() => sbyte.Parse(null));
			Assert.Throws<FormatException>(() => sbyte.Parse("notanumber"));
			Assert.Throws<OverflowException>(() => sbyte.Parse("54768"));
			Assert.Throws<FormatException>(() => sbyte.Parse("2.5"));
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((sbyte)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((sbyte)123).ToString(10), "123");
			Assert.AreEqual(((sbyte)0x12).ToString(16), "12");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((sbyte)0).GetHashCode(), ((sbyte)0).GetHashCode());
			Assert.AreEqual   (((sbyte)1).GetHashCode(), ((sbyte)1).GetHashCode());
			Assert.AreNotEqual(((sbyte)0).GetHashCode(), ((sbyte)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((sbyte)0).Equals((object)(sbyte)0));
			Assert.IsFalse(((sbyte)1).Equals((object)(sbyte)0));
			Assert.IsFalse(((sbyte)0).Equals((object)(sbyte)1));
			Assert.IsTrue (((sbyte)1).Equals((object)(sbyte)1));
		}


		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((sbyte)0).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)1).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)0).Equals((sbyte)1));
			Assert.IsTrue (((sbyte)1).Equals((sbyte)1));

			Assert.IsTrue (((IEquatable<sbyte>)((sbyte)0)).Equals((sbyte)0));
			Assert.IsFalse(((IEquatable<sbyte>)((sbyte)1)).Equals((sbyte)0));
			Assert.IsFalse(((IEquatable<sbyte>)((sbyte)0)).Equals((sbyte)1));
			Assert.IsTrue (((IEquatable<sbyte>)((sbyte)1)).Equals((sbyte)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((sbyte)0).CompareTo((sbyte)0) == 0);
			Assert.IsTrue(((sbyte)1).CompareTo((sbyte)0) > 0);
			Assert.IsTrue(((sbyte)0).CompareTo((sbyte)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)0)).CompareTo((sbyte)0) == 0);
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)1)).CompareTo((sbyte)0) > 0);
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)0)).CompareTo((sbyte)1) < 0);
		}
	}
}
