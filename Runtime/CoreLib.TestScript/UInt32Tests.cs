using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class UInt32Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(int)0 is uint);
			Assert.IsFalse((object)0.5 is uint);
			Assert.AreEqual(typeof(uint).FullName, "ss.Int32");
			Assert.IsFalse(typeof(uint).IsClass);
			Assert.IsTrue(typeof(IComparable<uint>).IsAssignableFrom(typeof(uint)));
			Assert.IsTrue(typeof(IEquatable<uint>).IsAssignableFrom(typeof(uint)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(uint)));
			object i = (uint)0;
			Assert.IsTrue(i is uint);
			Assert.IsTrue(i is IComparable<uint>);
			Assert.IsTrue(i is IEquatable<uint>);
			Assert.IsTrue(i is IFormattable);

			var interfaces = typeof(uint).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<uint>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<uint>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<uint>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new uint(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<uint>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(uint.MinValue, 0);
			Assert.AreEqual(uint.MaxValue, 4294967295U);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((uint)0x123).Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((uint)0x123).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((uint)0x123)).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((uint)0x123).LocaleFormat("x"), "123");
		}

		[Test]
		public void ParseWithoutRadixWorks() {
			Assert.AreEqual(uint.Parse("234"), 234);
		}

		[Test]
		public void ParseWithRadixWorks() {
			Assert.AreEqual(uint.Parse("234", 16), 0x234);
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(((uint)123).ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(((uint)123).ToString(10), "123");
			Assert.AreEqual(((uint)0x123).ToString(16), "123");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((uint)0).GetHashCode(), ((uint)0).GetHashCode());
			Assert.AreEqual   (((uint)1).GetHashCode(), ((uint)1).GetHashCode());
			Assert.AreNotEqual(((uint)0).GetHashCode(), ((uint)1).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((uint)0).Equals((object)(uint)0));
			Assert.IsFalse(((uint)1).Equals((object)(uint)0));
			Assert.IsFalse(((uint)0).Equals((object)(uint)1));
			Assert.IsTrue (((uint)1).Equals((object)(uint)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((uint)0).Equals((uint)0));
			Assert.IsFalse(((uint)1).Equals((uint)0));
			Assert.IsFalse(((uint)0).Equals((uint)1));
			Assert.IsTrue (((uint)1).Equals((uint)1));

			Assert.IsTrue (((IEquatable<uint>)((uint)0)).Equals((uint)0));
			Assert.IsFalse(((IEquatable<uint>)((uint)1)).Equals((uint)0));
			Assert.IsFalse(((IEquatable<uint>)((uint)0)).Equals((uint)1));
			Assert.IsTrue (((IEquatable<uint>)((uint)1)).Equals((uint)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((uint)0).CompareTo((uint)0) == 0);
			Assert.IsTrue(((uint)1).CompareTo((uint)0) > 0);
			Assert.IsTrue(((uint)0).CompareTo((uint)1) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<uint>)((uint)0)).CompareTo((uint)0) == 0);
			Assert.IsTrue(((IComparable<uint>)((uint)1)).CompareTo((uint)0) > 0);
			Assert.IsTrue(((IComparable<uint>)((uint)0)).CompareTo((uint)1) < 0);
		}
	}
}
