using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class UInt16Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(short)0 is ushort);
			Assert.IsFalse((object)0.5 is ushort);
			Assert.AreEqual(typeof(ushort).FullName, "ss.Int32");
			Assert.IsFalse(typeof(ushort).IsClass);
			Assert.IsTrue(typeof(IComparable<ushort>).IsAssignableFrom(typeof(ushort)));
			Assert.IsTrue(typeof(IEquatable<ushort>).IsAssignableFrom(typeof(ushort)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(ushort)));
			object s = (ushort)0;
			Assert.IsTrue(s is ushort);
			Assert.IsTrue(s is IComparable<ushort>);
			Assert.IsTrue(s is IEquatable<ushort>);
			Assert.IsTrue(s is IFormattable);

			var interfaces = typeof(ushort).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<ushort>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<ushort>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
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
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((ushort)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((ushort)0x123)).ToString("x"), "123");
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
		public void TryParseWorks() {
			ushort numberResult;
			bool result = ushort.TryParse("23445", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 23445);

			result = ushort.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ushort.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ushort.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ushort.TryParse("32768", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 32768);

			result = ushort.TryParse("-1", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = ushort.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
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

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((ushort)0).GetHashCode(), ((ushort)0).GetHashCode());
			Assert.AreEqual   (((ushort)1).GetHashCode(), ((ushort)1).GetHashCode());
			Assert.AreNotEqual(((ushort)0).GetHashCode(), ((ushort)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((ushort)0).Equals((object)(ushort)0));
			Assert.IsFalse(((ushort)1).Equals((object)(ushort)0));
			Assert.IsFalse(((ushort)0).Equals((object)(ushort)1));
			Assert.IsTrue (((ushort)1).Equals((object)(ushort)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((ushort)0).Equals((ushort)0));
			Assert.IsFalse(((ushort)1).Equals((ushort)0));
			Assert.IsFalse(((ushort)0).Equals((ushort)1));
			Assert.IsTrue (((ushort)1).Equals((ushort)1));

			Assert.IsTrue (((IEquatable<ushort>)((ushort)0)).Equals((ushort)0));
			Assert.IsFalse(((IEquatable<ushort>)((ushort)1)).Equals((ushort)0));
			Assert.IsFalse(((IEquatable<ushort>)((ushort)0)).Equals((ushort)1));
			Assert.IsTrue (((IEquatable<ushort>)((ushort)1)).Equals((ushort)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((ushort)0).CompareTo((ushort)0) == 0);
			Assert.IsTrue(((ushort)1).CompareTo((ushort)0) > 0);
			Assert.IsTrue(((ushort)0).CompareTo((ushort)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<ushort>)((ushort)0)).CompareTo((ushort)0) == 0);
			Assert.IsTrue(((IComparable<ushort>)((ushort)1)).CompareTo((ushort)0) > 0);
			Assert.IsTrue(((IComparable<ushort>)((ushort)0)).CompareTo((ushort)1) < 0);
		}
	}
}
