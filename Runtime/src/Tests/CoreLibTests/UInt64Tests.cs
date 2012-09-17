using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class UInt64Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(ulong)0 is ulong);
			Assert.IsFalse((object)0.5 is ulong);
			Assert.AreEqual(typeof(ulong).FullName, "ss.Int32");
			Assert.IsFalse(typeof(ulong).IsClass);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<ulong>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new ulong(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(ulong.MinValue, 0);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((ulong)0x123).Format("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((ulong)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(ulong.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(ulong.Parse("234", 16), 0x234);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((ulong)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((ulong)123).ToString(10), "123");
			Assert.AreEqual(((ulong)0x123).ToString(16), "123");
		}
	}
}
