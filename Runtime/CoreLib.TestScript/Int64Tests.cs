using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class Int64Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(long)0 is long);
			Assert.IsFalse((object)0.5 is long);
			Assert.AreEqual(typeof(long).FullName, "ss.Int32");
			Assert.IsFalse(typeof(long).IsClass);
			Assert.IsTrue(typeof(IComparable<long>).IsAssignableFrom(typeof(long)));
			Assert.IsTrue(typeof(IEquatable<long>).IsAssignableFrom(typeof(long)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(long)));
			object l = (long)0;
			Assert.IsTrue(l is long);
			Assert.IsTrue(l is IComparable<long>);
			Assert.IsTrue(l is IEquatable<long>);
			Assert.IsTrue(l is IFormattable);

			var interfaces = typeof(long).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<long>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<long>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<long>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new long(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<long>(), 0);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((long)0x123).Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((long)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((long)0x123)).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((long)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(long.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(long.Parse("234", 16), 0x234);
		}

		[Test]
		public void TryParseWorks() {
			long numberResult;
			bool result = long.TryParse("57574", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 57574);

			result = long.TryParse("-14", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, -14);

			result = long.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = long.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((long)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((long)123).ToString(10), "123");
			Assert.AreEqual(((long)0x123).ToString(16), "123");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((long)0).GetHashCode(), ((long)0).GetHashCode());
			Assert.AreEqual   (((long)1).GetHashCode(), ((long)1).GetHashCode());
			Assert.AreNotEqual(((long)0).GetHashCode(), ((long)1).GetHashCode());
			Assert.IsTrue((long)0x100000000L.GetHashCode() <= 0xffffffffL);
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((long)0).Equals((object)(long)0));
			Assert.IsFalse(((long)1).Equals((object)(long)0));
			Assert.IsFalse(((long)0).Equals((object)(long)1));
			Assert.IsTrue (((long)1).Equals((object)(long)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((long)0).Equals((long)0));
			Assert.IsFalse(((long)1).Equals((long)0));
			Assert.IsFalse(((long)0).Equals((long)1));
			Assert.IsTrue (((long)1).Equals((long)1));

			Assert.IsTrue (((IEquatable<long>)((long)0)).Equals((long)0));
			Assert.IsFalse(((IEquatable<long>)((long)1)).Equals((long)0));
			Assert.IsFalse(((IEquatable<long>)((long)0)).Equals((long)1));
			Assert.IsTrue (((IEquatable<long>)((long)1)).Equals((long)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((long)0).CompareTo((long)0) == 0);
			Assert.IsTrue(((long)1).CompareTo((long)0) > 0);
			Assert.IsTrue(((long)0).CompareTo((long)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<long>)((long)0)).CompareTo((long)0) == 0);
			Assert.IsTrue(((IComparable<long>)((long)1)).CompareTo((long)0) > 0);
			Assert.IsTrue(((IComparable<long>)((long)0)).CompareTo((long)1) < 0);
		}
	}
}
