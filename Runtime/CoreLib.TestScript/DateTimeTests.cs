using System;
using QUnit;
using System.Globalization;

namespace CoreLib.TestScript {
	[TestFixture]
	public class DateTimeTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(DateTime).FullName, "Date");
			Assert.IsFalse(typeof(DateTime).IsClass);
			Assert.IsTrue(typeof(IComparable<DateTime>).IsAssignableFrom(typeof(DateTime)));
			Assert.IsTrue(typeof(IEquatable<DateTime>).IsAssignableFrom(typeof(DateTime)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(DateTime)));
			object d = new DateTime();
			Assert.IsTrue(d is DateTime);
			Assert.IsTrue(d is IComparable<DateTime>);
			Assert.IsTrue(d is IEquatable<DateTime>);
			Assert.IsTrue(d is IFormattable);

			var interfaces = typeof(DateTime).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 3);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<DateTime>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<DateTime>)));
			Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
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
		public void CreatingInstanceReturnsDateWithZeroValue() {
			var dt = Activator.CreateInstance<DateTime>();
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
			Assert.AreEqual(dt.GetMonth(), 8);
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
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-07-12");
		}

		[Test]
		public void IFormattableToStringWorks() {
			var dt = new DateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.ToString("yyyy-MM-dd"), "2011-07-12");
			Assert.AreEqual(((IFormattable)dt).ToString("yyyy-MM-dd"), "2011-07-12");
		}

		[Test]
		public void LocaleFormatWorks() {
			var dt = new DateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.LocaleFormat("yyyy-MM-dd"), "2011-07-12");
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
			var dt = new DateTime(2011, 8, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDay(), 5);
		}

		[Test]
		public void GetTimeWorks() {
			var dt = new DateTime(DateTime.Utc(1970, 1, 2));
			Assert.AreEqual(dt.GetTime(), 1440 * 60 * 1000);
		}

		[Test]
		public void ValueOfWorks() {
			var dt = new DateTime(DateTime.Utc(1970, 1, 2));
			Assert.AreEqual(dt.ValueOf(), 1440 * 60 * 1000);
		}

		[Test]
		public void GetTimezoneOffsetWorks() {
			var dt = new DateTime(0);
			Assert.AreEqual(dt.GetTimezoneOffset(), new DateTime(1970, 1, 1).ValueOf() / 60000);
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
			var dt = new DateTime(DateTime.Utc(2011, 8, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDay(), 5);
		}

		[Test]
		public void ParseWorks() {
			var dt = DateTime.Parse("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 8);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWorks() {
			var dt = DateTime.ParseExact("2012-12-08", "yyyy-dd-MM");
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetMonth(), 8);
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
			Assert.AreEqual(dt.Value.GetMonth(), 8);
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
			Assert.AreEqual(dt.Value.GetUtcMonth(), 8);
			Assert.AreEqual(dt.Value.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExactUtc("X", "yyyy-dd-MM");
			Assert.IsFalse(dt.HasValue);
		}

		[Test]
		public void ParseExactUtcWithCultureWorks() {
			var dt = DateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.IsTrue(dt.HasValue);
			Assert.AreEqual(dt.Value.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.Value.GetUtcMonth(), 8);
			Assert.AreEqual(dt.Value.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcWithCultureReturnsNullIfTheInputIsInvalid() {
			var dt = DateTime.ParseExactUtc("X", "yyyy-dd-MM", CultureInfo.InvariantCulture);
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

			Assert.AreEqual(new DateTime(2011, 7, 12).Subtract(new DateTime(2011, 7, 11)), new TimeSpan(1, 0, 0, 0));
			Assert.AreEqual(new DateTime(2011, 7, 12, 15, 0, 0).Subtract(new DateTime(2011, 7, 11, 13, 0, 0)),
				new TimeSpan(1, 2, 0, 0));
		}

		[Test]
		public void AreEqualWorks() {
			Assert.IsTrue(DateTime.AreEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 12)));
			Assert.IsFalse(DateTime.AreEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 13)));
			Assert.AreStrictEqual(DateTime.AreEqual(new DateTime(2011, 7, 12), (DateTime?)null), false);
			Assert.AreStrictEqual(DateTime.AreEqual((DateTime?)null, new DateTime(2011, 7, 12)), false);
			Assert.AreStrictEqual(DateTime.AreEqual((DateTime?)null, (DateTime?)null), true);
		}

		[Test]
		public void AreNotEqualWorks() {
			Assert.IsFalse(DateTime.AreNotEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 12)));
			Assert.IsTrue(DateTime.AreNotEqual(new DateTime(2011, 7, 12), new DateTime(2011, 7, 13)));
			Assert.AreStrictEqual(DateTime.AreNotEqual(new DateTime(2011, 7, 12), (DateTime?)null), true);
			Assert.AreStrictEqual(DateTime.AreNotEqual((DateTime?)null, new DateTime(2011, 7, 12)), true);
			Assert.AreStrictEqual(DateTime.AreNotEqual((DateTime?)null, (DateTime?)null), false);
		}

		[Test]
		public void DateEqualityWorks() {
			Assert.IsTrue(new DateTime(2011, 7, 12) == new DateTime(2011, 7, 12));
			Assert.IsFalse(new DateTime(2011, 7, 12) == new DateTime(2011, 7, 13));
			Assert.AreStrictEqual(new DateTime(2011, 7, 12) == (DateTime?)null, false);
			Assert.AreStrictEqual((DateTime?)null == new DateTime(2011, 7, 12), false);
			Assert.AreStrictEqual((DateTime?)null == (DateTime?)null, true);
		}

		[Test]
		public void DateInequalityWorks() {
			Assert.IsFalse(new DateTime(2011, 7, 12) != new DateTime(2011, 7, 12));
			Assert.IsTrue(new DateTime(2011, 7, 12) != new DateTime(2011, 7, 13));
			Assert.AreStrictEqual(new DateTime(2011, 7, 12) != (DateTime?)null, true);
			Assert.AreStrictEqual((DateTime?)null != new DateTime(2011, 7, 12), true);
			Assert.AreStrictEqual((DateTime?)null != (DateTime?)null, false);
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
			JsDate mdt = (JsDate)dt;
			Assert.IsFalse((object)dt == (object)mdt);
			Assert.AreEqual(mdt.GetFullYear(), 2011);
			Assert.AreEqual(mdt.GetMonth(), 6);
			Assert.AreEqual(mdt.GetDate(), 12);
		}

		[Test]
		public void ConvertingMutableDateToDateReturnsANewButEqualInstance() {
			var mdt = new JsDate(2011, 7, 12);
			DateTime dt = (DateTime)mdt;
			Assert.IsFalse((object)dt == (object)mdt);
			Assert.AreEqual(mdt.GetFullYear(), 2011);
			Assert.AreEqual(mdt.GetMonth(), 7);
			Assert.AreEqual(mdt.GetDate(), 12);
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (new DateTime(0).GetHashCode(), new DateTime(0).GetHashCode());
			Assert.AreEqual   (new DateTime(1).GetHashCode(), new DateTime(1).GetHashCode());
			Assert.AreNotEqual(new DateTime(0).GetHashCode(), new DateTime(1).GetHashCode());
			Assert.IsTrue((long)new DateTime(3000, 1, 1).GetHashCode() < 0xffffffffL);
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue( new DateTime(0).Equals((object)new DateTime(0)));
			Assert.IsFalse(new DateTime(1).Equals((object)new DateTime(0)));
			Assert.IsFalse(new DateTime(0).Equals((object)new DateTime(1)));
			Assert.IsTrue( new DateTime(1).Equals((object)new DateTime(1)));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue( new DateTime(0).Equals(new DateTime(0)));
			Assert.IsFalse(new DateTime(1).Equals(new DateTime(0)));
			Assert.IsFalse(new DateTime(0).Equals(new DateTime(1)));
			Assert.IsTrue( new DateTime(1).Equals(new DateTime(1)));

			Assert.IsTrue( ((IEquatable<DateTime>)new DateTime(0)).Equals(new DateTime(0)));
			Assert.IsFalse(((IEquatable<DateTime>)new DateTime(1)).Equals(new DateTime(0)));
			Assert.IsFalse(((IEquatable<DateTime>)new DateTime(0)).Equals(new DateTime(1)));
			Assert.IsTrue( ((IEquatable<DateTime>)new DateTime(1)).Equals(new DateTime(1)));
		}

		[Test]
		public void StaticEqualsWorks() {
			Assert.IsTrue( DateTime.Equals(new DateTime(0), new DateTime(0)));
			Assert.IsFalse(DateTime.Equals(new DateTime(1), new DateTime(0)));
			Assert.IsFalse(DateTime.Equals(new DateTime(0), new DateTime(1)));
			Assert.IsTrue( DateTime.Equals(new DateTime(1), new DateTime(1)));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(new DateTime(0).CompareTo(new DateTime(0)) == 0);
			Assert.IsTrue(new DateTime(1).CompareTo(new DateTime(0)) > 0);
			Assert.IsTrue(new DateTime(0).CompareTo(new DateTime(1)) < 0);
		}

		[Test]
		public void StaticCompareWorks() {
			Assert.IsTrue(DateTime.Compare(new DateTime(0), new DateTime(0)) == 0);
			Assert.IsTrue(DateTime.Compare(new DateTime(1), new DateTime(0)) > 0);
			Assert.IsTrue(DateTime.Compare(new DateTime(0), new DateTime(1)) < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<DateTime>)new DateTime(0)).CompareTo(new DateTime(0)) == 0);
			Assert.IsTrue(((IComparable<DateTime>)new DateTime(1)).CompareTo(new DateTime(0)) > 0);
			Assert.IsTrue(((IComparable<DateTime>)new DateTime(0)).CompareTo(new DateTime(1)) < 0);
		}

		[Test]
		public void DatePropertyWorks() {
			var dt = new DateTime(2012, 8, 12, 13, 14, 15, 16);
			Assert.AreEqual(dt.Date, new DateTime(2012, 8, 12));
		}

		[Test]
		public void DayPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Day, 12);
		}

		[Test]
		public void DayOfWeekPropertyWorks() {
			var dt = new DateTime(2011, 8, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.DayOfWeek, DayOfWeek.Friday);
		}

		[Test]
		public void DayOfYearPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.DayOfYear, 193);
		}

		[Test]
		public void HourPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Hour, 13);
		}

		[Test]
		public void MillisecondPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Millisecond, 345);
		}

		[Test]
		public void MinutePropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Minute, 42);
		}

		[Test]
		public void MonthPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Month, 7);
		}

		[Test]
		public void SecondPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Second, 56);
		}

		[Test]
		public void YearPropertyWorks() {
			var dt = new DateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.Year, 2011);
		}

		[Test]
		public void AddDaysWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddDays(2.5);
			Assert.AreEqual(actual, new DateTime(2011, 7, 14, 14, 42, 56, 345));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddHoursWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddHours(2.5);
			Assert.AreEqual(actual, new DateTime(2011, 7, 12, 5, 12, 56, 345));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddMillisecondsWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddMilliseconds(250.4);
			Assert.AreEqual(actual, new DateTime(2011, 7, 12, 2, 42, 56, 595));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddMinutesWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddMinutes(2.5);
			Assert.AreEqual(actual, new DateTime(2011, 7, 12, 2, 45, 26, 345));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddMonthsWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddMonths(6);
			Assert.AreEqual(actual, new DateTime(2012, 1, 12, 2, 42, 56, 345));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddSecondsWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddSeconds(2.5);
			Assert.AreEqual(actual, new DateTime(2011, 7, 12, 2, 42, 58, 845));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void AddYearsWorks() {
			var dt = new DateTime(2011, 7, 12, 2, 42, 56, 345);
			var actual = dt.AddYears(3);
			Assert.AreEqual(actual, new DateTime(2014, 7, 12, 2, 42, 56, 345));
			Assert.AreEqual(dt, new DateTime(2011, 7, 12, 2, 42, 56, 345));
		}

		[Test]
		public void DaysInMonthWorks() {
			Assert.AreEqual(DateTime.DaysInMonth(2013, 1), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 2), 28);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 3), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 4), 30);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 5), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 6), 30);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 7), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 8), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 9), 30);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 10), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 11), 30);
			Assert.AreEqual(DateTime.DaysInMonth(2013, 12), 31);
			Assert.AreEqual(DateTime.DaysInMonth(2003, 2), 28);
			Assert.AreEqual(DateTime.DaysInMonth(2004, 2), 29);
		}

		[Test]
		public void IsLeapYearWorks() {
			Assert.IsTrue (DateTime.IsLeapYear(2004));
			Assert.IsTrue (DateTime.IsLeapYear(2000));
			Assert.IsFalse(DateTime.IsLeapYear(2003));
		}
	}
}
