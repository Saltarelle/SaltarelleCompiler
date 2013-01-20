using System;
using QUnit;
using System.Text.RegularExpressions;

namespace CoreLib.TestScript {
	[TestFixture]
	public class StringTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(string).FullName, "String");
			Assert.IsTrue(typeof(string).IsClass);
			Assert.IsTrue(typeof(IComparable<string>).IsAssignableFrom(typeof(string)));
			Assert.IsTrue(typeof(IEquatable<string>).IsAssignableFrom(typeof(string)));
			object s = "X";
			Assert.IsTrue(s is string);
			Assert.IsTrue(s is IComparable<string>);
			Assert.IsTrue(s is IEquatable<string>);
		}

		[Test]
		public void DefaultConstructorWorks() {
			Assert.AreEqual(new string(), "");
		}

		[Test]
		public void CopyConstructorWorks() {
			Assert.AreEqual(new string("abcd"), "abcd");
		}

		[Test]
		public void CharAndCountConstructorWorks() {
			Assert.AreEqual(new string('x', 5), "xxxxx");
		}

		[Test]
		public void EmptyFieldWorks() {
			Assert.AreEqual(string.Empty, "");
		}

		[Test]
		public void LengthPropertyWorks() {
			Assert.AreEqual("abcd".Length, 4);
		}

		[Test]
		public void CharAtWorks() {
			Assert.AreEqual("abcd".CharAt(2), "c");
		}

		[Test]
		public void CharCodeAtWorks() {
			Assert.AreEqual((int)"abcd".CharCodeAt(2), (int)'c');
		}

		[Test]
		public void CompareToWithIgnoreCaseArgWorks() {
			Assert.IsTrue("abcd".CompareTo("abcd", false) == 0);
			Assert.IsTrue("abcd".CompareTo("abcb", false) > 0);
			Assert.IsTrue("abcd".CompareTo("abce", false) < 0);
			Assert.IsTrue("abcd".CompareTo("ABCD", true) == 0);
			Assert.IsTrue("abcd".CompareTo("ABCB", true) > 0);
			Assert.IsTrue("abcd".CompareTo("ABCE", true) < 0);
		}

		[Test]
		public void CompareWorks() {
			Assert.IsTrue(string.Compare("abcd", "abcd") == 0);
			Assert.IsTrue(string.Compare("abcd", "abcb") > 0);
			Assert.IsTrue(string.Compare("abcd", "abce") < 0);
		}

		[Test]
		public void CompareWithIgnoreCaseArgWorks() {
			Assert.IsTrue(string.Compare("abcd", "abcd", false) == 0);
			Assert.IsTrue(string.Compare("abcd", "abcb", false) > 0);
			Assert.IsTrue(string.Compare("abcd", "abce", false) < 0);
			Assert.IsTrue(string.Compare("abcd", "ABCD", true) == 0);
			Assert.IsTrue(string.Compare("abcd", "ABCB", true) > 0);
			Assert.IsTrue(string.Compare("abcd", "ABCE", true) < 0);
		}

		[Test]
		public void ConcatWorks() {
			Assert.AreEqual(string.Concat("a", "b"), "ab");
			Assert.AreEqual(string.Concat("a", "b", "c"), "abc");
			Assert.AreEqual(string.Concat("a", "b", "c", "d"), "abcd");
			Assert.AreEqual(string.Concat("a", "b", "c", "d", "e"), "abcde");
			Assert.AreEqual(string.Concat("a", "b", "c", "d", "e", "f"), "abcdef");
			Assert.AreEqual(string.Concat("a", "b", "c", "d", "e", "f", "g"), "abcdefg");
			Assert.AreEqual(string.Concat("a", "b", "c", "d", "e", "f", "g", "h"), "abcdefgh");
			Assert.AreEqual(string.Concat("a", "b", "c", "d", "e", "f", "g", "h", "i"), "abcdefghi");
		}

		[Test]
		public void ConcatWithObjectsWorks() {
			Assert.AreEqual(string.Concat(1, 2), "12");
			Assert.AreEqual(string.Concat(1, 2, 3), "123");
			Assert.AreEqual(string.Concat(1, 2, 3, 4), "1234");
			Assert.AreEqual(string.Concat(1, 2, 3, 4, 5), "12345");
			Assert.AreEqual(string.Concat(1, 2, 3, 4, 5, 6), "123456");
			Assert.AreEqual(string.Concat(1, 2, 3, 4, 5, 6, 7), "1234567");
			Assert.AreEqual(string.Concat(1, 2, 3, 4, 5, 6, 7, 8), "12345678");
			Assert.AreEqual(string.Concat(1, 2, 3, 4, 5, 6, 7, 8, 9), "123456789");
		}

		[Test]
		public void DecodeUriWorks() {
			Assert.AreEqual(string.DecodeUri("%20"), " ");
		}

		[Test]
		public void DecodeUriComponentWorks() {
			Assert.AreEqual(string.DecodeUriComponent("%20"), " ");
		}

		[Test]
		public void EncodeUriWorks() {
			Assert.AreEqual(string.EncodeUri(" "), "%20");
		}

		[Test]
		public void EncodeUriComponentWorks() {
			Assert.AreEqual(string.EncodeUriComponent(" "), "%20");
		}

		[Test]
		public void EndsWithCharWorks() {
			Assert.IsTrue("abcd".EndsWith('d'));
			Assert.IsFalse("abcd".EndsWith('e'));
		}

		[Test]
		public void EndsWithStringWorks() {
			Assert.IsTrue("abcd".EndsWith("d"));
			Assert.IsFalse("abcd".EndsWith("e"));
		}

		[Test]
		public void StaticEqualsWorks() {
			Assert.IsTrue(string.Equals("abcd", "abcd", false));
			Assert.IsFalse(string.Equals("abcd", "abce", false));
			Assert.IsFalse(string.Equals("abcd", "ABCD", false));
			Assert.IsTrue(string.Equals("abcd", "abcd", true));
			Assert.IsFalse(string.Equals("abcd", "abce", true));
			Assert.IsTrue(string.Equals("abcd", "ABCD", true));
		}

		[Test]
		public void EscapeWorks() {
			Assert.AreEqual(string.Escape("a .,b"), "a%20.%2Cb");
		}

		[Test]
		public void UnescapeWorks() {
			Assert.AreEqual(string.Unescape("a%20.%2Cb"), "a .,b");
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(string.Format("x"), "x");
			Assert.AreEqual(string.Format("x{0}", "a"), "xa");
			Assert.AreEqual(string.Format("x{0}{1}", "a", "b"), "xab");
			Assert.AreEqual(string.Format("x{0}{1}{2}", "a", "b", "c"), "xabc");
			Assert.AreEqual(string.Format("x{0}{1}{2}{3}", "a", "b", "c", "d"), "xabcd");
		}

		[Test]
		public void FromCharCodeWorks() {
			Assert.AreEqual(string.FromCharCode(), "");
			Assert.AreEqual(string.FromCharCode('a'), "a");
			Assert.AreEqual(string.FromCharCode('a', 'b'), "ab");
			Assert.AreEqual(string.FromCharCode('a', 'b', 'c'), "abc");
		}

		[Test]
		public void HtmlEncodeWorks() {
			Assert.AreEqual("<".HtmlEncode(), "&lt;");
		}

		[Test]
		public void HtmlDecodeWorks() {
			Assert.AreEqual("&lt;".HtmlDecode(), "<");
		}

		[Test]
		public void IndexOfCharWorks() {
			Assert.AreEqual("abc".IndexOf('b'), 1);
			Assert.AreEqual("abc".IndexOf('d'), -1);
		}

		[Test]
		public void IndexOfStringWorks() {
			Assert.AreEqual("abc".IndexOf("bc"), 1);
			Assert.AreEqual("abc".IndexOf("bd"), -1);
		}

		[Test]
		public void IndexOfCharWithStartIndexWorks() {
			Assert.AreEqual("abcabc".IndexOf('b', 3), 4);
			Assert.AreEqual("abcabc".IndexOf('d', 3), -1);
		}

		[Test]
		public void IndexOfStringWithStartIndexWorks() {
			Assert.AreEqual("abcabc".IndexOf("bc", 3), 4);
			Assert.AreEqual("abcabc".IndexOf("bd", 3), -1);
		}

		[Test]
		public void IndexOfAnyWorks() {
			Assert.AreEqual("abcd".IndexOfAny('b'), 1);
			Assert.AreEqual("abcd".IndexOfAny('b', 'x'), 1);
			Assert.AreEqual("abcd".IndexOfAny('b', 'x', 'y'), 1);
			Assert.AreEqual("abcd".IndexOfAny('x', 'y'), -1);
		}

		[Test]
		public void IndexOfAnyWithStartIndexWorks() {
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b' }, 4), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b', 'x'}, 4), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b', 'x', 'y'}, 4), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'x', 'y'}, 4), -1);
		}

		[Test]
		public void IndexOfAnyWithStartIndexAndCountWorks() {
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b' }, 4, 2), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b', 'x'}, 4, 2), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'b', 'x', 'y'}, 4, 2), 5);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'x', 'y'}, 4, 2), -1);
			Assert.AreEqual("abcdabcd".IndexOfAny(new[] { 'c' }, 4, 2), -1);
		}

		[Test]
		public void InsertWorks() {
			Assert.AreEqual("abcd".Insert(2, "xyz"), "abxyzcd");
		}

		[Test]
		public void IsNullOrEmptyWorks() {
			Assert.IsTrue(string.IsNullOrEmpty(null));
			Assert.IsTrue(string.IsNullOrEmpty(""));
			Assert.IsFalse(string.IsNullOrEmpty(" "));
			Assert.IsFalse(string.IsNullOrEmpty("0"));
		}

		[Test]
		public void LastIndexOfCharWorks() {
			Assert.AreEqual("abc".LastIndexOf('b'), 1);
			Assert.AreEqual("abc".LastIndexOf('d'), -1);
		}

		[Test]
		public void LastIndexOfStringWorks() {
			Assert.AreEqual("abc".LastIndexOf("bc"), 1);
			Assert.AreEqual("abc".LastIndexOf("bd"), -1);
		}

		[Test]
		public void LastIndexOfCharWithStartIndexWorks() {
			Assert.AreEqual("abcabc".LastIndexOf('b', 3), 1);
			Assert.AreEqual("abcabc".LastIndexOf('d', 3), -1);
		}

		[Test]
		public void LastIndexOfStringWithStartIndexWorks() {
			Assert.AreEqual("abcabc".LastIndexOf("bc", 3), 1);
			Assert.AreEqual("abcabc".LastIndexOf("bd", 3), -1);
		}

		[Test]
		public void LastIndexOfAnyWorks() {
			Assert.AreEqual("abcd".LastIndexOfAny('b'), 1);
			Assert.AreEqual("abcd".LastIndexOfAny('b', 'x'), 1);
			Assert.AreEqual("abcd".LastIndexOfAny('b', 'x', 'y'), 1);
			Assert.AreEqual("abcd".LastIndexOfAny('x', 'y'), -1);
		}

		[Test]
		public void LastIndexOfAnyWithStartIndexWorks() {
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b' }, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b', 'x'}, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b', 'x', 'y'}, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'x', 'y'}, 4), -1);
		}

		[Test]
		public void LastIndexOfAnyWithStartIndexAndCountWorks() {
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b' }, 4, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b', 'x'}, 4, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b', 'x', 'y'}, 4, 4), 1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'x', 'y'}, 4, 4), -1);
			Assert.AreEqual("abcdabcd".LastIndexOfAny(new[] { 'b' }, 4, 2), -1);
		}

		[Test]
		public void LocaleCompareWorks() {
			Assert.IsTrue("abcd".LocaleCompare("abcd") == 0);
			Assert.IsTrue("abcd".LocaleCompare("abcb") > 0);
			Assert.IsTrue("abcd".LocaleCompare("abce") < 0);
		}

		/* Seems to be a bug in HtmlUnit, 'x'.toLocaleString() returns [object String]'. Test passes in Chrome.
		Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(string.LocaleFormat("x"), "x");
			Assert.AreEqual(string.LocaleFormat("x{0}", "a"), "xa");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}", "a", "b"), "xab");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}{2}", "a", "b", "c"), "xabc");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}{2}{3}", "a", "b", "c", "d"), "xabcd");
		}*/

		[Test]
		public void MatchWorks() {
			var result = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Match(new Regex("[A-E]", "gi"));
			Assert.AreEqual(result, new[] { "A", "B", "C", "D", "E", "a", "b", "c", "d", "e" });
		}

		[Test]
		public void PadLeftWorks() {
			Assert.AreEqual("abc".PadLeft(5), "  abc");
		}

		[Test]
		public void PadLeftWithCharWorks() {
			Assert.AreEqual("abc".PadLeft(5, '0'), "00abc");
		}

		[Test]
		public void PadRightWorks() {
			Assert.AreEqual("abc".PadRight(5), "abc  ");
		}

		[Test]
		public void PadRightWithCharWorks() {
			Assert.AreEqual("abc".PadRight(5, '0'), "abc00");
		}

		[Test]
		public void RemoveWorks() {
			Assert.AreEqual("abcde".Remove(2), "ab");
		}

		[Test]
		public void RemoveWithCountWorks() {
			Assert.AreEqual("abcde".Remove(2, 2), "abe");
		}

		[Test]
		public void ReplaceWorks() {
			Assert.AreEqual("abcabcabc".Replace("a", "x"), "xbcxbcxbc");
		}

		[Test]
		public void ReplaceFirstWorks() {
			Assert.AreEqual("abcabcabc".ReplaceFirst("a", "x"), "xbcabcabc");
		}

		[Test]
		public void ReplaceRegexWithReplaceTextWorks() {
			Assert.AreEqual("abcabcabc".Replace(new Regex("a|b", "g"), "x"), "xxcxxcxxc");
		}

		[Test]
		public void ReplaceRegexWithReplaceCallbackWorks() {
			Assert.AreEqual("abcabcabc".Replace(new Regex("a|b", "g"), s => s == "a" ? "x" : "y"), "xycxycxyc");
		}

		[Test]
		public void SearchWorks() {
			Assert.AreEqual("abcabcabc".Search(new Regex("ca")), 2);
			Assert.AreEqual("abcabcabc".Search(new Regex("x")), -1);
		}

		[Test]
		public void SplitWithStringWorks() {
			Assert.AreEqual("abcabcabc".Split("b"), new[] { "a", "ca", "ca", "c" });
		}

		[Test]
		public void SplitWithCharWorks() {
			Assert.AreEqual("abcabcabc".Split('b'), new[] { "a", "ca", "ca", "c" });
		}

		[Test]
		public void SplitWithStringAndLimitWorks() {
			Assert.AreEqual("abcabcabc".Split("b", 2), new[] { "a", "ca" });
		}

		[Test]
		public void SplitWithCharAndLimitWorks() {
			Assert.AreEqual("abcabcabc".Split('b', 2), new[] { "a", "ca" });
		}

		[Test]
		public void SplitWithRegexWorks() {
			Assert.AreEqual("abcaxcaxc".Split(new Regex("b|x", "g")), new[] { "a", "ca", "ca", "c" });
		}

		[Test]
		public void SplitWithRegexAndLimitWorks() {
			Assert.AreEqual("abcaxcaxc".Split(new Regex("b|x", "g"), 2), new[] { "a", "ca" });
		}

		[Test]
		public void StartsWithCharWorks() {
			Assert.IsTrue("abc".StartsWith('a'));
			Assert.IsFalse("abc".StartsWith('b'));
		}

		[Test]
		public void StartsWithStringWorks() {
			Assert.IsTrue("abc".StartsWith("ab"));
			Assert.IsFalse("abc".StartsWith("bc"));
		}

		[Test]
		public void SubstrWorks() {
			Assert.AreEqual("abcde".Substr(2), "cde");
		}

		[Test]
		public void SubstrWithLengthWorks() {
			Assert.AreEqual("abcde".Substr(2, 2), "cd");
		}

		[Test]
		public void SubstringWorks() {
			Assert.AreEqual("abcde".Substring(2), "cde");
		}

		[Test]
		public void SubstringWithEndIndexWorks() {
			Assert.AreEqual("abcde".Substring(2, 4), "cd");
		}

		[Test]
		public void ToLocaleLowerCaseWorks() {
			Assert.AreEqual("ABcd".ToLocaleLowerCase(), "abcd");
		}

		[Test]
		public void ToLocaleUpperCaseWorks() {
			Assert.AreEqual("ABcd".ToLocaleUpperCase(), "ABCD");
		}

		[Test]
		public void ToLowerCaseWorks() {
			Assert.AreEqual("ABcd".ToLowerCase(), "abcd");
		}

		[Test]
		public void ToUpperCaseWorks() {
			Assert.AreEqual("ABcd".ToUpperCase(), "ABCD");
		}

		[Test]
		public void ToLowerWorks() {
			Assert.AreEqual("ABcd".ToLower(), "abcd");
		}

		[Test]
		public void ToUpperWorks() {
			Assert.AreEqual("ABcd".ToUpper(), "ABCD");
		}

		[Test]
		public void TrimWorks() {
			Assert.AreEqual("  abc  ".Trim(), "abc");
		}

		[Test]
		public void TrimStartWorks() {
			Assert.AreEqual("  abc  ".TrimStart(), "abc  ");
		}

		[Test]
		public void TrimEndWorks() {
			Assert.AreEqual("  abc  ".TrimEnd(), "  abc");
		}

		[Test]
		public void StringEqualityWorks() {
			string s1 = "abc", s2 = null, s3 = null;
			Assert.IsTrue(s1 == "abc");
			Assert.IsFalse(s1 == "aBc");
			Assert.IsFalse(s1 == s2);
			Assert.IsTrue(s2 == s3);
		}

		[Test]
		public void StringInequalityWorks() {
			string s1 = "abc", s2 = null, s3 = null;
			Assert.IsFalse(s1 != "abc");
			Assert.IsTrue(s1 != "aBc");
			Assert.IsTrue(s1 != s2);
			Assert.IsFalse(s2 != s3);
		}

		[Test]
		public void StringIndexingWorks() {
			var s = "abcd";
			Assert.AreEqual((int)s[0], (int)'a');
			Assert.AreEqual((int)s[1], (int)'b');
			Assert.AreEqual((int)s[2], (int)'c');
			Assert.AreEqual((int)s[3], (int)'d');
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   ("a".GetHashCode(), "a".GetHashCode());
			Assert.AreEqual   ("b".GetHashCode(), "b".GetHashCode());
			Assert.AreNotEqual("a".GetHashCode(), "b".GetHashCode());
			Assert.AreNotEqual("a".GetHashCode(), "ab".GetHashCode());
			Assert.IsTrue((long)"abcdefghijklmnopq".GetHashCode() < 0xffffffffL);
		}

		[Test]
		public void InstanceEqualsWorks() {
			Assert.IsTrue( "a".Equals((object)"a"));
			Assert.IsFalse("b".Equals((object)"a"));
			Assert.IsFalse("a".Equals((object)"b"));
			Assert.IsTrue( "b".Equals((object)"b"));
			Assert.IsFalse("a".Equals((object)"A"));
			Assert.IsFalse("a".Equals((object)"ab"));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue( "a".Equals("a"));
			Assert.IsFalse("b".Equals("a"));
			Assert.IsFalse("a".Equals("b"));
			Assert.IsTrue( "b".Equals("b"));
			Assert.IsFalse("a".Equals("A"));
			Assert.IsFalse("a".Equals("ab"));

			Assert.IsTrue( ((IEquatable<string>)"a").Equals("a"));
			Assert.IsFalse(((IEquatable<string>)"b").Equals("a"));
			Assert.IsFalse(((IEquatable<string>)"a").Equals("b"));
			Assert.IsTrue( ((IEquatable<string>)"b").Equals("b"));
			Assert.IsFalse(((IEquatable<string>)"a").Equals("A"));
			Assert.IsFalse(((IEquatable<string>)"a").Equals("ab"));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue("abcd".CompareTo("abcd") == 0);
			Assert.IsTrue("abcd".CompareTo("abcD") != 0);
			Assert.IsTrue("abcd".CompareTo("abcb") > 0);
			Assert.IsTrue("abcd".CompareTo("abce") < 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<string>)"abcd").CompareTo("abcd") == 0);
			Assert.IsTrue(((IComparable<string>)"abcd").CompareTo("abcD") != 0);
			Assert.IsTrue(((IComparable<string>)"abcd").CompareTo("abcb") > 0);
			Assert.IsTrue(((IComparable<string>)"abcd").CompareTo("abce") < 0);
		}
	}
}
