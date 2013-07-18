using System;
using System.Globalization;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class NumberFormatInfoTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var format = NumberFormatInfo.InvariantInfo;
			Assert.AreEqual(typeof(NumberFormatInfo).FullName, "ss.NumberFormatInfo");
			Assert.IsTrue(typeof(NumberFormatInfo).IsClass);
			Assert.IsTrue(format is NumberFormatInfo);
			Assert.AreEqual(typeof(NumberFormatInfo).GetInterfaces(), new[] { typeof(IFormatProvider) });
		}

		[Test]
		public void GetFormatWorks() {
			var format = NumberFormatInfo.InvariantInfo;
			Assert.AreEqual(format.GetFormat(typeof(int)), null);
			Assert.AreEqual(format.GetFormat(typeof(NumberFormatInfo)), format);
		}

		[Test]
		public void InvariantWorks() {
			var format = NumberFormatInfo.InvariantInfo;
			Assert.AreEqual(format.NaNSymbol, "NaN");
			Assert.AreEqual(format.NegativeSign, "-");
			Assert.AreEqual(format.PositiveSign, "+");
			Assert.AreEqual(format.NegativeInfinitySymbol, "-Infinity");
			Assert.AreEqual(format.PositiveInfinitySymbol, "Infinity");

			Assert.AreEqual(format.PercentSymbol, "%");
			Assert.AreEqual(format.PercentGroupSizes, new[] {3});
			Assert.AreEqual(format.PercentDecimalDigits, 2);
			Assert.AreEqual(format.PercentDecimalSeparator, ".");
			Assert.AreEqual(format.PercentGroupSeparator, ",");
			Assert.AreEqual(format.PercentPositivePattern, 0);
			Assert.AreEqual(format.PercentNegativePattern, 0);

			Assert.AreEqual(format.CurrencySymbol, "$");
			Assert.AreEqual(format.CurrencyGroupSizes, new[] {3});
			Assert.AreEqual(format.CurrencyDecimalDigits, 2);
			Assert.AreEqual(format.CurrencyDecimalSeparator, ".");
			Assert.AreEqual(format.CurrencyGroupSeparator, ",");
			Assert.AreEqual(format.CurrencyNegativePattern, 0);
			Assert.AreEqual(format.CurrencyPositivePattern, 0);

			Assert.AreEqual(format.NumberGroupSizes, new[] {3});
			Assert.AreEqual(format.NumberDecimalDigits, 2);
			Assert.AreEqual(format.NumberDecimalSeparator, ".");
			Assert.AreEqual(format.NumberGroupSeparator, ",");
		}
	}
}
