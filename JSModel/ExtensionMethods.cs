using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Saltarelle.Compiler.JSModel {
    public static class ExtensionMethods {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source) {
            if (source == null)
                return new List<T>().AsReadOnly();
            else if (source is ReadOnlyCollection<T>)
                return (ReadOnlyCollection<T>)source;
            else if (source is List<T>)
                return ((List<T>)source).AsReadOnly();
            else
                return new List<T>(source).AsReadOnly();
        }

        private static readonly Regex _jsIdentifierRegex = new Regex("^[_$a-z][_$a-z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool IsValidJavaScriptIdentifier(this string s) {
            return _jsIdentifierRegex.IsMatch(s);
        }
    }
}
