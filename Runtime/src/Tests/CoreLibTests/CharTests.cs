using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class CharTests {
		[Test]
		public void TypePropertiesAreInt32() {
			Assert.AreEqual(typeof(char).FullName, "ss.Int32");
			Assert.IsFalse(typeof(char).IsClass);
		}

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
		public void CharFormatWorks() {
			Assert.AreEqual('\x23'.Format("x4"), "0023");
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
	}
}
