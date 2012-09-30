using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class DoubleTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(double)0.5 is double);
			Assert.AreEqual(typeof(double).FullName, "Number");
			Assert.IsFalse(typeof(double).IsClass);
		}

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
			Assert.AreEqual(double.MinValue, 5e-324, "MinValue should be correct");
			Assert.IsTrue(double.MaxValue > (double)(object)1.7e+308, "MaxValue should be correct");
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
	}
}
