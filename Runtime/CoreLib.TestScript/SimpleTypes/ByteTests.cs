using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class ByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is byte);
			Assert.IsFalse((object)0.5 is byte);
			Assert.IsFalse((object)-1 is byte);
			Assert.IsFalse((object)256 is byte);
			Assert.AreEqual(typeof(byte).FullName, "ss.Byte");
			Assert.IsFalse(typeof(byte).IsClass);
			Assert.IsTrue(typeof(IComparable<byte>).IsAssignableFrom(typeof(byte)));
			Assert.IsTrue(typeof(IEquatable<byte>).IsAssignableFrom(typeof(byte)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(byte)));
			object b = (byte)0;
			Assert.IsTrue(b is byte);
			Assert.IsTrue(b is IComparable<byte>);
			Assert.IsTrue(b is IEquatable<byte>);
			Assert.IsTrue(b is IFormattable);

			var interfaces = typeof(byte).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<byte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<byte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[Test]
		public void CastsWork() {
			int i1 = -1, i2 = 0, i3 = 234, i4 = 255, i5 = 256;
			int? ni1 = -1, ni2 = 0, ni3 = 234, ni4 = 255, ni5 = 256, ni6 = null;

			unchecked {
				Assert.AreStrictEqual((byte)i1, 255, "-1 unchecked");
				Assert.AreStrictEqual((byte)i2, 0, "0 unchecked");
				Assert.AreStrictEqual((byte)i3, 234, "234 unchecked");
				Assert.AreStrictEqual((byte)i4, 255, "255 unchecked");
				Assert.AreStrictEqual((byte)i5, 0, "256 unchecked");

				Assert.AreStrictEqual((byte?)ni1, 255, "nullable -1 unchecked");
				Assert.AreStrictEqual((byte?)ni2, 0, "nullable 0 unchecked");
				Assert.AreStrictEqual((byte?)ni3, 234, "nullable 234 unchecked");
				Assert.AreStrictEqual((byte?)ni4, 255, "nullable 255 unchecked");
				Assert.AreStrictEqual((byte?)ni5, 0, "nullable 256 unchecked");
				Assert.AreStrictEqual((byte?)ni6, null, "null unchecked");
			}

			checked {
				Assert.Throws<OverflowException>(() => { var x = (byte)i1; }, "-1 checked");
				Assert.AreStrictEqual((byte)i2, 0, "0 checked");
				Assert.AreStrictEqual((byte)i3, 234, "234 checked");
				Assert.AreStrictEqual((byte)i4, 255, "256 checked");
				Assert.Throws<OverflowException>(() => { var x = (byte)i5; }, "256 checked");

				Assert.Throws<OverflowException>(() => { var x = (byte?)ni1; }, "nullable -1 checked");
				Assert.AreStrictEqual((byte?)ni2, 0, "nullable 0 checked");
				Assert.AreStrictEqual((byte?)ni3, 234, "nullable 234 checked");
				Assert.AreStrictEqual((byte?)ni4, 255, "nullable 255 checked");
				Assert.Throws<OverflowException>(() => { var x = (byte?)ni5; }, "nullable 256 checked");
				Assert.AreStrictEqual((byte?)ni6, null, "null checked");
			}
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<byte>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new byte(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<byte>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(byte.MinValue, 0);
			Assert.AreEqual(byte.MaxValue, 255);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((byte)0x12).Format("x"), "12");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((byte)0x12).ToString("x"), "12");
			Assert.AreEqual(((IFormattable)((byte)0x12)).ToString("x"), "12");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((byte)0x12).LocaleFormat("x"), "12");
		}

		[Test]
		public void TryParseWorks() {
			byte numberResult;
			bool result = byte.TryParse("234", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 234);

			result = byte.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = byte.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = byte.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = byte.TryParse("54768", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = byte.TryParse("-1", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = byte.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(byte.Parse("234"), 234);
			Assert.Throws<FormatException>(() => byte.Parse(""));
			Assert.Throws<ArgumentNullException>(() => byte.Parse(null));
			Assert.Throws<FormatException>(() => byte.Parse("notanumber"));
			Assert.Throws<OverflowException>(() => byte.Parse("54768"));
			Assert.Throws<OverflowException>(() => byte.Parse("-1"));
			Assert.Throws<FormatException>(() => byte.Parse("2.5"));
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((byte)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((byte)123).ToString(10), "123");
			Assert.AreEqual(((byte)0x12).ToString(16), "12");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((byte)0).GetHashCode(), ((byte)0).GetHashCode());
			Assert.AreEqual   (((byte)1).GetHashCode(), ((byte)1).GetHashCode());
			Assert.AreNotEqual(((byte)0).GetHashCode(), ((byte)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((byte)0).Equals((object)(byte)0));
			Assert.IsFalse(((byte)1).Equals((object)(byte)0));
			Assert.IsFalse(((byte)0).Equals((object)(byte)1));
			Assert.IsTrue (((byte)1).Equals((object)(byte)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((byte)0).Equals((byte)0));
			Assert.IsFalse(((byte)1).Equals((byte)0));
			Assert.IsFalse(((byte)0).Equals((byte)1));
			Assert.IsTrue (((byte)1).Equals((byte)1));

			Assert.IsTrue (((IEquatable<byte>)((byte)0)).Equals((byte)0));
			Assert.IsFalse(((IEquatable<byte>)((byte)1)).Equals((byte)0));
			Assert.IsFalse(((IEquatable<byte>)((byte)0)).Equals((byte)1));
			Assert.IsTrue (((IEquatable<byte>)((byte)1)).Equals((byte)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((byte)0).CompareTo((byte)0) == 0);
			Assert.IsTrue(((byte)1).CompareTo((byte)0) > 0);
			Assert.IsTrue(((byte)0).CompareTo((byte)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<byte>)((byte)0)).CompareTo((byte)0) == 0);
			Assert.IsTrue(((IComparable<byte>)((byte)1)).CompareTo((byte)0) > 0);
			Assert.IsTrue(((IComparable<byte>)((byte)0)).CompareTo((byte)1) < 0);
		}
	}
}
