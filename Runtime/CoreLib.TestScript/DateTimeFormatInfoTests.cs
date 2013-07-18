using System;
using System.Globalization;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class DateTimeFormatInfoTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var format = DateTimeFormatInfo.InvariantInfo;
			Assert.AreEqual(typeof (DateTimeFormatInfo).FullName, "ss.DateTimeFormatInfo");
			Assert.IsTrue(typeof (DateTimeFormatInfo).IsClass);
			Assert.AreEqual(typeof(DateTimeFormatInfo).GetInterfaces(), new[] { typeof(IFormatProvider) });
			Assert.IsTrue(format is DateTimeFormatInfo);
		}

		[Test]
		public void GetFormatWorks() {
			var format = DateTimeFormatInfo.InvariantInfo;
			Assert.AreEqual(format.GetFormat(typeof(int)), null);
			Assert.AreEqual(format.GetFormat(typeof(DateTimeFormatInfo)), format);
		}

		[Test]
		public void InvariantWorks() {
			var format = DateTimeFormatInfo.InvariantInfo;
			Assert.AreEqual(format.AMDesignator, "AM");
			Assert.AreEqual(format.PMDesignator, "PM");

			Assert.AreEqual(format.DateSeparator, "/");
			Assert.AreEqual(format.TimeSeparator, ":");

			Assert.AreEqual(format.GMTDateTimePattern, "ddd, dd MMM yyyy HH:mm:ss 'GMT'");
			Assert.AreEqual(format.UniversalDateTimePattern, "yyyy-MM-dd HH:mm:ssZ");
			Assert.AreEqual(format.SortableDateTimePattern, "yyyy-MM-ddTHH:mm:ss");
			Assert.AreEqual(format.DateTimePattern, "dddd, MMMM dd, yyyy h:mm:ss tt");

			Assert.AreEqual(format.LongDatePattern, "dddd, MMMM dd, yyyy");
			Assert.AreEqual(format.ShortDatePattern, "M/d/yyyy");

			Assert.AreEqual(format.LongTimePattern, "h:mm:ss tt");
			Assert.AreEqual(format.ShortTimePattern, "h:mm tt");

			Assert.AreEqual(format.FirstDayOfWeek, 0);
			Assert.AreEqual(format.DayNames, new[] {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"});
			Assert.AreEqual(format.ShortDayNames, new[] {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"});
			Assert.AreEqual(format.MinimizedDayNames, new[] {"Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"});

			Assert.AreEqual(format.MonthNames, new[] {
			  "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
			  "November", "December", ""
			});
			Assert.AreEqual(format.ShortMonthNames,
				new[] {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""});
		}
	}
}
