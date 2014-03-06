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
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Date")]
    public struct DateTime : IComparable<DateTime>, IEquatable<DateTime>, IFormattable, ILocaleFormattable {
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
		/// <param name="month">The month (1 through 12)</param>
		/// <param name="day">The day of the month (1 through # of days in the specified month)</param>
		[InlineCode("new {$System.DateTime}({year}, {month} - 1, {day})")]
		public DateTime(int year, int month, int day) {
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (1 through 12)</param>
		/// <param name="day">The day of the month (1 through # of days in the specified month)</param>
		/// <param name="hours">The hours (0 through 23)</param>
		[InlineCode("new {$System.DateTime}({year}, {month} - 1, {day}, {hours})")]
		public DateTime(int year, int month, int day, int hours)
		{
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (1 through 12)</param>
		/// <param name="day">The day of the month (1 through # of days in the specified month)</param>
		/// <param name="hours">The hours (0 through 23)</param>
		/// <param name="minutes">The minutes (0 through 59)</param>
		[InlineCode("new {$System.DateTime}({year}, {month} - 1, {day}, {hours}, {minutes})")]
		public DateTime(int year, int month, int day, int hours, int minutes)
		{
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (1 through 12)</param>
		/// <param name="day">The day of the month (1 through # of days in the specified month)</param>
		/// <param name="hours">The hours (0 through 23)</param>
		/// <param name="minutes">The minutes (0 through 59)</param>
		/// <param name="seconds">The seconds (0 through 59)</param>
		[InlineCode("new {$System.DateTime}({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds})")]
		public DateTime(int year, int month, int day, int hours, int minutes, int seconds)
		{
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (1 through 12)</param>
		/// <param name="day">The day of the month (1 through # of days in the specified month)</param>
		/// <param name="hours">The hours (0 through 23)</param>
		/// <param name="minutes">The minutes (0 through 59)</param>
		/// <param name="seconds">The seconds (0 through 59)</param>
		/// <param name="milliseconds">The milliseconds (0 through 999)</param>
		[InlineCode("new {$System.DateTime}({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}, {milliseconds})")]
		public DateTime(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds)
		{
		}

		/// <summary>
		/// Returns the current date and time.
		/// </summary>
		public static DateTime Now { [InlineCode("new Date()")] get { return default(DateTime); } }

		/// <summary>
		/// Returns the current date and time according to UTC
		/// </summary>
		public static DateTime UtcNow { [InlineCode("{$System.Script}.utcNow()")] get { return default(DateTime); } }

		[InlineCode("{$System.Script}.toUTC({this})")]
		public DateTime ToUniversalTime() {
			return default(DateTime);
		}

		[InlineCode("{$System.Script}.fromUTC({this})")]
		public DateTime ToLocalTime() {
			return default(DateTime);
		}

		/// <summary>
		/// Gets the current date.
		/// </summary>
		/// <returns>
		/// An object that is set to today's date, with the time component set to 00:00:00.
		/// </returns>
		public static DateTime Today { [InlineCode("{$System.Script}.today()")] get { return default(DateTime); } }

		[InlineCode("{$System.Script}.formatDate({this}, {format})")]
		public string Format(string format) {
			return null;
		}

		[InlineCode("{$System.Script}.formatDate({this}, {format})")]
		public string ToString(string format) {
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

		[InlineCode("{this}.getMonth() + 1")]
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

		[InlineCode("{this}.getUTCMonth() + 1")]
		public int GetUtcMonth() {
			return 0;
		}

		[ScriptName("getUTCSeconds")]
		public int GetUtcSeconds() {
			return 0;
		}

		[InlineCode("{$System.Script}.localeFormatDate({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[InlineCode("new Date(Date.parse({value}))")]
		public static DateTime Parse(string value) {
			return default(DateTime);
		}

		[InlineCode("{$System.Script}.parseExactDate({value}, {format})")]
		public static DateTime? ParseExact(string value, string format) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDate({value}, {format}, {provider})")]
		public static DateTime? ParseExact(string value, string format, IFormatProvider provider) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDateUTC({value}, {format})")]
		public static DateTime? ParseExactUtc(string value, string format) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDateUTC({value}, {format}, {provider})")]
		public static DateTime? ParseExactUtc(string value, string format, IFormatProvider provider) {
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

		[InlineCode("new Date(Date.UTC({year}, {month} - 1, {day}))")]
		public static DateTime FromUtc(int year, int month, int day) {
			return default(DateTime);
		}

		[InlineCode("new Date(Date.UTC({year}, {month} - 1, {day}, {hours}))")]
		public static DateTime FromUtc(int year, int month, int day, int hours) {
			return default(DateTime);
		}

		[InlineCode("new Date(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}))")]
		public static DateTime FromUtc(int year, int month, int day, int hours, int minutes) {
			return default(DateTime);
		}

		[InlineCode("new Date(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}))")]
		public static DateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds) {
			return default(DateTime);
		}

		[InlineCode("new Date(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}, {milliseconds}))")]
		public static DateTime FromUtc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
			return default(DateTime);
		}

		[InlineCode("Date.UTC({year}, {month} - 1, {day})")]
		public static int Utc(int year, int month, int day) {
			return 0;
		}

		[InlineCode("Date.UTC({year}, {month} - 1, {day}, {hours})")]
		public static int Utc(int year, int month, int day, int hours) {
			return 0;
		}

		[InlineCode("Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes})")]
		public static int Utc(int year, int month, int day, int hours, int minutes) {
			return 0;
		}

		[InlineCode("Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds})")]
		public static int Utc(int year, int month, int day, int hours, int minutes, int seconds) {
			return 0;
		}

		[InlineCode("Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}, {milliseconds})")]
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

		[InlineCode("new {$System.TimeSpan}(({this} - {value}) * 10000)")]
		public TimeSpan Subtract(DateTime value) {
			return default(TimeSpan);
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool AreEqual(DateTime? a, DateTime? b) {
			return false;
		}

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool AreNotEqual(DateTime? a, DateTime? b) {
			return false;
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator==(DateTime a, DateTime b) {
			return false;
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator==(DateTime? a, DateTime b) {
			return false;
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator==(DateTime a, DateTime? b) {
			return false;
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator==(DateTime? a, DateTime? b) {
			return false;
		}

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator!=(DateTime a, DateTime b) {
			return false;
		}

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator!=(DateTime? a, DateTime b) {
			return false;
		}

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator!=(DateTime a, DateTime? b) {
			return false;
		}

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator!=(DateTime? a, DateTime? b) {
			return false;
		}

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
		/// Converts a DateTime to a JsDate. Returns a copy of the immutable datetime.
		/// </summary>
		[InlineCode("new Date({dt}.valueOf())")]
		public static explicit operator DateTime(JsDate dt) {
			return default(DateTime);
		}

		/// <summary>
		/// Converts a JsDate to a DateTime. Returns a copy of the mutable datetime.
		/// </summary>
		[InlineCode("new Date({dt}.valueOf())")]
		public static explicit operator JsDate(DateTime dt) {
			return null;
		}


		/// <summary>
		/// Gets the date component of this instance.
		/// </summary>
		/// <returns>
		/// A new object with the same date as this instance, and the time value set to 12:00:00 midnight (00:00:00).
		/// </returns>
		public DateTime Date { [InlineCode("new Date({this}.getFullYear(), {this}.getMonth(), {this}.getDate())")] get { return default(DateTime); } }

		/// <summary>
		/// Gets the day of the month represented by this instance.
		/// </summary>
		/// <returns>
		/// The day component, expressed as a value between 1 and 31.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public int Day { [ScriptName("getDate")] get { return 0; } }

		/// <summary>
		/// Gets the day of the week represented by this instance.
		/// </summary>
		/// <returns>
		/// An enumerated constant that indicates the day of the week of this <see cref="T:System.DateTime"/> value.
		/// </returns>
		public DayOfWeek DayOfWeek { [ScriptName("getDay")] get { return 0; } }

		/// <summary>
		/// Gets the day of the year represented by this instance.
		/// </summary>
		/// <returns>
		/// The day of the year, expressed as a value between 1 and 366.
		/// </returns>
		public int DayOfYear { [InlineCode("Math.ceil(({this} - new Date({this}.getFullYear(), 0, 1)) / 86400000)")] get { return 0; } }

		/// <summary>
		/// Gets the hour component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The hour component, expressed as a value between 0 and 23.
		/// </returns>
		public int Hour { [ScriptName("getHours")] get { return 0; } }

		/// <summary>
		/// Gets the milliseconds component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The milliseconds component, expressed as a value between 0 and 999.
		/// </returns>
		public int Millisecond { [ScriptName("getMilliseconds")] get { return 0; } }

		/// <summary>
		/// Gets the minute component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The minute component, expressed as a value between 0 and 59.
		/// </returns>
		public int Minute { [ScriptName("getMinutes")] get { return 0; } }

		/// <summary>
		/// Gets the month component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The month component, expressed as a value between 1 and 12.
		/// </returns>
		public int Month { [InlineCode("{this}.getMonth() + 1")] get { return 0; } }

		/// <summary>
		/// Gets the seconds component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The seconds component, expressed as a value between 0 and 59.
		/// </returns>
		public int Second { [ScriptName("getSeconds")] get { return 0; } }

		/// <summary>
		/// Gets the year component of the date represented by this instance.
		/// </summary>
		/// <returns>
		/// The year, between 1 and 9999.
		/// </returns>
		public int Year { [ScriptName("getFullYear")] get { return 0; } }

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of days to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of days represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional days. The <paramref name="value"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.valueOf() + Math.round({value} * 86400000))")]
		public DateTime AddDays(double value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of hours to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of hours represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional hours. The <paramref name="value"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.valueOf() + Math.round({value} * 3600000))")]
		public DateTime AddHours(double value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of milliseconds to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of milliseconds represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional milliseconds. The <paramref name="value"/> parameter can be negative or positive. Note that this value is rounded to the nearest integer.</param>
		[InlineCode("new Date({this}.valueOf() + Math.round({value}))")]
		public DateTime AddMilliseconds(double value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of minutes to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of minutes represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional minutes. The <paramref name="value"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.valueOf() + Math.round({value} * 60000))")]
		public DateTime AddMinutes(double value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of months to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and <paramref name="months"/>.
		/// </returns>
		/// <param name="months">A number of months. The <paramref name="months"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.getFullYear(), {this}.getMonth() + {months}, {this}.getDate(), {this}.getHours(), {this}.getMinutes(), {this}.getSeconds(), {this}.getMilliseconds())")]
		public DateTime AddMonths(int months) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of seconds to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of seconds represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of whole and fractional seconds. The <paramref name="value"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.valueOf() + Math.round({value} * 1000))")]
		public DateTime AddSeconds(double value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns a new <see cref="T:System.DateTime"/> that adds the specified number of years to the value of this instance.
		/// </summary>
		/// <returns>
		/// An object whose value is the sum of the date and time represented by this instance and the number of years represented by <paramref name="value"/>.
		/// </returns>
		/// <param name="value">A number of years. The <paramref name="value"/> parameter can be negative or positive. </param>
		[InlineCode("new Date({this}.getFullYear() + {value}, {this}.getMonth(), {this}.getDate(), {this}.getHours(), {this}.getMinutes(), {this}.getSeconds(), {this}.getMilliseconds())")]
		public DateTime AddYears(int value) {
			return default(DateTime);
		}

		/// <summary>
		/// Returns the number of days in the specified month and year.
		/// </summary>
		/// <returns>
		/// The number of days in <paramref name="month"/> for the specified <paramref name="year"/>.For example, if <paramref name="month"/> equals 2 for February, the return value is 28 or 29 depending upon whether <paramref name="year"/> is a leap year.
		/// </returns>
		/// <param name="year">The year. </param><param name="month">The month (a number ranging from 1 to 12). </param>
		[InlineCode("new Date({year}, {month}, -1).getDate() + 1")]
		public static int DaysInMonth(int year, int month) {
			return 0;
		}

		/// <summary>
		/// Returns an indication whether the specified year is a leap year.
		/// </summary>
		/// <returns>
		/// true if <paramref name="year"/> is a leap year; otherwise, false.
		/// </returns>
		/// <param name="year">A 4-digit year. </param>
		[InlineCode("new Date({year}, 2, -1).getDate() === 28")]
		public static bool IsLeapYear(int year) {
			return false;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(DateTime other) {
			return 0;
		}

		/// <summary>
		/// Compares two instances of <see cref="T:System.DateTime"/> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.
		/// </summary>
		/// <returns>
		/// A signed number indicating the relative values of <paramref name="t1"/> and <paramref name="t2"/>.Value Type Condition Less than zero <paramref name="t1"/> is earlier than <paramref name="t2"/>. Zero <paramref name="t1"/> is the same as <paramref name="t2"/>. Greater than zero <paramref name="t1"/> is later than <paramref name="t2"/>.
		/// </returns>
		/// <param name="t1">The first object to compare. </param><param name="t2">The second object to compare. </param>
		[InlineCode("{$System.Script}.compare({t1}, {t2})")]
		public static int Compare(DateTime t1, DateTime t2) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(DateTime other) {
			return false;
		}

		/// <summary>
		/// Returns a value indicating whether two <see cref="T:System.DateTime"/> instances have the same date and time value.
		/// </summary>
		/// <returns>
		/// true if the two values are equal; otherwise, false.
		/// </returns>
		/// <param name="t1">The first object to compare. </param><param name="t2">The second object to compare. </param>
		[InlineCode("{$System.Script}.equalsT({t1}, {t2})")]
		public static bool Equals(DateTime t1, DateTime t2) {
			return false;
		}
	}
}
