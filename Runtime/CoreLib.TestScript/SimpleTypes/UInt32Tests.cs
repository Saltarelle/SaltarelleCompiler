using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class UInt32Tests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(int)0 is uint);
			Assert.IsFalse((object)0.5 is uint);
			Assert.IsFalse((object)-1 is uint);
			Assert.IsFalse((object)4294967296 is uint);
			Assert.AreEqual(typeof(uint).FullName, "ss.UInt32");
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

		[Test]
		public void CastsWork() {
			long i1 = -1, i2 = 0, i3 = 234, i4 = 4294967295, i5 = 4294967296;
			long? ni1 = -1, ni2 = 0, ni3 = 234, ni4 = 4294967295, ni5 = 4294967296, ni6 = null;

			unchecked {
				Assert.AreStrictEqual((uint)i1, 4294967295, "-1 unchecked");
				Assert.AreStrictEqual((uint)i2, 0, "0 unchecked");
				Assert.AreStrictEqual((uint)i3, 234, "234 unchecked");
				Assert.AreStrictEqual((uint)i4, 4294967295, "4294967295 unchecked");
				Assert.AreStrictEqual((uint)i5, 0, "4294967296 unchecked");

				Assert.AreStrictEqual((uint?)ni1, 4294967295, "nullable -1 unchecked");
				Assert.AreStrictEqual((uint?)ni2, 0, "nullable 0 unchecked");
				Assert.AreStrictEqual((uint?)ni3, 234, "nullable 234 unchecked");
				Assert.AreStrictEqual((uint?)ni4, 4294967295, "nullable 4294967295 unchecked");
				Assert.AreStrictEqual((uint?)ni5, 0, "nullable 4294967296 unchecked");
				Assert.AreStrictEqual((uint?)ni6, null, "null unchecked");
			}

			checked {
				Assert.Throws<OverflowException>(() => { var x = (uint)i1; }, "-1 checked");
				Assert.AreStrictEqual((uint)i2, 0, "0 checked");
				Assert.AreStrictEqual((uint)i3, 234, "234 checked");
				Assert.AreStrictEqual((uint)i4, 4294967295, "4294967295 checked");
				Assert.Throws<OverflowException>(() => { var x = (uint)i5; }, "4294967296 checked");

				Assert.Throws<OverflowException>(() => { var x = (uint?)ni1; }, "nullable -1 checked");
				Assert.AreStrictEqual((uint?)ni2, 0, "nullable 0 checked");
				Assert.AreStrictEqual((uint?)ni3, 234, "nullable 234 checked");
				Assert.AreStrictEqual((uint?)ni4, 4294967295, "nullable 4294967295 checked");
				Assert.Throws<OverflowException>(() => { var x = (uint?)ni5; }, "nullable 4294967296 checked");
				Assert.AreStrictEqual((uint?)ni6, null, "null checked");
			}
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
		public void TryParseWorks() {
			uint numberResult;
			bool result = uint.TryParse("23445", out numberResult);
			Assert.IsTrue(result);
			Assert.AreEqual(numberResult, 23445);

			result = uint.TryParse("", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = uint.TryParse(null, out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = uint.TryParse("notanumber", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = uint.TryParse("-1", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);

			result = uint.TryParse("2.5", out numberResult);
			Assert.IsFalse(result);
			Assert.AreEqual(numberResult, 0);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(uint.Parse("23445"), 23445);
			Assert.Throws<FormatException>(() => uint.Parse(""));
			Assert.Throws<ArgumentNullException>(() => uint.Parse(null));
			Assert.Throws<FormatException>(() => uint.Parse("notanumber"));
			Assert.Throws<OverflowException>(() => uint.Parse("4294967296"));
			Assert.Throws<OverflowException>(() => uint.Parse("-1"));
			Assert.Throws<FormatException>(() => uint.Parse("2.5"));
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
