using System;
using System.Testing;
using System.Globalization;

namespace CoreLibTests {
	[TestFixture]
	public class MutableDateTimeTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(MutableDateTime).FullName, "ss.MutableDateTime");
			Assert.IsTrue(typeof(MutableDateTime).IsClass);
			object o = new MutableDateTime();
			Assert.IsTrue(o is MutableDateTime);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var dt = new MutableDateTime();
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void MillisecondSinceEpochConstructorWorks() {
			var dt = new MutableDateTime(1440L * 60 * 500 * 1000);
			Assert.AreEqual(dt.GetFullYear(), 1971);
		}

		[Test]
		public void StringConstructorWorks() {
			var dt = new MutableDateTime("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDConstructorWorks() {
			var dt = new MutableDateTime(2011, 7, 12);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDHConstructorWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void YMDHNConstructorWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void YMDHNSConstructorWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void YMDHNSUConstructorWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
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
			var dt = MutableDateTime.Now;
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void TodayWorks() {
			var dt = MutableDateTime.Today;
			Assert.IsTrue(dt.GetFullYear() > 2011);
			Assert.AreEqual(dt.GetHours(), 0);
			Assert.AreEqual(dt.GetMinutes(), 0);
			Assert.AreEqual(dt.GetSeconds(), 0);
			Assert.AreEqual(dt.GetMilliseconds(), 0);
		}

		[Test]
		public void FormatWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void LocaleFormatWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void GetFullYearWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetFullYear(), 2011);
		}

		[Test]
		public void GetMonthWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMonth(), 7);
		}

		[Test]
		public void GetDateWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void GetHoursWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void GetMinutesWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void GetSecondsWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void GetMillisecondsWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMilliseconds(), 345);
		}

		[Test]
		public void GetDayWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDay(), 5);
		}

		[Test]
		public void GetTimeWorks() {
			var dt = new MutableDateTime(DateTime.Utc(1970, 0, 2));
			Assert.AreEqual(dt.GetTime(), 1440 * 60 * 1000);
		}

		[Test]
		public void ValueOfWorks() {
			var dt = new MutableDateTime(DateTime.Utc(1970, 0, 2));
			Assert.AreEqual(dt.ValueOf(), 1440 * 60 * 1000);
		}

		[Test]
		public void GetTimezoneOffsetWorks() {
			var dt = new MutableDateTime(0);
			Assert.AreEqual(dt.GetTimezoneOffset(), new MutableDateTime(1970, 0, 1).ValueOf() / 60000);
		}

		[Test]
		public void GetUtcFullYearWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcFullYear(), 2011);
		}

		[Test]
		public void GetUtcMonthWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMonth(), 7);
		}

		[Test]
		public void GetUtcDateWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void GetUtcHoursWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcHours(), 13);
		}

		[Test]
		public void GetUtcMinutesWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMinutes(), 42);
		}

		[Test]
		public void GetUtcSecondsWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcSeconds(), 56);
		}

		[Test]
		public void GetUtcMillisecondsWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMilliseconds(), 345);
		}

		[Test]
		public void GetUtcDayWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDay(), 5);
		}

		[Test]
		public void ParseWorks() {
			var dt = MutableDateTime.Parse("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWorks() {
			var dt = MutableDateTime.ParseExact("2012-12-08", "yyyy-dd-MM");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWithCultureWorks() {
			var dt = MutableDateTime.ParseExact("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactUtcWorks() {
			var dt = MutableDateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM");
			Assert.AreEqual(dt.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcWithCultureWorks() {
			var dt = MutableDateTime.ParseExactUtc("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.AreEqual(dt.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void ToDateStringWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			var s = dt.ToDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToTimeStringWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			var s = dt.ToTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToUtcStringWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			var s = dt.ToUtcString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToLocaleDateStringWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToLocaleTimeStringWorks() {
			var dt = new MutableDateTime(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		private void AssertDateUtc(MutableDateTime dt, int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
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
			AssertDateUtc(MutableDateTime.FromUtc(2011, 7, 12), 2011, 7, 12, 0, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHWorks() {
			AssertDateUtc(MutableDateTime.FromUtc(2011, 7, 12, 13), 2011, 7, 12, 13, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNWorks() {
			AssertDateUtc(MutableDateTime.FromUtc(2011, 7, 12, 13, 42), 2011, 7, 12, 13, 42, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNSWorks() {
			AssertDateUtc(MutableDateTime.FromUtc(2011, 7, 12, 13, 42, 56), 2011, 7, 12, 13, 42, 56, 0);
		}

		[Test]
		public void FromUtcYMDHNSUWorks() {
			AssertDateUtc(MutableDateTime.FromUtc(2011, 7, 12, 13, 42, 56, 345), 2011, 7, 12, 13, 42, 56, 345);
		}

		[Test]
		public void SubtractingDatesWorks() {
			Assert.AreEqual(new MutableDateTime(2011, 7, 12) - new MutableDateTime(2011, 7, 11), 1440 * 60 * 1000);
		}

		[Test]
		public void DateEqualityWorks() {
			Assert.IsTrue(new MutableDateTime(2011, 7, 12) == new MutableDateTime(2011, 7, 12));
			Assert.IsFalse(new MutableDateTime(2011, 7, 12) == new MutableDateTime(2011, 7, 13));
			Assert.AreStrictEqual(new MutableDateTime(2011, 7, 12) == (MutableDateTime)null, false);
			Assert.AreStrictEqual((MutableDateTime)null == new MutableDateTime(2011, 7, 12), false);
			Assert.AreStrictEqual((MutableDateTime)null == (MutableDateTime)null, true);
		}

		[Test]
		public void DateInequalityWorks() {
			Assert.IsFalse(new MutableDateTime(2011, 7, 12) != new MutableDateTime(2011, 7, 12));
			Assert.IsTrue(new MutableDateTime(2011, 7, 12) != new MutableDateTime(2011, 7, 13));
			Assert.AreStrictEqual(new MutableDateTime(2011, 7, 12) != (MutableDateTime)null, true);
			Assert.AreStrictEqual((MutableDateTime)null != new MutableDateTime(2011, 7, 12), true);
			Assert.AreStrictEqual((MutableDateTime)null != (MutableDateTime)null, false);
		}

		[Test]
		public void DateLessThanWorks() {
			Assert.IsTrue(new MutableDateTime(2011, 7, 11) < new MutableDateTime(2011, 7, 12));
			Assert.IsFalse(new MutableDateTime(2011, 7, 12) < new MutableDateTime(2011, 7, 12));
			Assert.IsFalse(new MutableDateTime(2011, 7, 13) < new MutableDateTime(2011, 7, 12));
		}

		[Test]
		public void DateLessEqualWorks() {
			Assert.IsTrue(new MutableDateTime(2011, 7, 11) <= new MutableDateTime(2011, 7, 12));
			Assert.IsTrue(new MutableDateTime(2011, 7, 12) <= new MutableDateTime(2011, 7, 12));
			Assert.IsFalse(new MutableDateTime(2011, 7, 13) <= new MutableDateTime(2011, 7, 12));
		}

		[Test]
		public void DateGreaterThanWorks() {
			Assert.IsFalse(new MutableDateTime(2011, 7, 11) > new MutableDateTime(2011, 7, 12));
			Assert.IsFalse(new MutableDateTime(2011, 7, 12) > new MutableDateTime(2011, 7, 12));
			Assert.IsTrue(new MutableDateTime(2011, 7, 13) > new MutableDateTime(2011, 7, 12));
		}

		[Test]
		public void DateGreaterEqualWorks() {
			Assert.IsFalse(new MutableDateTime(2011, 7, 11) >= new MutableDateTime(2011, 7, 12));
			Assert.IsTrue(new MutableDateTime(2011, 7, 12) >= new MutableDateTime(2011, 7, 12));
			Assert.IsTrue(new MutableDateTime(2011, 7, 13) >= new MutableDateTime(2011, 7, 12));
		}

        [Test]
        public void SetFullYearWithOneParameterWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetFullYear(2021);
			Assert.AreEqual(dt.GetFullYear(), 2021);
        }

        [Test]
        public void SetFullYearWithTwoParametersWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetFullYear(2021, 7);
			Assert.AreEqual(dt.GetFullYear(), 2021);
			Assert.AreEqual(dt.GetMonth(), 7);
        }

        [Test]
        public void SetFullYearWithThreeParametersWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetFullYear(2021, 7, 13);
			Assert.AreEqual(dt.GetFullYear(), 2021);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 13);
        }

		[Test]
        public void SetMonthWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetMonth(3);
			Assert.AreEqual(dt.GetMonth(), 3);
        }

        [Test]
		public void SetDateWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetDate(12);
			Assert.AreEqual(dt.GetDate(), 12);
        }

        [Test]
        public void SetHoursWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetHours(11);
			Assert.AreEqual(dt.GetHours(), 11);
        }

        [Test]
        public void SetMinutesWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetMinutes(34);
			Assert.AreEqual(dt.GetMinutes(), 34);
        }

        [Test]
        public void SetSecondsWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetSeconds(23);
			Assert.AreEqual(dt.GetSeconds(), 23);
        }

        [Test]
        public void SetMillisecondsWorks() {
			var dt = new MutableDateTime(2000, 0, 1);
			dt.SetMilliseconds(435);
			Assert.AreEqual(dt.GetMilliseconds(), 435);
        }

        [Test]
        public void SetTimeWorks() {
			var dt = new MutableDateTime();
			dt.SetTime(3498302349234L);
			Assert.AreEqual(dt.GetTime(), 3498302349234L);
        }

        [Test]
        public void SetUtcFullYearWithOneParameterWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
        }

        [Test]
        public void SetUtcFullYearWithTwoParametersWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021, 7);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
        }

        [Test]
        public void SetUtcFullYearWithThreeParametersWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021, 7, 13);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 13);
        }

		[Test]
        public void SetUtcMonthWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcMonth(3);
			Assert.AreEqual(dt.GetUtcMonth(), 3);
        }

        [Test]
		public void SetUtcDateWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcDate(12);
			Assert.AreEqual(dt.GetUtcDate(), 12);
        }

        [Test]
        public void SetUtcHoursWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcHours(11);
			Assert.AreEqual(dt.GetUtcHours(), 11);
        }

        [Test]
        public void SetUtcMinutesWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcMinutes(34);
			Assert.AreEqual(dt.GetUtcMinutes(), 34);
        }

        [Test]
        public void SetUtcSecondsWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcSeconds(23);
			Assert.AreEqual(dt.GetUtcSeconds(), 23);
        }

        [Test]
        public void SetUtcMillisecondsWorks() {
			var dt = new MutableDateTime(DateTime.Utc(2000, 0, 1));
			dt.SetUtcMilliseconds(435);
			Assert.AreEqual(dt.GetUtcMilliseconds(), 435);
        }
	}
}
