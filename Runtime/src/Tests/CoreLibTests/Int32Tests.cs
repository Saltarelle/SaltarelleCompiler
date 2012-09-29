using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class Int32Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(int)0 is int);
			Assert.IsFalse((object)0.5 is int);
			Assert.AreEqual(typeof(int).FullName, "ss.Int32");
			Assert.IsFalse(typeof(int).IsClass);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<int>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new int(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<int>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(int.MinValue, -2147483648);
			Assert.AreEqual(int.MaxValue, 2147483647);
		}


		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((int)0x123).Format("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((int)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(int.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(int.Parse("234", 16), 0x234);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((int)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((int)123).ToString(10), "123");
			Assert.AreEqual(((int)0x123).ToString(16), "123");
		}
	}
}
