using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class SingleTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(float)0.5 is float);
			Assert.AreEqual(typeof(float).FullName, "Number");
			Assert.IsFalse(typeof(float).IsClass);
		}

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
			Assert.AreEqual(float.MinValue, 5e-324, "MinValue should be correct");
			Assert.IsTrue(float.MaxValue > (float)(object)1.7e+308, "MaxValue should be correct");
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
	}
}
