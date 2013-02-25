using System;
using System.Runtime.CompilerServices;
using QUnit;

#pragma warning disable 184, 219, 458

namespace CoreLib.TestScript {
	[TestFixture]
	public class Int32Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(int)0 is int);
			Assert.IsFalse((object)0.5 is int);
			Assert.AreEqual(typeof(int).FullName, "ss.Int32");
			Assert.IsFalse(typeof(int).IsClass);
			Assert.IsTrue(typeof(IComparable<int>).IsAssignableFrom(typeof(int)));
			Assert.IsTrue(typeof(IEquatable<int>).IsAssignableFrom(typeof(int)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(int)));
			object i = (int)0;
			Assert.IsTrue(i is int);
			Assert.IsTrue(i is IComparable<int>);
			Assert.IsTrue(i is IEquatable<int>);
			Assert.IsTrue(i is IFormattable);

			var interfaces = typeof(int).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<int>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<int>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[Test]
		public void TypeIsWorksForInt32() {
			Assert.IsFalse((object)null is int);
			Assert.IsFalse((object)1.5 is int);
			Assert.IsFalse(new object() is int);
			Assert.IsTrue ((object)1 is int);
		}

		[Test]
		public void TypeAsWorksForInt32() {
			Assert.IsFalse((null as int?) != null);
			Assert.IsFalse((new object() as int?) != null);
			Assert.IsFalse(((object)1.5 as int?) != null);
			Assert.IsTrue ((1 as int?) != null);
		}

		[Test]
		public void UnboxingWorksForInt32() {
			object _null = null;
			object o = new object();
			object d = 1.5;
			object i = 1;
			Assert.AreEqual((int?)_null, null);
			Assert.Throws(() => { var _ = (int?)o; });
			Assert.Throws(() => { var _ = (int?)d; });
			Assert.AreEqual((int?)i, 1);
		}

		[IncludeGenericArguments]
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
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((int)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((int)0x123)).ToString("x"), "123");
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
		public void TryParseWorks()
		{
			int numberResult;
			bool result = int.TryParse("57574", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 57574);

			result = int.TryParse("-14", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, -14);

			result = int.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = int.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = int.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
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

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((int)0).GetHashCode(), ((int)0).GetHashCode());
			Assert.AreEqual   (((int)1).GetHashCode(), ((int)1).GetHashCode());
			Assert.AreNotEqual(((int)0).GetHashCode(), ((int)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((int)0).Equals((object)(int)0));
			Assert.IsFalse(((int)1).Equals((object)(int)0));
			Assert.IsFalse(((int)0).Equals((object)(int)1));
			Assert.IsTrue (((int)1).Equals((object)(int)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((int)0).Equals((int)0));
			Assert.IsFalse(((int)1).Equals((int)0));
			Assert.IsFalse(((int)0).Equals((int)1));
			Assert.IsTrue (((int)1).Equals((int)1));

			Assert.IsTrue (((IEquatable<int>)((int)0)).Equals((int)0));
			Assert.IsFalse(((IEquatable<int>)((int)1)).Equals((int)0));
			Assert.IsFalse(((IEquatable<int>)((int)0)).Equals((int)1));
			Assert.IsTrue (((IEquatable<int>)((int)1)).Equals((int)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((int)0).CompareTo((int)0) == 0);
			Assert.IsTrue(((int)1).CompareTo((int)0) > 0);
			Assert.IsTrue(((int)0).CompareTo((int)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<int>)((int)0)).CompareTo((int)0) == 0);
			Assert.IsTrue(((IComparable<int>)((int)1)).CompareTo((int)0) > 0);
			Assert.IsTrue(((IComparable<int>)((int)0)).CompareTo((int)1) < 0);
		}

		[Test]
		public void IntegerDivisionWorks() {
			int a = 17, b = 4;
			Assert.AreEqual(a / b, 4);
			Assert.AreEqual(-a / b, -4);
			Assert.AreEqual(a / -b, -4);
			Assert.AreEqual(-a / -b, 4);
		}

		[Test]
		public void DoublesAreTruncatedWhenConvertedToIntegers() {
			double d1 = 4.5;
			double? d2 = null;
			double? d3 = 8.5;
			Assert.AreEqual((int)d1, 4);
			Assert.AreEqual((int)-d1, -4);
			Assert.AreEqual((int?)d2, null);
			Assert.AreEqual((int)d3, 8);
			Assert.AreEqual((int)-d3, -8);
			Assert.AreEqual((int?)d3, 8);
			Assert.AreEqual((int?)-d3, -8);
		}
	}
}
