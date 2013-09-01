using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class CharTests {
		[Test]
		public void TypePropertiesAreInt32() {
			Assert.AreEqual(typeof(char).FullName, "ss.Int32");
			Assert.IsFalse(typeof(char).IsClass);
			Assert.IsTrue(typeof(IComparable<byte>).IsAssignableFrom(typeof(char)));
			Assert.IsTrue(typeof(IEquatable<byte>).IsAssignableFrom(typeof(char)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(char)));

			var interfaces = typeof(char).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<char>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<char>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueWorks() {
			Assert.AreEqual((int)GetDefaultValue<char>(), 0);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual((int)new char(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual((int)Activator.CreateInstance<char>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual((int)char.MinValue, 0);
			Assert.AreEqual((int)char.MaxValue, 65535);
		}

		[Test]
		public void CharComparisonWorks() {
			char a = 'a', a2 = 'a', b = 'b';
			Assert.IsTrue(a == a2);
			Assert.IsFalse(a == b);
			Assert.IsFalse(a != a2);
			Assert.IsTrue(a != b);
			Assert.IsFalse(a < a2);
			Assert.IsTrue(a < b);
		}

		[Test]
		public void CharParseWorks() {
			Assert.AreEqual(char.Parse("abc"), (int)'a');
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual('\x23'.Format("x4"), "0023");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual('\x23'.ToString("x4"), "0023");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual('\x23'.LocaleFormat("x4"), "0023");
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual('A'.ToString(), "A");
		}

		[Test]
		public void ToLocaleStringWorks() {
			Assert.AreEqual('A'.ToLocaleString(), "A");
		}

		[Test]
		public void CastCharToStringWorks() {
			Assert.AreEqual((string)'c', "c");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   ('0'.GetHashCode(), '0'.GetHashCode());
			Assert.AreEqual   ('1'.GetHashCode(), '1'.GetHashCode());
			Assert.AreNotEqual('0'.GetHashCode(), '1'.GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue ('0'.Equals((int)'0'));
			Assert.IsFalse('1'.Equals((int)'0'));
			Assert.IsFalse('0'.Equals((int)'1'));
			Assert.IsTrue ('1'.Equals((int)'1'));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue ('0'.Equals('0'));
			Assert.IsFalse('1'.Equals('0'));
			Assert.IsFalse('0'.Equals('1'));
			Assert.IsTrue ('1'.Equals('1'));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue('1'.CompareTo('0') > 0);
			Assert.IsTrue('0'.CompareTo('1') < 0);
			Assert.IsTrue('0'.CompareTo('0') == 0);
			Assert.IsTrue('1'.CompareTo('1') == 0);
		}

		[Test]
		public void IsLowerWorks() {
			Assert.IsTrue (char.IsLower('a'), "#1");
			Assert.IsFalse(char.IsLower('A'), "#2");
			Assert.IsFalse(char.IsLower('3'), "#3");
		}

		[Test]
		public void IsUpperWorks() {
			Assert.IsTrue (char.IsUpper('A'), "#1");
			Assert.IsFalse(char.IsUpper('a'), "#2");
			Assert.IsFalse(char.IsUpper('3'), "#3");
		}
	}
}
