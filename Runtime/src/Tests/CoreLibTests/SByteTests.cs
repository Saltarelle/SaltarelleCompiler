using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class SByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is sbyte);
			Assert.IsFalse((object)0.5 is sbyte);
			Assert.AreEqual(typeof(sbyte).FullName, "ss.Int32");
			Assert.IsFalse(typeof(sbyte).IsClass);
		}

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
		public void LocaleFormatWorks() {
			Assert.AreEqual(((sbyte)0x12).LocaleFormat("x"), "12");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(sbyte.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(sbyte.Parse("234", 16), 0x234);
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
			Assert.IsTrue (((sbyte)0).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)1).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)0).Equals((sbyte)1));
			Assert.IsTrue (((sbyte)1).Equals((sbyte)1));
		}
	}
}
