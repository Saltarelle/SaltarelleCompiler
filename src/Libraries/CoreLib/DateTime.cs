// Date.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// Equivalent to the Date type in Javascript, but emulates value-type semantics by removing all mutators.
    /// </summary>
    [IgnoreNamespace]
	[Imported(IsRealType = true)]
	[ScriptName("Date")]
    public struct DateTime {
        /// <summary>
        /// Creates a new instance of Date initialized from the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Milliseconds since January 1st, 1970.</param>
        [AlternateSignature]
        public DateTime(long milliseconds) {
        }

        /// <summary>
        /// Creates a new instance of Date initialized from parsing the specified date.
        /// </summary>
        /// <param name="date"></param>
        [AlternateSignature]
        public DateTime(string date) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        [AlternateSignature]
        public DateTime(int year, int month, int date) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        [AlternateSignature]
        public DateTime(int year, int month, int date, int hours) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        /// <param name="minutes">The minutes (0 through 59)</param>
        [AlternateSignature]
        public DateTime(int year, int month, int date, int hours, int minutes) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        /// <param name="minutes">The minutes (0 through 59)</param>
        /// <param name="seconds">The seconds (0 through 59)</param>
        [AlternateSignature]
        public DateTime(int year, int month, int date, int hours, int minutes, int seconds) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        /// <param name="minutes">The minutes (0 through 59)</param>
        /// <param name="seconds">The seconds (0 through 59)</param>
        /// <param name="milliseconds">The milliseconds (0 through 999)</param>
        [AlternateSignature]
        public DateTime(int year, int month, int date, int hours, int minutes, int seconds, int milliseconds) {
        }

        /// <summary>
        /// Returns the current date and time.
        /// </summary>
        public static DateTime Now {
            get {
                return null;
            }
        }

        /// <summary>
        /// Returns the current date with the time part set to 00:00:00.
        /// </summary>
        public static DateTime Today {
            get {
                return null;
            }
        }

        public string Format(string format) {
            return null;
        }

        public int GetDate() {
            return 0;
        }

        public int GetDay() {
            return 0;
        }

        public int GetFullYear() {
            return 0;
        }

        public int GetHours() {
            return 0;
        }

        public int GetMilliseconds() {
            return 0;
        }

        public int GetMinutes() {
            return 0;
        }

        public int GetMonth() {
            return 0;
        }

        public int GetSeconds() {
            return 0;
        }

        public long GetTime() {
            return 0;
        }

        public int GetTimezoneOffset() {
            return 0;
        }

        [ScriptName("getUTCDate")]
        public int GetUtcDate() {
            return 0;
        }

        [ScriptName("getUTCDay")]
		public int GetUtcDay() {
            return 0;
        }

        [ScriptName("getUTCFullYear")]
        public int GetUtcFullYear() {
            return 0;
        }

        [ScriptName("getUTCHours")]
        public int GetUtcHours() {
            return 0;
        }

        [ScriptName("getUTCMilliseconds")]
        public int GetUtcMilliseconds() {
            return 0;
        }

        [ScriptName("getUTCMinutes")]
        public int GetUtcMinutes() {
            return 0;
        }

        [ScriptName("getUTCMonth")]
        public int GetUtcMonth() {
            return 0;
        }

        [ScriptName("getUTCSeconds")]
        public int GetUtcSeconds() {
            return 0;
        }

        public string LocaleFormat(string format) {
            return null;
        }

        [ScriptName("parseDate")]
        public static DateTime Parse(string value) {
            return null;
        }

		public static DateTime ParseExact(string value, string format) {
			return null;
		}

		public static DateTime ParseExact(string value, string format, CultureInfo culture) {
			return null;
		}

		[ScriptName("parseExactUTC")]
		public static DateTime ParseExactUtc(string value, string format) {
			return null;
		}

		[ScriptName("parseExactUTC")]
		public static DateTime ParseExactUtc(string value, string format, CultureInfo culture) {
			return null;
		}

        public string ToDateString() {
            return null;
        }

        public string ToLocaleDateString() {
            return null;
        }

        public string ToLocaleTimeString() {
            return null;
        }

        public string ToTimeString() {
            return null;
        }

        [ScriptName("toUTCString")]
		public string ToUtcString() {
            return null;
        }

		public long ValueOf() {
			return 0;
		}

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}))")]
        public static DateTime FromUtc(int year, int month, int day) {
            return default(DateTime);
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}))")]
        public static DateTime FromUtc(int year, int month, int day, int hours) {
            return default(DateTime);
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}))")]
        public static DateTime FromUtc(int year, int month, int day, int hours, int minutes) {
            return default(DateTime);
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}))")]
        public static DateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds) {
            return default(DateTime);
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}, {milliseconds}))")]
        public static DateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
            return default(DateTime);
        }

        [ScriptName("UTC")]
        public static int Utc(int year, int month, int day) {
            return 0;
        }

        [ScriptName("UTC")]
        public static int Utc(int year, int month, int day, int hours) {
            return 0;
        }

        [ScriptName("UTC")]
        public static int Utc(int year, int month, int day, int hours, int minutes) {
            return 0;
        }

        [ScriptName("UTC")]
        public static int Utc(int year, int month, int day, int hours, int minutes, int seconds) {
            return 0;
        }

        [ScriptName("UTC")]
        public static int Utc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
            return 0;
        }

        // NOTE: There is no + operator since in JavaScript that returns the
        //       concatenation of the date strings, which is pretty much useless.

        /// <summary>
        /// Returns the difference in milliseconds between two dates.
        /// </summary>
        [IntrinsicOperator]
        public static int operator -(DateTime a, DateTime b) {
            return 0;
        }

		public static bool AreEqual(DateTime? a, DateTime? b) {
			return false;
		}

		// Unfortunately we can't define the equality and inequality operators, because 1) the JS versions are terrible (use reference equality), and 2) if we define them as a static method, hell breaks loose when lifting it.

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator <(DateTime a, DateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator >(DateTime a, DateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator <=(DateTime a, DateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator >=(DateTime a, DateTime b) {
            return false;
        }

		/// <summary>
		/// Converts a DateTime to a MutableDateTime. Returns a copy of the immutable datetime.
		/// </summary>
		[InlineCode("new Date({dt}.valueOf())")]
		public static implicit operator DateTime(MutableDateTime dt) {
			return default(DateTime);
		}

		/// <summary>
		/// Converts an ImmutableDateTime to a DateTime. Returns a copy of the mutable datetime.
		/// </summary>
		[InlineCode("new Date({dt}.valueOf())")]
		public static implicit operator MutableDateTime(DateTime dt) {
			return null;
		}
    }
}
