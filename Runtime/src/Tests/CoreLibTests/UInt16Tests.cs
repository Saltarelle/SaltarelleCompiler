using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class UInt16Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(short)0 is ushort);
			Assert.IsFalse((object)0.5 is ushort);
			Assert.AreEqual(typeof(ushort).FullName, "ss.Int32");
			Assert.IsFalse(typeof(ushort).IsClass);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<ushort>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new ushort(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<ushort>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(ushort.MinValue, 0);
			Assert.AreEqual(ushort.MaxValue, 65535);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((ushort)0x123).Format("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((ushort)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(ushort.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(ushort.Parse("234", 16), 0x234);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((ushort)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((ushort)123).ToString(10), "123");
			Assert.AreEqual(((ushort)0x123).ToString(16), "123");
		}
	}
}
