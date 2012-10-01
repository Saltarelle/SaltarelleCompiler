using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class BooleanTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)true is bool);
			Assert.AreEqual(typeof(bool).FullName, "Boolean");
			Assert.IsFalse(typeof(bool).IsClass);
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(bool.Parse("true"), true);
			Assert.AreEqual(bool.Parse("false"), false);
		}

		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIsFalse() {
			Assert.AreStrictEqual(GetDefaultValue<bool>(), false);
		}

		[Test]
		public void CreatingInstanceReturnsFalse() {
			Assert.AreStrictEqual(Activator.CreateInstance<bool>(), false);
		}

		[Test]
		public void DefaultConstructorReturnsFalse() {
		    Assert.AreStrictEqual(new bool(), false);
		}
	}
}
