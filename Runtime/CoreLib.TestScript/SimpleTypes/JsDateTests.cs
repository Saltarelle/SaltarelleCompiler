using System;
using QUnit;
using System.Globalization;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class JsDateTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(JsDate).FullName, "ss.JsDate");
			Assert.IsTrue(typeof(JsDate).IsClass);
			Assert.IsTrue(typeof(IComparable<JsDate>).IsAssignableFrom(typeof(JsDate)));
			Assert.IsTrue(typeof(IEquatable<JsDate>).IsAssignableFrom(typeof(JsDate)));
			object o = new JsDate();
			Assert.IsTrue(o is JsDate);
			Assert.IsTrue(o is IComparable<JsDate>);
			Assert.IsTrue(o is IEquatable<JsDate>);

			var interfaces = typeof(JsDate).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<JsDate>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<JsDate>)));
		}

		[Test]
		public void DefaultConstructorReturnsTodaysDate() {
			var dt = new JsDate();
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void CreatingInstanceReturnsTodaysDate() {
			Assert.IsTrue(Activator.CreateInstance<JsDate>().GetFullYear() > 2011);
		}

		[Test]
		public void MillisecondSinceEpochConstructorWorks() {
			var dt = new JsDate(1440L * 60 * 500 * 1000);
			Assert.AreEqual(dt.GetFullYear(), 1971);
		}

		[Test]
		public void StringConstructorWorks() {
			var dt = new JsDate("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDConstructorWorks() {
			var dt = new JsDate(2011, 7, 12);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void YMDHConstructorWorks() {
			var dt = new JsDate(2011, 7, 12, 13);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void YMDHNConstructorWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void YMDHNSConstructorWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56);
			Assert.AreEqual(dt.GetFullYear(), 2011);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
			Assert.AreEqual(dt.GetHours(), 13);
			Assert.AreEqual(dt.GetMinutes(), 42);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void YMDHNSUConstructorWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
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
			var dt = JsDate.Now;
			Assert.IsTrue(dt.GetFullYear() > 2011);
		}

		[Test]
		public void UtcNowWorks() {
			var utc   = JsDate.UtcNow;
			var local = JsDate.Now;
			Assert.IsTrue(Math.Abs(new JsDate(local.GetUtcFullYear(), local.GetUtcMonth(), local.GetUtcDate(), local.GetUtcHours(), local.GetUtcMinutes(), local.GetUtcSeconds(), local.GetUtcMilliseconds()) - utc) < 1000);
		}

		[Test]
		public void ToUniversalWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			var utc = dt.ToUniversalTime();
			Assert.AreEqual(dt.GetUtcFullYear(), utc.GetFullYear());
			Assert.AreEqual(dt.GetUtcMonth(), utc.GetMonth());
			Assert.AreEqual(dt.GetUtcDate(), utc.GetDate());
			Assert.AreEqual(dt.GetUtcHours(), utc.GetHours());
			Assert.AreEqual(dt.GetUtcMinutes(), utc.GetMinutes());
			Assert.AreEqual(dt.GetUtcSeconds(), utc.GetSeconds());
			Assert.AreEqual(dt.GetUtcMilliseconds(), utc.GetMilliseconds());
		}

		[Test]
		public void ToLocalWorks() {
			var utc = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			var dt = utc.ToLocalTime();
			Assert.AreEqual(dt.GetUtcFullYear(), utc.GetFullYear());
			Assert.AreEqual(dt.GetUtcMonth(), utc.GetMonth());
			Assert.AreEqual(dt.GetUtcDate(), utc.GetDate());
			Assert.AreEqual(dt.GetUtcHours(), utc.GetHours());
			Assert.AreEqual(dt.GetUtcMinutes(), utc.GetMinutes());
			Assert.AreEqual(dt.GetUtcSeconds(), utc.GetSeconds());
			Assert.AreEqual(dt.GetUtcMilliseconds(), utc.GetMilliseconds());
		}

		[Test]
		public void TodayWorks() {
			var dt = JsDate.Today;
			Assert.IsTrue(dt.GetFullYear() > 2011);
			Assert.AreEqual(dt.GetHours(), 0);
			Assert.AreEqual(dt.GetMinutes(), 0);
			Assert.AreEqual(dt.GetSeconds(), 0);
			Assert.AreEqual(dt.GetMilliseconds(), 0);
		}

		[Test]
		public void FormatWorks() {
			var dt = new JsDate(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void IFormattableToStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13);
			Assert.AreEqual(dt.ToString("yyyy-MM-dd"), "2011-08-12");
			Assert.AreEqual(((IFormattable)dt).ToString("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void LocaleFormatWorks() {
			var dt = new JsDate(2011, 7, 12, 13);
			Assert.AreEqual(dt.Format("yyyy-MM-dd"), "2011-08-12");
		}

		[Test]
		public void GetFullYearWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetFullYear(), 2011);
		}

		[Test]
		public void GetMonthWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMonth(), 7);
		}

		[Test]
		public void GetDateWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void GetHoursWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetHours(), 13);
		}

		[Test]
		public void GetMinutesWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMinutes(), 42);
		}

		[Test]
		public void GetSecondsWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetSeconds(), 56);
		}

		[Test]
		public void GetMillisecondsWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetMilliseconds(), 345);
		}

		[Test]
		public void GetDayWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42, 56, 345);
			Assert.AreEqual(dt.GetDay(), 5);
		}

		[Test]
		public void GetTimeWorks() {
			var dt = new JsDate(JsDate.Utc(1970, 0, 2));
			Assert.AreEqual(dt.GetTime(), 1440 * 60 * 1000);
		}

		[Test]
		public void ValueOfWorks() {
			var dt = new JsDate(JsDate.Utc(1970, 0, 2));
			Assert.AreEqual(dt.ValueOf(), 1440 * 60 * 1000);
		}

		[Test]
		public void GetTimezoneOffsetWorks() {
			var dt = new JsDate(0);
			Assert.AreEqual(dt.GetTimezoneOffset(), new JsDate(1970, 0, 1).ValueOf() / 60000);
		}

		[Test]
		public void GetUtcFullYearWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcFullYear(), 2011);
		}

		[Test]
		public void GetUtcMonthWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMonth(), 7);
		}

		[Test]
		public void GetUtcDateWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void GetUtcHoursWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcHours(), 13);
		}

		[Test]
		public void GetUtcMinutesWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMinutes(), 42);
		}

		[Test]
		public void GetUtcSecondsWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcSeconds(), 56);
		}

		[Test]
		public void GetUtcMillisecondsWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcMilliseconds(), 345);
		}

		[Test]
		public void GetUtcDayWorks() {
			var dt = new JsDate(JsDate.Utc(2011, 7, 12, 13, 42, 56, 345));
			Assert.AreEqual(dt.GetUtcDay(), 5);
		}

		[Test]
		public void ParseWorks() {
			var dt = JsDate.Parse("Aug 12, 2012");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWorks() {
			var dt = JsDate.ParseExact("2012-12-08", "yyyy-dd-MM");
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactWithCultureWorks() {
			var dt = JsDate.ParseExact("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.AreEqual(dt.GetFullYear(), 2012);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 12);
		}

		[Test]
		public void ParseExactUtcWorks() {
			var dt = JsDate.ParseExactUtc("2012-12-08", "yyyy-dd-MM");
			Assert.AreEqual(dt.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void ParseExactUtcWithCultureWorks() {
			var dt = JsDate.ParseExactUtc("2012-12-08", "yyyy-dd-MM", CultureInfo.InvariantCulture);
			Assert.AreEqual(dt.GetUtcFullYear(), 2012);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 12);
		}

		[Test]
		public void ToDateStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			var s = dt.ToDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToTimeStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			var s = dt.ToTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToUtcStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			var s = dt.ToUtcString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") >= 0);
		}

		[Test]
		public void ToLocaleDateStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleDateString();
			Assert.IsTrue(s.IndexOf("2011") >= 0 && s.IndexOf("42") < 0);
		}

		[Test]
		public void ToLocaleTimeStringWorks() {
			var dt = new JsDate(2011, 7, 12, 13, 42);
			var s = dt.ToLocaleTimeString();
			Assert.IsTrue(s.IndexOf("2011") < 0 && s.IndexOf("42") >= 0);
		}

		private void AssertDateUtc(JsDate dt, int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
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
			AssertDateUtc(JsDate.FromUtc(2011, 7, 12), 2011, 7, 12, 0, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHWorks() {
			AssertDateUtc(JsDate.FromUtc(2011, 7, 12, 13), 2011, 7, 12, 13, 0, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNWorks() {
			AssertDateUtc(JsDate.FromUtc(2011, 7, 12, 13, 42), 2011, 7, 12, 13, 42, 0, 0);
		}

		[Test]
		public void FromUtcYMDHNSWorks() {
			AssertDateUtc(JsDate.FromUtc(2011, 7, 12, 13, 42, 56), 2011, 7, 12, 13, 42, 56, 0);
		}

		[Test]
		public void FromUtcYMDHNSUWorks() {
			AssertDateUtc(JsDate.FromUtc(2011, 7, 12, 13, 42, 56, 345), 2011, 7, 12, 13, 42, 56, 345);
		}

		[Test]
		public void SubtractingDatesWorks() {
			Assert.AreEqual(new JsDate(2011, 7, 12) - new JsDate(2011, 7, 11), 1440 * 60 * 1000);
		}

		[Test]
		public void SubtractMethodReturningTimeSpanWorks() {
			Assert.AreEqual(new JsDate(2011, 6, 12).Subtract(new JsDate(2011, 6, 11)), new TimeSpan(1, 0, 0, 0));
			Assert.AreEqual(new JsDate(2011, 6, 12, 15, 0, 0).Subtract(new JsDate(2011, 6, 11, 13, 0, 0)), new TimeSpan(1, 2, 0, 0));
		}

		[Test]
		public void DateEqualityWorks() {
			Assert.IsTrue(new JsDate(2011, 7, 12) == new JsDate(2011, 7, 12));
			Assert.IsFalse(new JsDate(2011, 7, 12) == new JsDate(2011, 7, 13));
			Assert.AreStrictEqual(new JsDate(2011, 7, 12) == (JsDate)null, false);
			Assert.AreStrictEqual((JsDate)null == new JsDate(2011, 7, 12), false);
			Assert.AreStrictEqual((JsDate)null == (JsDate)null, true);
		}

		[Test]
		public void DateInequalityWorks() {
			Assert.IsFalse(new JsDate(2011, 7, 12) != new JsDate(2011, 7, 12));
			Assert.IsTrue(new JsDate(2011, 7, 12) != new JsDate(2011, 7, 13));
			Assert.AreStrictEqual(new JsDate(2011, 7, 12) != (JsDate)null, true);
			Assert.AreStrictEqual((JsDate)null != new JsDate(2011, 7, 12), true);
			Assert.AreStrictEqual((JsDate)null != (JsDate)null, false);
		}

		[Test]
		public void DateLessThanWorks() {
			Assert.IsTrue(new JsDate(2011, 7, 11) < new JsDate(2011, 7, 12));
			Assert.IsFalse(new JsDate(2011, 7, 12) < new JsDate(2011, 7, 12));
			Assert.IsFalse(new JsDate(2011, 7, 13) < new JsDate(2011, 7, 12));
		}

		[Test]
		public void DateLessEqualWorks() {
			Assert.IsTrue(new JsDate(2011, 7, 11) <= new JsDate(2011, 7, 12));
			Assert.IsTrue(new JsDate(2011, 7, 12) <= new JsDate(2011, 7, 12));
			Assert.IsFalse(new JsDate(2011, 7, 13) <= new JsDate(2011, 7, 12));
		}

		[Test]
		public void DateGreaterThanWorks() {
			Assert.IsFalse(new JsDate(2011, 7, 11) > new JsDate(2011, 7, 12));
			Assert.IsFalse(new JsDate(2011, 7, 12) > new JsDate(2011, 7, 12));
			Assert.IsTrue(new JsDate(2011, 7, 13) > new JsDate(2011, 7, 12));
		}

		[Test]
		public void DateGreaterEqualWorks() {
			Assert.IsFalse(new JsDate(2011, 7, 11) >= new JsDate(2011, 7, 12));
			Assert.IsTrue(new JsDate(2011, 7, 12) >= new JsDate(2011, 7, 12));
			Assert.IsTrue(new JsDate(2011, 7, 13) >= new JsDate(2011, 7, 12));
		}

        [Test]
        public void SetFullYearWithOneParameterWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetFullYear(2021);
			Assert.AreEqual(dt.GetFullYear(), 2021);
        }

        [Test]
        public void SetFullYearWithTwoParametersWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetFullYear(2021, 7);
			Assert.AreEqual(dt.GetFullYear(), 2021);
			Assert.AreEqual(dt.GetMonth(), 7);
        }

        [Test]
        public void SetFullYearWithThreeParametersWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetFullYear(2021, 7, 13);
			Assert.AreEqual(dt.GetFullYear(), 2021);
			Assert.AreEqual(dt.GetMonth(), 7);
			Assert.AreEqual(dt.GetDate(), 13);
        }

		[Test]
        public void SetMonthWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetMonth(3);
			Assert.AreEqual(dt.GetMonth(), 3);
        }

        [Test]
		public void SetDateWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetDate(12);
			Assert.AreEqual(dt.GetDate(), 12);
        }

        [Test]
        public void SetHoursWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetHours(11);
			Assert.AreEqual(dt.GetHours(), 11);
        }

        [Test]
        public void SetMinutesWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetMinutes(34);
			Assert.AreEqual(dt.GetMinutes(), 34);
        }

        [Test]
        public void SetSecondsWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetSeconds(23);
			Assert.AreEqual(dt.GetSeconds(), 23);
        }

        [Test]
        public void SetMillisecondsWorks() {
			var dt = new JsDate(2000, 0, 1);
			dt.SetMilliseconds(435);
			Assert.AreEqual(dt.GetMilliseconds(), 435);
        }

        [Test]
        public void SetTimeWorks() {
			var dt = new JsDate();
			dt.SetTime(3498302349234L);
			Assert.AreEqual(dt.GetTime(), 3498302349234L);
        }

        [Test]
        public void SetUtcFullYearWithOneParameterWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
        }

        [Test]
        public void SetUtcFullYearWithTwoParametersWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021, 7);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
        }

        [Test]
        public void SetUtcFullYearWithThreeParametersWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcFullYear(2021, 7, 13);
			Assert.AreEqual(dt.GetUtcFullYear(), 2021);
			Assert.AreEqual(dt.GetUtcMonth(), 7);
			Assert.AreEqual(dt.GetUtcDate(), 13);
        }

		[Test]
        public void SetUtcMonthWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcMonth(3);
			Assert.AreEqual(dt.GetUtcMonth(), 3);
        }

        [Test]
		public void SetUtcDateWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcDate(12);
			Assert.AreEqual(dt.GetUtcDate(), 12);
        }

        [Test]
        public void SetUtcHoursWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcHours(11);
			Assert.AreEqual(dt.GetUtcHours(), 11);
        }

        [Test]
        public void SetUtcMinutesWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcMinutes(34);
			Assert.AreEqual(dt.GetUtcMinutes(), 34);
        }

        [Test]
        public void SetUtcSecondsWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcSeconds(23);
			Assert.AreEqual(dt.GetUtcSeconds(), 23);
        }

        [Test]
        public void SetUtcMillisecondsWorks() {
			var dt = new JsDate(JsDate.Utc(2000, 0, 1));
			dt.SetUtcMilliseconds(435);
			Assert.AreEqual(dt.GetUtcMilliseconds(), 435);
        }

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (new JsDate(0).GetHashCode(), new JsDate(0).GetHashCode());
			Assert.AreEqual   (new JsDate(1).GetHashCode(), new JsDate(1).GetHashCode());
			Assert.AreNotEqual(new JsDate(0).GetHashCode(), new JsDate(1).GetHashCode());
			Assert.IsTrue((long)new JsDate(3000, 1, 1).GetHashCode() < 0xffffffffL);
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue( new JsDate(0).Equals((object)new JsDate(0)));
			Assert.IsFalse(new JsDate(1).Equals((object)new JsDate(0)));
			Assert.IsFalse(new JsDate(0).Equals((object)new JsDate(1)));
			Assert.IsTrue( new JsDate(1).Equals((object)new JsDate(1)));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue( new JsDate(0).Equals(new JsDate(0)));
			Assert.IsFalse(new JsDate(1).Equals(new JsDate(0)));
			Assert.IsFalse(new JsDate(0).Equals(new JsDate(1)));
			Assert.IsTrue( new JsDate(1).Equals(new JsDate(1)));

			Assert.IsTrue( ((IEquatable<JsDate>)new JsDate(0)).Equals(new JsDate(0)));
			Assert.IsFalse(((IEquatable<JsDate>)new JsDate(1)).Equals(new JsDate(0)));
			Assert.IsFalse(((IEquatable<JsDate>)new JsDate(0)).Equals(new JsDate(1)));
			Assert.IsTrue( ((IEquatable<JsDate>)new JsDate(1)).Equals(new JsDate(1)));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(new JsDate(0).CompareTo(new JsDate(0)) == 0);
			Assert.IsTrue(new JsDate(1).CompareTo(new JsDate(0)) > 0);
			Assert.IsTrue(new JsDate(0).CompareTo(new JsDate(1)) < 0);
			Assert.IsTrue(new JsDate(0).CompareTo(null) > 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<JsDate>)new JsDate(0)).CompareTo(new JsDate(0)) == 0);
			Assert.IsTrue(((IComparable<JsDate>)new JsDate(1)).CompareTo(new JsDate(0)) > 0);
			Assert.IsTrue(((IComparable<JsDate>)new JsDate(0)).CompareTo(new JsDate(1)) < 0);
			Assert.IsTrue(((IComparable<JsDate>)new JsDate(0)).CompareTo(null) > 0);
		}
	}
}
