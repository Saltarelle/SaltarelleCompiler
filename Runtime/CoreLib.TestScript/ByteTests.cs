using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is byte);
			Assert.IsFalse((object)0.5 is byte);
			Assert.AreEqual(typeof(byte).FullName, "ss.Int32");
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
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(byte.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(byte.Parse("234", 16), 0x234);
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
