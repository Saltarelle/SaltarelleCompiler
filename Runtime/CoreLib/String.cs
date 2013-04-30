// String.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace System {

	/// <summary>
	/// Equivalent to the String type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	public sealed class String : IComparable<String>, IEquatable<String> {
		[ScriptName("")]
		public String() {}

		[ScriptName("")]
		public String(String other) {}

		[InlineCode("{$System.Script}.stringFromChar({$System.String}.fromCharCode({ch}), {count})")]
		public String(char ch, int count) {}

		[InlineCode("{$System.String}.fromCharCode.apply(null, {value})")]
		public String(char[] value) {}

		[InlineCode("{$System.String}.fromCharCode.apply(null, {value}.slice({startIndex}, {startIndex} + {length}))")]
		public String(char[] value, int startIndex, int length) {}

		[IndexerName("Chars")]
		public char this[int index] { [InlineCode("{this}.charCodeAt({index})")] get { return '\0'; } }

		[NonScriptable]
		public IEnumerator<char> GetEnumerator() { return null; }

		/// <summary>
		/// An empty zero-length string.
		/// </summary>
		[InlineConstant]
		public const String Empty = "";

		/// <summary>
		/// The number of characters in the string.
		/// </summary>
		[IntrinsicProperty]
		public int Length {
			get {
				return 0;
			}
		}

		/// <summary>
		/// Retrieves the character at the specified position.
		/// </summary>
		/// <param name="index">The specified 0-based position.</param>
		/// <returns>The character within the string.</returns>
		public string CharAt(int index) {
			return null;
		}

		/// <summary>
		/// Retrieves the character code of the character at the specified position.
		/// </summary>
		/// <param name="index">The specified 0-based position.</param>
		/// <returns>The character code of the character within the string.</returns>
		public char CharCodeAt(int index) {
			return '\0';
		}

		[InlineCode("{$System.Script}.compareStrings({s1}, {s2})")]
		public static int Compare(string s1, string s2) {
			return 0;
		}

		[InlineCode("{$System.Script}.compareStrings({s1}, {s2}, {ignoreCase})")]
		public static int Compare(string s1, string s2, bool ignoreCase) {
			return 0;
		}

		[InlineCode("{$System.Script}.compareStrings({this}, {s}, {ignoreCase})")]
		public int CompareTo(string s, bool ignoreCase) {
			return 0;
		}

		[InlineCode("[{s1}, {s2}].join('')")]
		public static string Concat(string s1, string s2) {
			return null;
		}

		[InlineCode("[{s1}, {s2}, {s3}].join('')")]
		public static string Concat(string s1, string s2, string s3) {
			return null;
		}

		[InlineCode("[{s1}, {s2}, {s3}, {s4}].join('')")]
		public static string Concat(string s1, string s2, string s3, string s4) {
			return null;
		}

		/// <summary>
		/// Concatenates a set of individual strings into a single string.
		/// </summary>
		/// <param name="strings">The sequence of strings</param>
		/// <returns>The concatenated string.</returns>
		[InlineCode("{strings}.join('')")]
		public static string Concat(params string[] strings) {
			return null;
		}

		[InlineCode("[{o1}, {o2}].join('')")]
		public static string Concat(object o1, object o2) {
			return null;
		}

		[InlineCode("[{o1}, {o2}, {o3}].join('')")]
		public static string Concat(object o1, object o2, object o3) {
			return null;
		}

		[InlineCode("[{o1}, {o2}, {o3}, {o4}].join('')")]
		public static string Concat(object o1, object o2, object o3, object o4) {
			return null;
		}

		[InlineCode("{o}.join('')")]
		public static string Concat(params object[] o) {
			return null;
		}

		[InlineCode("[{o}].join('')")]
		public static string Concat(object o) {
			return null;
		}

		/// <summary>
		/// Returns the unencoded version of a complete encoded URI.
		/// </summary>
		/// <returns>The unencoded string.</returns>
		[ScriptAlias("decodeURI")]
		public static string DecodeUri(string s) {
			return null;
		}

		/// <summary>
		/// Returns the unencoded version of a single part or component of an encoded URI.
		/// </summary>
		/// <returns>The unencoded string.</returns>
		[ScriptAlias("decodeURIComponent")]
		public static string DecodeUriComponent(string s) {
			return null;
		}

		/// <summary>
		/// Encodes the complete URI.
		/// </summary>
		/// <returns>The encoded string.</returns>
		[ScriptAlias("encodeURI")]
		public static string EncodeUri(string s) {
			return null;
		}

		/// <summary>
		/// Encodes a single part or component of a URI.
		/// </summary>
		/// <returns>The encoded string.</returns>
		[ScriptAlias("encodeURIComponent")]
		public static string EncodeUriComponent(string s) {
			return null;
		}

		/// <summary>
		/// Determines if the string ends with the specified character.
		/// </summary>
		/// <param name="ch">The character to test for.</param>
		/// <returns>true if the string ends with the character; false otherwise.</returns>
		[InlineCode("{$System.Script}.endsWithString({this}, {$System.String}.fromCharCode({ch}))")]
		public bool EndsWith(char ch) {
			return false;
		}

		/// <summary>
		/// Determines if the string ends with the specified substring or suffix.
		/// </summary>
		/// <param name="suffix">The string to test for.</param>
		/// <returns>true if the string ends with the suffix; false otherwise.</returns>
		[InlineCode("{$System.Script}.endsWithString({this}, {suffix})")]
		public bool EndsWith(string suffix) {
			return false;
		}

		/// <summary>
		/// Determines if the strings are equal.
		/// </summary>
		/// <returns>true if the string s1 = s2; false otherwise.</returns>
		[InlineCode("{$System.Script}.compareStrings({s1}, {s2}, {ignoreCase}) === 0)")]
		public static bool Equals(string s1, string s2, bool ignoreCase) {
			return false;
		}

		/// <summary>
		/// Encodes a string by replacing punctuation, spaces etc. with their escaped equivalents.
		/// </summary>
		/// <returns>The escaped string.</returns>
		[ScriptAlias("escape") ]
		public static string Escape(string s) {
			return null;
		}

		[InlineCode("{$System.Script}.formatString({format}, {*values})", NonExpandedFormCode = "{$System.Script}.formatString.apply(null, [{format}].concat({values}))")]
		public static string Format(string format, params object[] values) {
			return null;
		}

		[ExpandParams]
		public static string FromCharCode(params char[] charCode) {
			return null;
		}

		[InlineCode("{$System.Script}.htmlDecode({this})")]
		public string HtmlDecode() {
			return null;
		}

		[InlineCode("{$System.Script}.htmlEncode({this})")]
		public string HtmlEncode() {
			return null;
		}

		[InlineCode("{this}.indexOf({$System.String}.fromCharCode({ch}))")]
		public int IndexOf(char ch) {
			return 0;
		}

		public int IndexOf(string subString) {
			return 0;
		}

		[InlineCode("{this}.indexOf({$System.String}.fromCharCode({ch}), {startIndex})")]
		public int IndexOf(char ch, int startIndex) {
			return 0;
		}

		public int IndexOf(string ch, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfString({this}, {$System.String}.fromCharCode({ch}), {startIndex}, {count})")]
		public int IndexOf(char ch, int startIndex, int count)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfString({this}, {ch}, {startIndex}, {count})")]
		public int IndexOf(string ch, int startIndex, int count)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfAnyString({this}, {ch})")]
		public int IndexOfAny(params char[] ch) {
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfAnyString({this}, {ch}, {startIndex})")]
		public int IndexOfAny(char[] ch, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfAnyString({this}, {ch}, {startIndex}, {count})")]
		public int IndexOfAny(char[] ch, int startIndex, int count) {
			return 0;
		}

		[InlineCode("{$System.Script}.insertString({this}, {index}, {value})")]
		public string Insert(int index, string value) {
			return null;
		}

		[InlineCode("{$System.Script}.isNullOrEmptyString({s})")]
		public static bool IsNullOrEmpty(string s) {
			return false;
		}

		[InlineCode("{this}.lastIndexOf({$System.String}.fromCharCode({ch}))")]
		public int LastIndexOf(char ch) {
			return 0;
		}

		public int LastIndexOf(string subString) {
			return 0;
		}

		public int LastIndexOf(string subString, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.lastIndexOfString({this}, {$System.String}.fromCharCode({ch}), {startIndex}, {count})")]
		public int LastIndexOf(char ch, int startIndex, int count)
		{
			return 0;
		}

		[InlineCode("{$System.Script}.lastIndexOfString({this}, {subString}, {startIndex}, {count})")]
		public int LastIndexOf(string subString, int startIndex, int count)
		{
			return 0;
		}

		[InlineCode("{this}.lastIndexOf({$System.String}.fromCharCode({ch}), {startIndex})")]
		public int LastIndexOf(char ch, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.lastIndexOfAnyString({this}, {ch})")]
		public int LastIndexOfAny(params char[] ch) {
			return 0;
		}

		[InlineCode("{$System.Script}.lastIndexOfAnyString({this}, {ch}, {startIndex})")]
		public int LastIndexOfAny(char[] ch, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.lastIndexOfAnyString({this}, {ch}, {startIndex}, {count})")]
		public int LastIndexOfAny(char[] ch, int startIndex, int count) {
			return 0;
		}

		public int LocaleCompare(string string2) {
			return 0;
		}

		[ExpandParams]
		public static string LocaleFormat(string format, params object[] values) {
			return null;
		}

		public string[] Match(Regex regex) {
			return null;
		}

		[InlineCode("{$System.Script}.padLeftString({this}, {totalWidth})")]
		public string PadLeft(int totalWidth) {
			return null;
		}

		[InlineCode("{$System.Script}.padLeftString({this}, {totalWidth}, {ch})")]
		public string PadLeft(int totalWidth, char ch) {
			return null;
		}

		[InlineCode("{$System.Script}.padRightString({this}, {totalWidth})")]
		public string PadRight(int totalWidth) {
			return null;
		}

		[InlineCode("{$System.Script}.padRightString({this}, {totalWidth}, {ch})")]
		public string PadRight(int totalWidth, char ch) {
			return null;
		}

		[InlineCode("{$System.Script}.removeString({this}, {index})")]
		public string Remove(int index) {
			return null;
		}

		[InlineCode("{$System.Script}.removeString({this}, {index}, {count})")]
		public string Remove(int index, int count) {
			return null;
		}

		[InlineCode("{$System.Script}.replaceAllString({this}, {oldText}, {replaceText})")]
		public string Replace(string oldText, string replaceText) {
			return null;
		}

		[InlineCode("{$System.Script}.replaceAllString({this}, {$System.String}.fromCharCode({oldChar}), {$System.String}.fromCharCode({replaceChar}))")]
		public string Replace(char oldChar, char replaceChar)
		{
			return null;
		}

		[ScriptName("replace")]
		public string ReplaceFirst(string oldText, string replaceText) {
			return null;
		}

		[ScriptName("replace")]
		public string Replace(Regex regex, string replaceText) {
			return null;
		}

		[ScriptName("replace")]
		public string Replace(Regex regex, StringReplaceCallback callback) {
			return null;
		}

		public int Search(Regex regex) {
			return 0;
		}

		public string[] Split(string separator) {
			return null;
		}

		[InlineCode("{this}.split({$System.String}.fromCharCode({separator}))")]
		public string[] Split(char separator) {
			return null;
		}

		public string[] Split(string separator, int limit) {
			return null;
		}

		[InlineCode("{$System.Script}.splitWithCharsAndSplitOptions({this}, {separator})")]
		public string[] Split(char[] separator) {
			return null;
		}

		[InlineCode("{$System.Script}.splitWithCharsAndSplitOptions({this}, {separator}, {options})")]
		public string[] Split(char[] separator, StringSplitOptions options) {
			return null;
		}

		[InlineCode("{this}.split({$System.String}.fromCharCode({separator}), {limit})")]
		public string[] Split(char separator, int limit) {
			return null;
		}

		public string[] Split(Regex regex) {
			return null;
		}

		public string[] Split(Regex regex, int limit) {
			return null;
		}

		[InlineCode("{$System.Script}.startsWithString({this}, {$System.String}.fromCharCode({ch}))")]
		public bool StartsWith(char ch) {
			return false;
		}

		[InlineCode("{$System.Script}.startsWithString({this}, {prefix})")]
		public bool StartsWith(string prefix) {
			return false;
		}

		public string Substr(int startIndex) {
			return null;
		}

		public string Substr(int startIndex, int length) {
			return null;
		}

		public string Substring(int startIndex) {
			return null;
		}

		[ScriptName("substr")]
		public string Substring(int startIndex, int length) {
			return null;
		}

		[ScriptName("substring")]
		public string JsSubstring(int startIndex, int end) {
			return null;
		}

		public string ToLocaleLowerCase() {
			return null;
		}

		public string ToLocaleUpperCase() {
			return null;
		}

		public string ToLowerCase() {
			return null;
		}

		[ScriptName("toLowerCase")]
		public string ToLower() {
			return null;
		}

		public string ToUpperCase() {
			return null;
		}

		[ScriptName("toUpperCase")]
		public string ToUpper() {
			return null;
		}

		public string Trim() {
			return null;
		}

		[InlineCode("{$System.Script}.trimString({this}, {values})")]
		public string Trim(params char[] values)
		{
			return null;
		}

		[InlineCode("{$System.Script}.trimStartString({this}, {values})")]
		public string TrimStart(params char[] values)
		{
			return null;
		}

		[InlineCode("{$System.Script}.trimEndString({this}, {values})")]
		public string TrimEnd(params char[] values)
		{
			return null;
		}

		[InlineCode("{$System.Script}.trimStartString({this})")]
		public string TrimStart() {
			return null;
		}

		[InlineCode("{$System.Script}.trimEndString({this})")]
		public string TrimEnd() {
			return null;
		}

		/// <summary>
		/// Decodes a string by replacing escaped parts with their equivalent textual representation.
		/// </summary>
		/// <returns>The unescaped string.</returns>
		[ScriptAlias("unescape")]
		public static string Unescape(string s) {
			return null;
		}

		[IntrinsicOperator]
		public static bool operator ==(string s1, string s2) {
			return false;
		}

		[IntrinsicOperator]
		public static bool operator !=(string s1, string s2) {
			return false;
		}

		[InlineCode("{$System.Script}.compare({this}, {other})")]
		public int CompareTo(string other) {
			return 0;
		}

		[InlineCode("{$System.Script}.equalsT({this}, {other})")]
		public bool Equals(string other) {
			return false;
		}

		[InlineCode("{$System.Script}.equalsT({a}, {b})")]
		public static bool Equals(string a, string b)
		{
			return false;
		}

		[InlineCode("{args}.join({separator})")]
		public static string Join(string separator, params string[] args)
		{
			return null;
		}

		[InlineCode("{args}.join({separator})")]
		public static string Join(string separator, params Object[] args)
		{
			return null;
		}

		[InlineCode("{$System.Script}.arrayFromEnumerable({args}).join({separator})")]
		public static string Join(string separator, IEnumerable<string> args)
		{
			return null;
		}

		[InlineCode("{$System.Script}.arrayFromEnumerable({args}).join({separator})")]
		public static string Join<T>(string separator, IEnumerable<T> args)
		{
			return null;
		}

		[InlineCode("{args}.slice({startIndex}, {startIndex} + {count}).join({separator})")]
		public static string Join(string separator, string[] args, int startIndex, int count)
		{
			return null;
		}

		[InlineCode("({this}.indexOf({value}) !== -1)")]
		public bool Contains(string value)
		{
			return false;
		}
	}
}
