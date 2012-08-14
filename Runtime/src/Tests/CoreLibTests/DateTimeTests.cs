using System;
using System.Testing;
using System.Globalization;

namespace CoreLibTests {
	[TestFixture]
	public class DateTimeTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(DateTime).FullName, "Date");
			Assert.IsFalse(typeof(DateTime).IsClass);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var dt = new DateTime();
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void DefaultValueWorks() {
			var dt = default(DateTime);
			Assert.AreEqual(dt.GetUtcFullYear(), 1970);
		}

		[Test]
		public void MillisecondSinceEpochConstructorWorks() {
			var dt = new DateTime(1440L * 60 * 500 * 1000);
			Assert.AreEqual(dt.GetFullYear(), 1971);
		}

		[Test]
		public void StringConstructorWorks() {
			var dt = new DateTime("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDConstructorWorks() {
			var dt = new DateTime(2011, 7, 12);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDHConstructorWorks() {
			var dt = new DateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void YMDHNConstructorWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void YMDHNSConstructorWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void YMDHNSUConstructorWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
			Assert.AreEqual(dt.GetSeconds(), 56);
			Assert.AreEqual(dt.GetMilliseconds(), 345);
		}

		[Test]
		public void NowWorks() {
			var dt = DateTime.Now;
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void TodayWorks() {
			var dt = DateTime.Today;
			Assert.IsTrue(dt.GetFullYear() > 2011);
			Assert.AreEqual(dt.GetHours(), 0);
			Assert.AreEqual(dt.GetMinutes(), 0);
			Assert.AreEqual(dt.GetSeconds(), 0);
			Assert.AreEqual(dt.GetMilliseconds(), 0);
		}

		[Test]
		public void FormatWorks() {
			var dt = new DateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void LocaleFormatWorks() {
			var dt = new DateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void GetFullYearWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetFullYear(), 2011);
		}

		[Test]
		public void GetMonthWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMonth(), 7);
		}

		[Test]
		public void GetDateWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void GetHoursWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void GetMinutesWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void GetSecondsWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void GetMillisecondsWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMilliseconds(), 345);
		}

		[Test]
		public void GetDayWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDay(), 5);
		}

		[Test]
		public void GetTimeWorks() {
			var dt = new DateTime(DateTime.Utc(1970, 0, 2));
			Assert.AreEqual(dt.GetTime(), 1440 * 60 * 1000);
		}

		[Test]
		public void ValueOfWorks() {
			var dt = new DateTime(DateTime.Utc(1970, 0, 2));
			Assert.AreEqual(dt.ValueOf(), 1440 * 60 * 1000);
		}

		[Test]
		public void GetTimezoneOffsetWorks() {
			var dt = new DateTime(0);
			Assert.AreEqual(dt.GetTimezoneOffset(), new DateTime(1970, 0, 1).ValueOf() / 60000);
		}

		[Test]
		public void GetUtcFullYearWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcFullYear(), 2011);
		}

		[Test]
		public void GetUtcMonthWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMonth(), 7);
		}

		[Test]
		public void GetUtcDateWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void GetUtcHoursWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcHours(), 13);
		}

		[Test]
		public void GetUtcMinutesWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMinutes(), 42);
		}

		[Test]
		public void GetUtcSecondsWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcSeconds(), 56);
		}

		[Test]
		public void GetUtcMillisecondsWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMilliseconds(), 345);
		}

		[Test]
		public void GetUtcDayWorks() {
			var dt = new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDay(), 5);
		}

		[Test]
		public void ParseWorks() {
			var dt = DateTime.Parse("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWorks() {
			var dt = DateTime.ParseExact("2012-12-08", "yyyy-dd-MM");
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetMonth(), 7);
			Assert.AreEqual(dt.Value.GetDate(), 12);
		}

		[Test]
		public void ParseExactReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExact("X", "yyyy-dd-MM");
			Assert.IsFalse(dt.HasValue);
		}

		[Test]
		public void ParseExactWithCultureWorks() {
			var dt = DateTime.ParseExact("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetMonth(), 7);
			Assert.AreEqual(dt.Value.GetDate(), 12);
		}

		[Test]
		public void ParseExactWithCultureReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExact("X", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.IsFalse(dt.HasValue);
		}

		[Test]
		public void ParseExactUtcWorks() {
			var dt = DateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM");
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetUtcMonth(), 7);
			Assert.AreEqual(dt.Value.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM");
			Assert.IsFalse(dt.HasValue);
		}

		[Test]
		public void ParseExactUtcWithCultureWorks() {
			var dt = DateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetUtcMonth(), 7);
			Assert.AreEqual(dt.Value.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcWithCultureReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.IsFalse(dt.HasValue);
		}

		[Test]
		public void ToDateStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			var s = dt.ToDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToTimeStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			var s = dt.ToTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToUtcStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			var s = dt.ToUtcString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToLocaleDateStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToLocaleTimeStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		private void AssertDateUtc(DateTime dt, int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
			Assert.AreEqual(dt.GetUtcFullYear(), year);
			Assert.AreEqual(dt.GetUtcMonth(), month);
			Assert.AreEqual(dt.GetUtcDate(), day);
			Assert.AreEqual(dt.GetUtcHours(), hours);
			Assert.AreEqual(dt.GetUtcMinutes(), minutes);
			Assert.AreEqual(dt.GetUtcSeconds(), seconds);
			Assert.AreEqual(dt.GetUtcMilliseconds(), milliseconds);
		}

		[Test]
		public void FromUtcYMDWorks() {
			AssertDateUtc(DateTime.FromUtc(2011, 7, 12), 2011, 7, 12, 0, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHWorks() {
			AssertDateUtc(DateTime.FromUtc(2011, 7, 12, 13), 2011, 7, 12, 13, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNWorks() {
			AssertDateUtc(DateTime.FromUtc(2011, 7, 12, 13, 42), 2011, 7, 12, 13, 42, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNSWorks() {
			AssertDateUtc(DateTime.FromUtc(2011, 7, 12, 13, 42, 56), 2011, 7, 12, 13, 42, 56, 0);
		}

		[Test]
		public void FromUtcYMDHNSUWorks() {
			AssertDateUtc(DateTime.FromUtc(2011, 7, 12, 13, 42, 56, 345), 2011, 7, 12, 13, 42, 56, 345);
		}

		[Test]
		public void UtcYMDWorks() {
			AssertDateUtc(new DateTime(DateTime.Utc(2011, 7, 12)), 2011, 7, 12, 0, 0, 0, 0);
		}

		[Test]
		public void UtcYMDHWorks() {
			AssertDateUtc(new DateTime(DateTime.Utc(2011, 7, 12, 13)), 2011, 7, 12, 13, 0, 0, 0);
		}

		[Test]
		public void UtcYMDHNWorks() {
			AssertDateUtc(new DateTime(DateTime.Utc(2011, 7, 12, 13, 42)), 2011, 7, 12, 13, 42, 0, 0);
		}

		[Test]
		public void UtcYMDHNSWorks() {
			AssertDateUtc(new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56)), 2011, 7, 12, 13, 42, 56, 0);
		}

		[Test]
		public void UtcYMDHNSUWorks() {
			AssertDateUtc(new DateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345)), 2011, 7, 12, 13, 42, 56, 345);
		}

		[Test]
		public void SubtractingDatesWorks() {
			Assert.AreEqual(new DateTime(2011, 7, 12) - new DateTime(2011, 7, 11), 1440 * 60 * 1000);
		}

		[Test]
		public void DateEqualityWorks() {
			Assert.IsTrue(DateTime.AreEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 12)));
			Assert.IsFalse(DateTime.AreEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 13)));
			Assert.AreStrictEqual(DateTime.AreEqual(new DateTime(2011, 7, 12), (DateTime?)null), false);
			Assert.AreStrictEqual(DateTime.AreEqual((DateTime?)null, new DateTime(2011, 7, 12)), false);
			Assert.AreStrictEqual(DateTime.AreEqual((DateTime?)null, (DateTime?)null), true);
		}

		[Test]
		public void DateLessThanWorks() {
			Assert.IsTrue(new DateTime(2011, 7, 11) < new DateTime(2011, 7, 12));
			Assert.IsFalse(new DateTime(2011, 7, 12) < new DateTime(2011, 7, 12));
			Assert.IsFalse(new DateTime(2011, 7, 13) < new DateTime(2011, 7, 12));
		}

		[Test]
		public void DateLessEqualWorks() {
			Assert.IsTrue(new DateTime(2011, 7, 11) <= new DateTime(2011, 7, 12));
			Assert.IsTrue(new DateTime(2011, 7, 12) <= new DateTime(2011, 7, 12));
			Assert.IsFalse(new DateTime(2011, 7, 13) <= new DateTime(2011, 7, 12));
		}

		[Test]
		public void DateGreaterThanWorks() {
			Assert.IsFalse(new DateTime(2011, 7, 11) > new DateTime(2011, 7, 12));
			Assert.IsFalse(new DateTime(2011, 7, 12) > new DateTime(2011, 7, 12));
			Assert.IsTrue(new DateTime(2011, 7, 13) > new DateTime(2011, 7, 12));
		}

		[Test]
		public void DateGreaterEqualWorks() {
			Assert.IsFalse(new DateTime(2011, 7, 11) >= new DateTime(2011, 7, 12));
			Assert.IsTrue(new DateTime(2011, 7, 12) >= new DateTime(2011, 7, 12));
			Assert.IsTrue(new DateTime(2011, 7, 13) >= new DateTime(2011, 7, 12));
		}

		[Test]
		public void ConvertingDateToMutableDateReturnsANewButEqualInstance() {
			var dt = new DateTime(2011, 7, 12);
			MutableDateTime mdt = dt;
			Assert.IsFalse((object)dt == (object)mdt);
			Assert.AreEqual(mdt.GetFullYear(), 2011);
			Assert.AreEqual(mdt.GetMonth(), 7);
			Assert.AreEqual(mdt.GetDate(), 12);
		}

		[Test]
		public void ConvertingMutableDateToDateReturnsANewButEqualInstance() {
			var mdt = new MutableDateTime(2011, 7, 12);
			DateTime dt = mdt;
			Assert.IsFalse((object)dt == (object)mdt);
			Assert.AreEqual(mdt.GetFullYear(), 2011);
			Assert.AreEqual(mdt.GetMonth(), 7);
			Assert.AreEqual(mdt.GetDate(), 12);
		}
	}
}
