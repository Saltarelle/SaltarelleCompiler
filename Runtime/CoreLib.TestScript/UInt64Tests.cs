using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class UInt64Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(ulong)0 is ulong);
			Assert.IsFalse((object)0.5 is ulong);
			Assert.AreEqual(typeof(ulong).FullName, "ss.Int32");
			Assert.IsFalse(typeof(ulong).IsClass);
			Assert.IsTrue(typeof(IComparable<ulong>).IsAssignableFrom(typeof(ulong)));
			Assert.IsTrue(typeof(IEquatable<ulong>).IsAssignableFrom(typeof(ulong)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(ulong)));
			object l = (ulong)0;
			Assert.IsTrue(l is ulong);
			Assert.IsTrue(l is IComparable<ulong>);
			Assert.IsTrue(l is IEquatable<ulong>);
			Assert.IsTrue(l is IFormattable);

			var interfaces = typeof(ulong).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<ulong>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<ulong>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
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
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<ulong>(), 0);
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
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((ulong)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((ulong)0x123)).ToString("x"), "123");
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
		public void TryParseWorks() {
			ulong numberResult;
			bool result = ulong.TryParse("23445", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 23445);

			result = ulong.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ulong.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ulong.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ulong.TryParse("-1", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ulong.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
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

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((ulong)0).GetHashCode(), ((ulong)0).GetHashCode());
			Assert.AreEqual   (((ulong)1).GetHashCode(), ((ulong)1).GetHashCode());
			Assert.AreNotEqual(((ulong)0).GetHashCode(), ((ulong)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((ulong)0).Equals((object)(ulong)0));
			Assert.IsFalse(((ulong)1).Equals((object)(ulong)0));
			Assert.IsFalse(((ulong)0).Equals((object)(ulong)1));
			Assert.IsTrue (((ulong)1).Equals((object)(ulong)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((ulong)0).Equals((ulong)0));
			Assert.IsFalse(((ulong)1).Equals((ulong)0));
			Assert.IsFalse(((ulong)0).Equals((ulong)1));
			Assert.IsTrue (((ulong)1).Equals((ulong)1));

			Assert.IsTrue (((IEquatable<ulong>)((ulong)0)).Equals((ulong)0));
			Assert.IsFalse(((IEquatable<ulong>)((ulong)1)).Equals((ulong)0));
			Assert.IsFalse(((IEquatable<ulong>)((ulong)0)).Equals((ulong)1));
			Assert.IsTrue (((IEquatable<ulong>)((ulong)1)).Equals((ulong)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((ulong)0).CompareTo((ulong)0) == 0);
			Assert.IsTrue(((ulong)1).CompareTo((ulong)0) > 0);
			Assert.IsTrue(((ulong)0).CompareTo((ulong)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<ulong>)((ulong)0)).CompareTo((ulong)0) == 0);
			Assert.IsTrue(((IComparable<ulong>)((ulong)1)).CompareTo((ulong)0) > 0);
			Assert.IsTrue(((IComparable<ulong>)((ulong)0)).CompareTo((ulong)1) < 0);
		}
	}
}
