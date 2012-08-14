// Date.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
    /// <summary>
    /// Equivalent to the Date type in Javascript.
    /// </summary>
    [ScriptNamespace("ss")]
	[Imported(IsRealType = true)]
    public sealed class MutableDateTime {
        /// <summary>
        /// Creates a new instance of Date initialized from the current time.
        /// </summary>
        [InlineCode("new Date()")]
        public MutableDateTime() {
        }

        /// <summary>
        /// Creates a new instance of Date initialized from the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Milliseconds since January 1st, 1970.</param>
        [InlineCode("new Date({milliseconds})")]
        public MutableDateTime(long milliseconds) {
        }

        /// <summary>
        /// Creates a new instance of Date initialized from parsing the specified date.
        /// </summary>
        /// <param name="date"></param>
        [InlineCode("new Date({date})")]
        public MutableDateTime(string date) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        [InlineCode("new Date({year}, {month}, {date})")]
        public MutableDateTime(int year, int month, int date) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        [InlineCode("new Date({year}, {month}, {date}, {hours})")]
        public MutableDateTime(int year, int month, int date, int hours) {
        }

        /// <summary>
        /// Creates a new instance of Date.
        /// </summary>
        /// <param name="year">The full year.</param>
        /// <param name="month">The month (0 through 11)</param>
        /// <param name="date">The day of the month (1 through # of days in the specified month)</param>
        /// <param name="hours">The hours (0 through 23)</param>
        /// <param name="minutes">The minutes (0 through 59)</param>
        [InlineCode("new Date({year}, {month}, {date}, {hours}, {minutes})")]
        public MutableDateTime(int year, int month, int date, int hours, int minutes) {
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
        [InlineCode("new Date({year}, {month}, {date}, {hours}, {minutes}, {seconds})")]
        public MutableDateTime(int year, int month, int date, int hours, int minutes, int seconds) {
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
        [InlineCode("new Date({year}, {month}, {date}, {hours}, {minutes}, {seconds}, {milliseconds})")]
        public MutableDateTime(int year, int month, int date, int hours, int minutes, int seconds, int milliseconds) {
        }

        /// <summary>
        /// Returns the current date and time.
        /// </summary>
        public static MutableDateTime Now {
			[InlineCode("{$System.DateTime}.get_now()")]
            get {
                return null;
            }
        }

        /// <summary>
        /// Returns the current date with the time part set to 00:00:00.
        /// </summary>
        public static MutableDateTime Today {
			[InlineCode("{$System.DateTime}.get_today()")]
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

        [InlineCode("{$System.DateTime}.parseDate({value})")]
        public static MutableDateTime Parse(string value) {
            return null;
        }

        [InlineCode("{$System.DateTime}.parseExact({value}, {format})")]
		public static MutableDateTime ParseExact(string value, string format) {
			return null;
		}

        [InlineCode("{$System.DateTime}.parseExact({value}, {format}, {culture})")]
		public static MutableDateTime ParseExact(string value, string format, CultureInfo culture) {
			return null;
		}

        [InlineCode("{$System.DateTime}.parseExactUTC({value}, {format})")]
		public static MutableDateTime ParseExactUtc(string value, string format) {
			return null;
		}

        [InlineCode("{$System.DateTime}.parseExactUTC({value}, {format}, {culture})")]
		public static MutableDateTime ParseExactUtc(string value, string format, CultureInfo culture) {
			return null;
		}

        public void SetDate(int date) {
        }

        public void SetFullYear(int year) {
        }

        public void SetFullYear(int year, int month) {
        }

        public void SetFullYear(int year, int month, int day) {
        }

        public void SetHours(int hours) {
        }

        public void SetMilliseconds(int milliseconds) {
        }

        public void SetMinutes(int minutes) {
        }

        public void SetMonth(int month) {
        }

        public void SetSeconds(int seconds) {
        }

        public void SetTime(long milliseconds) {
        }

        [ScriptName("setUTCDate")]
		public void SetUtcDate(int date) {
        }

        [ScriptName("setUTCFullYear")]
        public void SetUtcFullYear(int year) {
        }

        [ScriptName("setUTCFullYear")]
        public void SetUtcFullYear(int year, int month) {
        }

        [ScriptName("setUTCFullYear")]
        public void SetUtcFullYear(int year, int month, int day) {
        }

        [ScriptName("setUTCHours")]
        public void SetUtcHours(int hours) {
        }

        [ScriptName("setUTCMilliseconds")]
        public void SetUtcMilliseconds(int milliseconds) {
        }

        [ScriptName("setUTCMinutes")]
        public void SetUtcMinutes(int minutes) {
        }

        [ScriptName("setUTCMonth")]
        public void SetUtcMonth(int month) {
        }

        [ScriptName("setUTCSeconds")]
        public void SetUtcSeconds(int seconds) {
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
        public static MutableDateTime FromUtc(int year, int month, int day) {
            return null;
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}))")]
        public static MutableDateTime FromUtc(int year, int month, int day, int hours) {
            return null;
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}))")]
        public static MutableDateTime FromUtc(int year, int month, int day, int hours, int minutes) {
            return null;
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}))")]
        public static MutableDateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds) {
            return null;
        }

        [InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}, {milliseconds}))")]
        public static MutableDateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
            return null;
        }

        // NOTE: There is no + operator since in JavaScript that returns the
        //       concatenation of the date strings, which is pretty much useless.

        /// <summary>
        /// Returns the difference in milliseconds between two dates.
        /// </summary>
        [IntrinsicOperator]
        public static int operator -(MutableDateTime a, MutableDateTime b) {
            return 0;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [InlineCode("{$System.DateTime}.areEqual({a}, {b})")]
        public static bool operator ==(MutableDateTime a, MutableDateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [InlineCode("!{$System.DateTime}.areEqual({a}, {b})")]
        public static bool operator !=(MutableDateTime a, MutableDateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator <(MutableDateTime a, MutableDateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator >(MutableDateTime a, MutableDateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator <=(MutableDateTime a, MutableDateTime b) {
            return false;
        }

        /// <summary>
        /// Compares two dates
        /// </summary>
        [IntrinsicOperator]
        public static bool operator >=(MutableDateTime a, MutableDateTime b) {
            return false;
        }
    }
}
