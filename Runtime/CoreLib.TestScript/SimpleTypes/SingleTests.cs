using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class SingleTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(float)0.5 is float);
			Assert.AreEqual(typeof(float).FullName, "Number");
			Assert.IsFalse(typeof(float).IsClass);
			Assert.IsTrue(typeof(IComparable<float>).IsAssignableFrom(typeof(float)));
			Assert.IsTrue(typeof(IEquatable<float>).IsAssignableFrom(typeof(float)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(float)));
			object f = (float)0;
			Assert.IsTrue(f is float);
			Assert.IsTrue(f is IComparable<float>);
			Assert.IsTrue(f is IEquatable<float>);
			Assert.IsTrue(f is IFormattable);

			var interfaces = typeof(float).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<float>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<float>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<float>(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<float>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			double zero = 0;
			Assert.IsTrue((float)(object)float.MinValue < -3.4e38 && (float)(object)float.MinValue > -3.5e38, "MinValue should be correct");
			Assert.IsTrue((float)(object)float.MaxValue >  3.4e38 && (float)(object)float.MaxValue <  3.5e38, "MaxValue should be correct");
			Assert.AreEqual(float.Epsilon, 1.40129846432482E-45, "Epsilon should be correct");
			Assert.IsTrue(float.IsNaN(float.NaN), "NaN should be correct");
			Assert.AreStrictEqual(float.PositiveInfinity, 1 / zero, "PositiveInfinity should be correct");
			Assert.AreStrictEqual(float.NegativeInfinity, -1 / zero, "NegativeInfinity should be correct");
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new float(), 0);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(((float)291.0).Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(((float)291.0).ToString("x"), "123");
			Assert.AreEqual(((IFormattable)((float)291.0)).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(((float)291.0).LocaleFormat("x"), "123");
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual(((float)123.0).ToString(), "123");
		}

		[Test]
		public void ToExponentialWorks() {
			Assert.AreEqual(((float)123.0).ToExponential(), "1.23e+2");
		}

		[Test]
		public void ToExponentialWithFractionalDigitsWorks() {
			Assert.AreEqual(((float)123.0).ToExponential(1), "1.2e+2");
		}

		[Test]
		public void ToFixed() {
			Assert.AreEqual(((float)123.0).ToFixed(), "123");
		}

		[Test]
		public void ToFixedWithFractionalDigitsWorks() {
			Assert.AreEqual(((float)123.0).ToFixed(1), "123.0");
		}

		[Test]
		public void ToPrecisionWorks() {
			Assert.AreEqual(((float)12345.0).ToPrecision(), "12345");
		}

		[Test]
		public void ToPrecisionWithPrecisionWorks() {
			Assert.AreEqual(((float)12345.0).ToPrecision(2), "1.2e+4");
		}

		[Test]
		public void IsFiniteWorks() {
			float zero = 0, one = 1;
			Assert.IsTrue(float.IsFinite(one));
			Assert.IsFalse(float.IsFinite(one / zero));
			Assert.IsFalse(float.IsFinite(zero / zero));
		}

		[Test]
		public void IsNaNWorks() {
			float zero = 0, one = 1;
			Assert.IsFalse(float.IsNaN(one));
			Assert.IsFalse(float.IsNaN(one / zero));
			Assert.IsTrue(float.IsNaN(zero / zero));
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((float)0).GetHashCode(), ((float)0).GetHashCode());
			Assert.AreEqual   (((float)1).GetHashCode(), ((float)1).GetHashCode());
			Assert.AreNotEqual(((float)0).GetHashCode(), ((float)1).GetHashCode());
			Assert.AreNotEqual(((float)0).GetHashCode(), ((float)0.5).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((float)0).Equals((object)(float)0));
			Assert.IsFalse(((float)1).Equals((object)(float)0));
			Assert.IsFalse(((float)0).Equals((object)(float)0.5));
			Assert.IsTrue (((float)1).Equals((object)(float)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((float)0).Equals((float)0));
			Assert.IsFalse(((float)1).Equals((float)0));
			Assert.IsFalse(((float)0).Equals((float)0.5));
			Assert.IsTrue (((float)1).Equals((float)1));

			Assert.IsTrue (((IEquatable<float>)((float)0)).Equals((float)0));
			Assert.IsFalse(((IEquatable<float>)((float)1)).Equals((float)0));
			Assert.IsFalse(((IEquatable<float>)((float)0)).Equals((float)0.5));
			Assert.IsTrue (((IEquatable<float>)((float)1)).Equals((float)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((float)0).CompareTo((float)0) == 0);
			Assert.IsTrue(((float)1).CompareTo((float)0) > 0);
			Assert.IsTrue(((float)0).CompareTo((float)0.5) < 0);
			Assert.IsTrue(((float)1).CompareTo((float)1) == 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<float>)((float)0)).CompareTo((float)0) == 0);
			Assert.IsTrue(((IComparable<float>)((float)1)).CompareTo((float)0) > 0);
			Assert.IsTrue(((IComparable<float>)((float)0)).CompareTo((float)0.5) < 0);
			Assert.IsTrue(((IComparable<float>)((float)1)).CompareTo((float)1) == 0);
		}
	}
}
