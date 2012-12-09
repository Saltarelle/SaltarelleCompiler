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
			Assert.IsTrue(typeof(IComparable<int>).IsAssignableFrom(typeof(int)));
			Assert.IsTrue(typeof(IEquatable<int>).IsAssignableFrom(typeof(int)));
			object i = (int)0;
			Assert.IsTrue(i is int);
			Assert.IsTrue(i is IComparable<int>);
			Assert.IsTrue(i is IEquatable<int>);
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
	}
}
