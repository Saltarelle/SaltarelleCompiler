// Date.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	[Obsolete("This class cannot be used. If you want an immutable struct (.net semantics), use System.DateTime. If you want a mutable class that is more equivalent to JavaScript Date, use System.JsDate.", true)]
	public sealed class Date {}

	/// <summary>
	/// Equivalent to the Date type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public sealed class JsDate : IComparable<JsDate>, IEquatable<JsDate>, IFormattable {
		/// <summary>
		/// Creates a new instance of Date initialized from the current time.
		/// </summary>
		[InlineCode("new Date()")]
		public JsDate() {
		}

		/// <summary>
		/// Creates a new instance of Date initialized from the specified number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">Milliseconds since January 1st, 1970.</param>
		[InlineCode("new Date({milliseconds})")]
		public JsDate(long milliseconds) {
		}

		/// <summary>
		/// Creates a new instance of Date initialized from parsing the specified date.
		/// </summary>
		/// <param name="date"></param>
		[InlineCode("new Date({date})")]
		public JsDate(string date) {
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (0 through 11)</param>
		/// <param name="date">The day of the month (1 through # of days in the specified month)</param>
		[InlineCode("new Date({year}, {month}, {date})")]
		public JsDate(int year, int month, int date) {
		}

		/// <summary>
		/// Creates a new instance of Date.
		/// </summary>
		/// <param name="year">The full year.</param>
		/// <param name="month">The month (0 through 11)</param>
		/// <param name="date">The day of the month (1 through # of days in the specified month)</param>
		/// <param name="hours">The hours (0 through 23)</param>
		[InlineCode("new Date({year}, {month}, {date}, {hours})")]
		public JsDate(int year, int month, int date, int hours) {
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
		public JsDate(int year, int month, int date, int hours, int minutes) {
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
		public JsDate(int year, int month, int date, int hours, int minutes, int seconds) {
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
		public JsDate(int year, int month, int date, int hours, int minutes, int seconds, int milliseconds) {
		}

		/// <summary>
		/// Returns the current date and time.
		/// </summary>
		public static JsDate Now {
			[InlineCode("new Date()")]
			get {
				return null;
			}
		}

		/// <summary>
		/// Returns the current date with the time part set to 00:00:00.
		/// </summary>
		public static JsDate Today {
			[InlineCode("{$System.Script}.today()")]
			get {
				return null;
			}
		}

		public static JsDate UtcNow {
			[InlineCode("{$System.Script}.utcNow()")]
			get {
				return null;
			}
		}

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

		[InlineCode("{$System.Script}.localeFormatDate({this}, {format})")]
		public string LocaleFormat(string format) {
			return null;
		}

		[InlineCode("new Date(Date.parse({value}))")]
		public static JsDate Parse(string value) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDate({value}, {format})")]
		public static JsDate ParseExact(string value, string format) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDate({value}, {format}, {culture})")]
		public static JsDate ParseExact(string value, string format, CultureInfo culture) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDateUTC({value}, {format})")]
		public static JsDate ParseExactUtc(string value, string format) {
			return null;
		}

		[InlineCode("{$System.Script}.parseExactDateUTC({value}, {format}, {culture})")]
		public static JsDate ParseExactUtc(string value, string format, CultureInfo culture) {
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
		public static JsDate FromUtc(int year, int month, int day) {
			return null;
		}

		[InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}))")]
		public static JsDate FromUtc(int year, int month, int day, int hours) {
			return null;
		}

		[InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}))")]
		public static JsDate FromUtc(int year, int month, int day, int hours, int minutes) {
			return null;
		}

		[InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}))")]
		public static JsDate FromUtc(int year, int month, int day, int hours, int minutes, int seconds) {
			return null;
		}

		[InlineCode("new Date(Date.UTC({year}, {month}, {day}, {hours}, {minutes}, {seconds}, {milliseconds}))")]
		public static JsDate FromUtc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
			return null;
		}

		[ScriptAlias("Date.UTC")]
		public static int Utc(int year, int month, int day) {
			return 0;
		}

		[ScriptAlias("Date.UTC")]
		public static int Utc(int year, int month, int day, int hours) {
			return 0;
		}

		[ScriptAlias("Date.UTC")]
		public static int Utc(int year, int month, int day, int hours, int minutes) {
			return 0;
		}

		[ScriptAlias("Date.UTC")]
		public static int Utc(int year, int month, int day, int hours, int minutes, int seconds) {
			return 0;
		}

		[ScriptAlias("Date.UTC")]
		public static int Utc(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
			return 0;
		}

		// NOTE: There is no + operator since in JavaScript that returns the
		//       concatenation of the date strings, which is pretty much useless.

		/// <summary>
		/// Returns the difference in milliseconds between two dates.
		/// </summary>
		[IntrinsicOperator]
		public static int operator -(JsDate a, JsDate b) {
			return 0;
		}

		[InlineCode("new {$System.TimeSpan}(({this} - {value}) * 10000)")]
		public TimeSpan Subtract(JsDate value) {
			return default(TimeSpan);
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator ==(JsDate a, JsDate b) {
			return false;
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator !=(JsDate a, JsDate b) {
			return false;
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[IntrinsicOperator]
		public static bool operator <(JsDate a, JsDate b) {
			return false;
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[IntrinsicOperator]
		public static bool operator >(JsDate a, JsDate b) {
			return false;
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[IntrinsicOperator]
		public static bool operator <=(JsDate a, JsDate b) {
			return false;
		}

		/// <summary>
		/// Compares two dates
		/// </summary>
		[IntrinsicOperator]
		public static bool operator >=(JsDate a, JsDate b) {
			return false;
		}


		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(JsDate other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(JsDate other) {
			return false;
		}
	}
}
