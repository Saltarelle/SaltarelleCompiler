using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class BooleanTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)true is bool);
			Assert.AreEqual(typeof(bool).FullName, "Boolean");
			Assert.AreEqual(typeof(bool).BaseType, typeof(object));
			Assert.IsFalse(typeof(bool).IsClass);
			Assert.IsTrue(typeof(IComparable<bool>).IsAssignableFrom(typeof(bool)));
			Assert.IsTrue(typeof(IEquatable<bool>).IsAssignableFrom(typeof(bool)));
			object b = false;
			Assert.IsTrue(b is IComparable<bool>);
			Assert.IsTrue(b is IEquatable<bool>);

			var interfaces = typeof(bool).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<bool>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<bool>)));
		}

		[IncludeGenericArguments]
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

		[Test]
		public void ParseWorks() {
			Assert.AreStrictEqual(bool.Parse("true"), true, "true");
			Assert.AreStrictEqual(bool.Parse("TRue"), true, "TRue");
			Assert.AreStrictEqual(bool.Parse("TRUE"), true, "TRUE");
			Assert.AreStrictEqual(bool.Parse("  true\t"), true, "true with spaces");

			Assert.AreStrictEqual(bool.Parse("false"), false, "false");
			Assert.AreStrictEqual(bool.Parse("FAlse"), false, "FAlse");
			Assert.AreStrictEqual(bool.Parse("FALSE"), false, "FALSE");
			Assert.AreStrictEqual(bool.Parse("  false\t"), false, "false with spaces");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual(true.GetHashCode(), true.GetHashCode());
			Assert.AreEqual(false.GetHashCode(), false.GetHashCode());
			Assert.AreNotEqual(false.GetHashCode(), true.GetHashCode());
		}

		[Test]
		public void ObjectEqualsWorks() {
			Assert.IsTrue(true.Equals((object)true));
			Assert.IsFalse(true.Equals((object)false));
			Assert.IsFalse(false.Equals((object)true));
			Assert.IsTrue(false.Equals((object)false));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue(true.Equals(true));
			Assert.IsFalse(true.Equals(false));
			Assert.IsFalse(false.Equals(true));
			Assert.IsTrue(false.Equals(false));

			Assert.IsTrue(((IEquatable<bool>)true).Equals(true));
			Assert.IsFalse(((IEquatable<bool>)true).Equals(false));
			Assert.IsFalse(((IEquatable<bool>)false).Equals(true));
			Assert.IsTrue(((IEquatable<bool>)false).Equals(false));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(true.CompareTo(true) == 0);
			Assert.IsTrue(true.CompareTo(false) > 0);
			Assert.IsTrue(false.CompareTo(true) < 0);
			Assert.IsTrue(false.CompareTo(false) == 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<bool>)true).CompareTo(true) == 0);
			Assert.IsTrue(((IComparable<bool>)true).CompareTo(false) > 0);
			Assert.IsTrue(((IComparable<bool>)false).CompareTo(true) < 0);
			Assert.IsTrue(((IComparable<bool>)false).CompareTo(false) == 0);
		}
	}
}
