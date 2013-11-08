﻿using System;
using System.Runtime.CompilerServices;
using QUnit;

#pragma warning disable 219

namespace CoreLib.TestScript.SimpleTypes {
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
			Assert.AreEqual(typeof(TestEnum).FullName, "CoreLib.TestScript.SimpleTypes.EnumTests$TestEnum");
			Assert.IsTrue(typeof(TestEnum).IsEnum);
			Assert.IsFalse(typeof(TestEnum).IsFlags);
			Assert.IsTrue(typeof(FlagsEnum).IsEnum);
			Assert.IsTrue(typeof(FlagsEnum).IsFlags);
			Assert.IsTrue((object)TestEnum.FirstValue is TestEnum);

			var interfaces = typeof(TestEnum).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0);
		}

		[IncludeGenericArguments]
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
		public void DefaultExpressionWithEnumReturnsZero() {
			Assert.AreStrictEqual(default(TestEnum), 0);
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

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual(TestEnum.FirstValue.GetHashCode(), TestEnum.FirstValue.GetHashCode());
			Assert.AreNotEqual(TestEnum.FirstValue.GetHashCode(), TestEnum.SecondValue.GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue(TestEnum.FirstValue.Equals(TestEnum.FirstValue));
			Assert.IsFalse(TestEnum.FirstValue.Equals(TestEnum.SecondValue));
		}

		[Test]
		public void ConversionsToEnumAreTreatedAsConversionsToTheUnderlyingType() {
			Assert.AreEqual((TestEnum)(object)0, 0);
			Assert.Throws(() => { var _ = (TestEnum)(object)0.5; });
		}

		[Test]
		public void GetValuesWorks() {
			Array values = Enum.GetValues(typeof(TestEnum));
			Assert.AreEqual(values.Length, 3);
			Assert.AreEqual(values.GetValue(0), TestEnum.FirstValue);
			Assert.AreEqual(values.GetValue(1), TestEnum.SecondValue);
			Assert.AreEqual(values.GetValue(2), TestEnum.ThirdValue);

			values = Enum.GetValues(typeof(FlagsEnum));
			Assert.AreEqual(values.Length, 4);
			Assert.AreEqual(values.GetValue(0), FlagsEnum.None);
			Assert.AreEqual(values.GetValue(1), FlagsEnum.FirstValue);
			Assert.AreEqual(values.GetValue(2), FlagsEnum.SecondValue);
			Assert.AreEqual(values.GetValue(3), FlagsEnum.ThirdValue);
		}
	}
}
