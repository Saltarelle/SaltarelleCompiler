using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Saltarelle.Compiler.JSModel {
    public static class ExtensionMethods {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source) {
            if (source == null)
                return new List<T>().AsReadOnly();
            else if (source is ReadOnlyCollection<T>)
                return (ReadOnlyCollection<T>)source;
            else
                return new List<T>(source).AsReadOnly();
        }

        private static readonly Regex _jsIdentifierRegex = new Regex("^[_$a-z][_$a-z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool IsValidJavaScriptIdentifier(this string s) {
            return _jsIdentifierRegex.IsMatch(s);
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
