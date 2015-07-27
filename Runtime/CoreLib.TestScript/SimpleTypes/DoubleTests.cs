using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class DoubleTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(double)0.5 is double);
			Assert.AreEqual(typeof(double).FullName, "Number");
			Assert.IsFalse(typeof(double).IsClass);
			Assert.IsTrue(typeof(IComparable<double>).IsAssignableFrom(typeof(double)));
			Assert.IsTrue(typeof(IEquatable<double>).IsAssignableFrom(typeof(double)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(double)));
			object d = (double)0;
			Assert.IsTrue((object)d is double);
			Assert.IsTrue((object)d is IComparable<double>);
			Assert.IsTrue((object)d is IEquatable<double>);
			Assert.IsTrue((object)d is IFormattable);

			var interfaces = typeof(double).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<double>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<double>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
		}

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<double>(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<double>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			double zero = 0;
			Assert.IsTrue(double.MinValue < (double)(object)-1.7e+308, "MinValue should be correct");
			Assert.IsTrue(double.MaxValue > (double)(object)1.7e+308, "MaxValue should be correct");
			Assert.AreEqual(double.JsMinValue, 5e-324, "MinValue should be correct");
			Assert.AreEqual(double.Epsilon, 4.94065645841247E-324, "MinValue should be correct");
			Assert.IsTrue(double.IsNaN(double.NaN), "NaN should be correct");
			Assert.AreStrictEqual(double.PositiveInfinity, 1 / zero, "PositiveInfinity should be correct");
			Assert.AreStrictEqual(double.NegativeInfinity, -1 / zero, "NegativeInfinity should be correct");
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new double(), 0);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual((291.0).Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(291.0.ToString("x"), "123");
			Assert.AreEqual(((IFormattable)291.0).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual((291.0).LocaleFormat("x"), "123");
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual((123.0).ToString(), "123");
		}

		[Test]
		public void ToExponentialWorks() {
			Assert.AreEqual((123.0).ToExponential(), "1.23e+2");
		}

		[Test]
		public void ToExponentialWithFractionalDigitsWorks() {
			Assert.AreEqual((123.0).ToExponential(1), "1.2e+2");
		}

		[Test]
		public void ToFixed() {
			Assert.AreEqual((123.0).ToFixed(), "123");
		}

		[Test]
		public void ToFixedWithFractionalDigitsWorks() {
			Assert.AreEqual((123.0).ToFixed(1), "123.0");
		}

		[Test]
		public void ToPrecisionWorks() {
			Assert.AreEqual((12345.0).ToPrecision(), "12345");
		}

		[Test]
		public void ToPrecisionWithPrecisionWorks() {
			Assert.AreEqual((12345.0).ToPrecision(2), "1.2e+4");
		}

		[Test]
		public void IsPositiveInfinityWorks() {
			double inf = 1.0 / 0.0;
			Assert.IsTrue (double.IsPositiveInfinity(inf));
			Assert.IsFalse(double.IsPositiveInfinity(-inf));
			Assert.IsFalse(double.IsPositiveInfinity(0.0));
			Assert.IsFalse(double.IsPositiveInfinity(Double.NaN));
		}

		[Test]
		public void IsNegativeInfinityWorks() {
			double inf = 1.0 / 0.0;
			Assert.IsFalse(double.IsNegativeInfinity(inf));
			Assert.IsTrue (double.IsNegativeInfinity(-inf));
			Assert.IsFalse(double.IsNegativeInfinity(0.0));
			Assert.IsFalse(double.IsNegativeInfinity(Double.NaN));
		}

		[Test]
		public void IsInfinityWorks() {
			double inf = 1.0 / 0.0;
			Assert.IsTrue (double.IsInfinity(inf));
			Assert.IsTrue (double.IsInfinity(-inf));
			Assert.IsFalse(double.IsInfinity(0.0));
			Assert.IsFalse(double.IsInfinity(Double.NaN));
		}

		[Test]
		public void IsFiniteWorks() {
			double zero = 0, one = 1;
			Assert.IsTrue(double.IsFinite(one));
			Assert.IsFalse(double.IsFinite(one / zero));
			Assert.IsFalse(double.IsFinite(zero / zero));
		}

		[Test]
		public void IsNaNWorks() {
			double zero = 0, one = 1;
			Assert.IsFalse(double.IsNaN(one));
			Assert.IsFalse(double.IsNaN(one / zero));
			Assert.IsTrue(double.IsNaN(zero / zero));
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((double)0).GetHashCode(), ((double)0).GetHashCode());
			Assert.AreEqual   (((double)1).GetHashCode(), ((double)1).GetHashCode());
			Assert.AreNotEqual(((double)0).GetHashCode(), ((double)1).GetHashCode());
			Assert.AreNotEqual(((double)0).GetHashCode(), ((double)0.5).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((double)0).Equals((object)(double)0));
			Assert.IsFalse(((double)1).Equals((object)(double)0));
			Assert.IsFalse(((double)0).Equals((object)(double)0.5));
			Assert.IsTrue (((double)1).Equals((object)(double)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((double)0).Equals((double)0));
			Assert.IsFalse(((double)1).Equals((double)0));
			Assert.IsFalse(((double)0).Equals((double)0.5));
			Assert.IsTrue (((double)1).Equals((double)1));

			Assert.IsTrue (((IEquatable<double>)((double)0)).Equals((double)0));
			Assert.IsFalse(((IEquatable<double>)((double)1)).Equals((double)0));
			Assert.IsFalse(((IEquatable<double>)((double)0)).Equals((double)0.5));
			Assert.IsTrue (((IEquatable<double>)((double)1)).Equals((double)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((double)0).CompareTo((double)0) == 0);
			Assert.IsTrue(((double)1).CompareTo((double)0) > 0);
			Assert.IsTrue(((double)0).CompareTo((double)0.5) < 0);
			Assert.IsTrue(((double)1).CompareTo((double)1) == 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<double>)((double)0)).CompareTo((double)0) == 0);
			Assert.IsTrue(((IComparable<double>)((double)1)).CompareTo((double)0) > 0);
			Assert.IsTrue(((IComparable<double>)((double)0)).CompareTo((double)0.5) < 0);
			Assert.IsTrue(((IComparable<double>)((double)1)).CompareTo((double)1) == 0);
		}
	}
}
