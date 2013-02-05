using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Saltarelle.Compiler.JSModel.ExtensionMethods {
	public static class ExtensionMethods {
		public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source) {
			if (source == null)
				return new List<T>().AsReadOnly();
			else if (source is ReadOnlyCollection<T>)
				return (ReadOnlyCollection<T>)source;
			else
				return new List<T>(source).AsReadOnly();
		}

		public static void AddRange<T>(this IList<T> list, IEnumerable<T> items) {
			foreach (var item in items)
				list.Add(item);
		}

		private static readonly Regex _jsIdentifierRegex = new Regex(@"^[_$\p{L}][_$\p{L}0-9]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		public static bool IsValidJavaScriptIdentifier(this string s) {
			return s != null && _jsIdentifierRegex.IsMatch(s);
		}

		public static bool IsValidNestedJavaScriptIdentifier(this string s) {
			return !string.IsNullOrEmpty(s) && s.Split('.').All(IsValidJavaScriptIdentifier);
		}

		public static string EscapeJavascriptStringLiteral(this string s, bool isRegexp = false) {
			var sb = new StringBuilder();
			for (int i = 0; i < s.Length; i++) {
				switch (s[i]) {
					case '\b': sb.Append("\\b"); break;
					case '\f': sb.Append("\\b"); break;
					case '\n': sb.Append("\\n"); break;
					case '\0': sb.Append("\\0"); break;
					case '\r': sb.Append("\\r"); break;
					case '\t': sb.Append("\\t"); break;
					case '\v': sb.Append("\\v"); break;
					case '\'': sb.Append("\\\'"); break;
					case '\\': sb.Append("\\\\"); break;
					case '/':  sb.Append(isRegexp ? "\\/" : "/"); break;
					default:   sb.Append(s[i]); break;
				}
			}
			return sb.ToString();
		}

		public static void ForEach<T>(this IEnumerable<T> seq, Action<T> action) {
			foreach (var el in seq)
				action(el);
		}
	}
}
