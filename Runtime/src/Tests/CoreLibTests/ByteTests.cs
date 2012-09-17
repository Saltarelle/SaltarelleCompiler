using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class ByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is byte);
			Assert.IsFalse((object)0.5 is byte);
			Assert.AreEqual(typeof(byte).FullName, "ss.Int32");
			Assert.IsFalse(typeof(byte).IsClass);
		}

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
		public void ConstantsWork() {
			Assert.AreEqual(byte.MinValue, 0);
			Assert.AreEqual(byte.MaxValue, 255);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((byte)0x12).Format("x"), "12");
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
	}
}
