using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class Int16Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(short)0 is short);
			Assert.IsFalse((object)0.5 is short);
			Assert.AreEqual(typeof(short).FullName, "ss.Int32");
			Assert.IsFalse(typeof(short).IsClass);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<short>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new short(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<short>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(short.MinValue, -32768);
			Assert.AreEqual(short.MaxValue, 32767);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((short)0x123).Format("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((short)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(short.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(short.Parse("234", 16), 0x234);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((short)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((short)123).ToString(10), "123");
			Assert.AreEqual(((short)0x123).ToString(16), "123");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((short)0).GetHashCode(), ((short)0).GetHashCode());
			Assert.AreEqual   (((short)1).GetHashCode(), ((short)1).GetHashCode());
			Assert.AreNotEqual(((short)0).GetHashCode(), ((short)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((short)0).Equals((short)0));
			Assert.IsFalse(((short)1).Equals((short)0));
			Assert.IsFalse(((short)0).Equals((short)1));
			Assert.IsTrue (((short)1).Equals((short)1));
		}
	}
}
