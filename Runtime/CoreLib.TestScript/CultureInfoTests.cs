using System;
using System.Globalization;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class CultureInfoTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var culture = CultureInfo.InvariantCulture;
			Assert.AreEqual(typeof(CultureInfo).FullName, "ss.CultureInfo");
			Assert.IsTrue(typeof(CultureInfo).IsClass);
			Assert.AreEqual(typeof(CultureInfo).GetInterfaces(), new[] { typeof(IFormatProvider) });
			Assert.IsTrue(culture is CultureInfo);
		}

		[Test]
		public void GetFormatWorks() {
			var culture = CultureInfo.InvariantCulture;
			Assert.AreEqual(culture.GetFormat(typeof(int)), null);
			Assert.AreEqual(culture.GetFormat(typeof(NumberFormatInfo)), culture.NumberFormat);
			Assert.AreEqual(culture.GetFormat(typeof(DateTimeFormatInfo)), culture.DateTimeFormat);
		}

		[Test]
		public void InvariantWorks() {
			var culture = CultureInfo.InvariantCulture;
			Assert.AreEqual(culture.Name, "en-US");
			Assert.AreEqual(culture.DateTimeFormat, DateTimeFormatInfo.InvariantInfo);
			Assert.AreEqual(culture.NumberFormat, NumberFormatInfo.InvariantInfo);
		}
	}
}
