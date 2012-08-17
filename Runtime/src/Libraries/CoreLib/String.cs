// String.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace System {

    /// <summary>
    /// Equivalent to the String type in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported(IsRealType = true)]
    public sealed class String {
		[ScriptName("")]
		public String() {}

		[ScriptName("")]
		public String(String other) {}

		[InlineCode("{$System.String}.fromChar({$System.String}.fromCharCode({ch}), {count})")]
		public String(char ch, int count) {}

        /// <summary>
        /// An empty zero-length string.
        /// </summary>
        [PreserveCase]
        public static readonly String Empty = "";

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

        public static int Compare(string s1, string s2) {
            return 0;
        }

        public static int Compare(string s1, string s2, bool ignoreCase) {
            return 0;
        }

        public int CompareTo(string s) {
            return 0;
        }

        public int CompareTo(string s, bool ignoreCase) {
            return 0;
        }

        public static string Concat(string s1, string s2) {
            return null;
        }

        public static string Concat(string s1, string s2, string s3) {
            return null;
        }

        public static string Concat(string s1, string s2, string s3, string s4) {
            return null;
        }

        /// <summary>
        /// Concatenates a set of individual strings into a single string.
        /// </summary>
        /// <param name="strings">The sequence of strings</param>
        /// <returns>The concatenated string.</returns>
        [ExpandParams]
        public static string Concat(params string[] strings) {
            return null;
        }

        public static string Concat(object o1, object o2) {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string Concat(object o1, object o2, object o3) {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string Concat(object o1, object o2, object o3, object o4) {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [ExpandParams]
        public static string Concat(params object[] o) {
            return null;
        }

        /// <summary>
        /// Returns the unencoded version of a complete encoded URI.
        /// </summary>
        /// <returns>The unencoded string.</returns>
        [ScriptName("decodeURI")]
        public string DecodeUri() {
            return null;
        }

        /// <summary>
        /// Returns the unencoded version of a single part or component of an encoded URI.
        /// </summary>
        /// <returns>The unencoded string.</returns>
        [ScriptName("decodeURIComponent")]
        public string DecodeUriComponent() {
            return null;
        }

        /// <summary>
        /// Encodes the complete URI.
        /// </summary>
        /// <returns>The encoded string.</returns>
        [ScriptName("encodeURI")]
        public string EncodeUri() {
            return null;
        }

        /// <summary>
        /// Encodes a single part or component of a URI.
        /// </summary>
        /// <returns>The encoded string.</returns>
        [ScriptName("encodeURIComponent")]
        public string EncodeUriComponent() {
            return null;
        }

        /// <summary>
        /// Determines if the string ends with the specified character.
        /// </summary>
        /// <param name="ch">The character to test for.</param>
        /// <returns>true if the string ends with the character; false otherwise.</returns>
        [InlineCode("{this}.endsWith({$System.String}.fromCharCode({ch}))")]
		public bool EndsWith(char ch) {
            return false;
        }

        /// <summary>
        /// Determines if the string ends with the specified substring or suffix.
        /// </summary>
        /// <param name="suffix">The string to test for.</param>
        /// <returns>true if the string ends with the suffix; false otherwise.</returns>
        public bool EndsWith(string suffix) {
            return false;
        }

        /// <summary>
        /// Determines if the strings are equal.
        /// </summary>
        /// <returns>true if the string s1 = s2; false otherwise.</returns>
        public static bool Equals(string s1, string s2, bool ignoreCase) {
            return false;
        }

        /// <summary>
        /// Encodes a string by replacing punctuation, spaces etc. with their escaped equivalents.
        /// </summary>
        /// <returns>The escaped string.</returns>
        public string Escape() {
            return null;
        }

        [ExpandParams]
		public static string Format(string format, params object[] values) {
            return null;
        }

        [ExpandParams]
		public static string FromCharCode(params char[] charCode) {
            return null;
        }

        public string HtmlDecode() {
            return null;
        }

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

        public int IndexOfAny(params char[] ch) {
            return 0;
        }

        public int IndexOfAny(char[] ch, int startIndex) {
            return 0;
        }

        public int IndexOfAny(char[] ch, int startIndex, int count) {
            return 0;
        }

        public string Insert(int index, string value) {
            return null;
        }

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

        [InlineCode("{this}.lastIndexOf({$System.String}.fromCharCode({ch}), {startIndex})")]
        public int LastIndexOf(char ch, int startIndex) {
            return 0;
        }

        public int LastIndexOfAny(params char[] ch) {
            return 0;
        }

        public int LastIndexOfAny(char[] ch, int startIndex) {
            return 0;
        }

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

        public string PadLeft(int totalWidth) {
            return null;
        }

        public string PadLeft(int totalWidth, char ch) {
            return null;
        }

        public string PadRight(int totalWidth) {
            return null;
        }

        public string PadRight(int totalWidth, char ch) {
            return null;
        }

        public string Remove(int index) {
            return null;
        }

        public string Remove(int index, int count) {
            return null;
        }

        [ScriptName("replaceAll")]
		public string Replace(string oldText, string replaceText) {
            return null;
        }

        [ScriptName("replace")]
        public string ReplaceFirst(string oldText, string replaceText) {
            return null;
        }

        [ScriptName("replace")]
        public string ReplaceRegex(Regex regex, string replaceText) {
            return null;
        }

        [ScriptName("replace")]
        public string ReplaceRegex(Regex regex, StringReplaceCallback callback) {
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

        [InlineCode("{this}.startsWith({$System.String}.fromCharCode({ch}))")]
        public bool StartsWith(char ch) {
            return false;
        }

        public bool StartsWith(string prefix) {
            return false;
        }

        public string Substr(int startIndex) {
            return null;
        }

        public string Substr(int startIndex, int length) {
            return null;
        }

        public string Substring(int startIndex, int endIndex) {
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

        public string TrimStart() {
            return null;
        }

        public string TrimEnd() {
            return null;
        }

        /// <summary>
        /// Decodes a string by replacing escaped parts with their equivalent textual representation.
        /// </summary>
        /// <returns>The unescaped string.</returns>
        public string Unescape() {
            return null;
        }

        /// <internalonly />
        [IntrinsicOperator]
        public static bool operator ==(string s1, string s2) {
            return false;
        }

        /// <internalonly />
        [IntrinsicOperator]
        public static bool operator !=(string s1, string s2) {
            return false;
        }
    }
}
