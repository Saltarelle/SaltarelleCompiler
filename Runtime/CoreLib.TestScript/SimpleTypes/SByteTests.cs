﻿using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class SByteTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(byte)0 is sbyte);
			Assert.IsFalse((object)0.5 is sbyte);
			Assert.AreEqual(typeof(sbyte).FullName, "ss.Int32");
			Assert.IsFalse(typeof(sbyte).IsClass);
			Assert.IsTrue(typeof(IComparable<sbyte>).IsAssignableFrom(typeof(sbyte)));
			Assert.IsTrue(typeof(IEquatable<sbyte>).IsAssignableFrom(typeof(sbyte)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(sbyte)));
			object b = (sbyte)0;
			Assert.IsTrue(b is sbyte);
			Assert.IsTrue(b is IComparable<sbyte>);
			Assert.IsTrue(b is IEquatable<sbyte>);
			Assert.IsTrue(b is IFormattable);

			var interfaces = typeof(sbyte).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<sbyte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<sbyte>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
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
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((sbyte)0x12).ToString("x"), "12");
			Assert.AreEqual(((IFormattable)((sbyte)0x12)).ToString("x"), "12");
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
		public void TryParseWorks() {
			sbyte numberResult;
			bool result = sbyte.TryParse("124", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 124);

			result = sbyte.TryParse("-123", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, -123);

			result = sbyte.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("54768", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = sbyte.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
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
			Assert.IsTrue (((sbyte)0).Equals((object)(sbyte)0));
			Assert.IsFalse(((sbyte)1).Equals((object)(sbyte)0));
			Assert.IsFalse(((sbyte)0).Equals((object)(sbyte)1));
			Assert.IsTrue (((sbyte)1).Equals((object)(sbyte)1));
		}


		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((sbyte)0).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)1).Equals((sbyte)0));
			Assert.IsFalse(((sbyte)0).Equals((sbyte)1));
			Assert.IsTrue (((sbyte)1).Equals((sbyte)1));

			Assert.IsTrue (((IEquatable<sbyte>)((sbyte)0)).Equals((sbyte)0));
			Assert.IsFalse(((IEquatable<sbyte>)((sbyte)1)).Equals((sbyte)0));
			Assert.IsFalse(((IEquatable<sbyte>)((sbyte)0)).Equals((sbyte)1));
			Assert.IsTrue (((IEquatable<sbyte>)((sbyte)1)).Equals((sbyte)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((sbyte)0).CompareTo((sbyte)0) == 0);
			Assert.IsTrue(((sbyte)1).CompareTo((sbyte)0) > 0);
			Assert.IsTrue(((sbyte)0).CompareTo((sbyte)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)0)).CompareTo((sbyte)0) == 0);
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)1)).CompareTo((sbyte)0) > 0);
			Assert.IsTrue(((IComparable<sbyte>)((sbyte)0)).CompareTo((sbyte)1) < 0);
		}
	}
}
