using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class EnumTests {
		[PreserveMemberCase]
		public enum TestEnum {
			FirstValue,
			SecondValue,
			ThirdValue
		}

		[PreserveMemberCase]
		[Flags]
		public enum FlagsEnum {
			None = 0,
			FirstValue = 1,
			SecondValue = 2,
			ThirdValue = 4
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Enum).FullName, "ss.Enum");
			Assert.AreEqual(typeof(TestEnum).FullName, "CoreLibTests.EnumTests$TestEnum");
			Assert.IsTrue(typeof(TestEnum).IsEnum);
			Assert.IsFalse(typeof(TestEnum).IsFlags);
			Assert.IsTrue(typeof(FlagsEnum).IsEnum);
			Assert.IsTrue(typeof(FlagsEnum).IsFlags);
			Assert.IsTrue((object)TestEnum.FirstValue is TestEnum);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueOfEnumClassIsNull() {
			Assert.AreStrictEqual(GetDefaultValue<Enum>(), null);
		}

		[Test]
		public void DefaultValueOfEnumTypeIsZero() {
			Assert.AreStrictEqual(GetDefaultValue<TestEnum>(), 0);
		}

		[Test]
		public void DefaultConstructorOfEnumTypeReturnsZero() {
			Assert.AreStrictEqual(new TestEnum(), 0);
		}

		[Test]
		public void CreatingInstanceOfEnumTypeReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<TestEnum>(), 0);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual((TestEnum)Enum.Parse(typeof(TestEnum), "FirstValue"), TestEnum.FirstValue);
			Assert.AreEqual((TestEnum)Enum.Parse(typeof(FlagsEnum), "FirstValue | ThirdValue"), FlagsEnum.FirstValue | FlagsEnum.ThirdValue);
		}

		[Test]
		public void StaticToStringWorks() {
			Assert.AreEqual(Enum.ToString(typeof(TestEnum), TestEnum.FirstValue), "FirstValue");
			Assert.AreEqual(Enum.ToString(typeof(FlagsEnum), FlagsEnum.FirstValue | FlagsEnum.ThirdValue), "FirstValue | ThirdValue");
		}
	}
}
