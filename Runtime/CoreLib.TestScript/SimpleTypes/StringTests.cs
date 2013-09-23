using System;
using System.Collections;
using System.Collections.Generic;
using QUnit;
using System.Text.RegularExpressions;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class StringTests {
		class MyFormattable : IFormattable {
			public string ToString(string format) {
				return "Formatted: " + format;
			}
		}

		class MyEnumerable<T> : IEnumerable<T> {
			private readonly T[] _items;

			public MyEnumerable(T[] items) {
				_items = items;
			}

			IEnumerator IEnumerable.GetEnumerator() { return null; }

			public IEnumerator<T> GetEnumerator() {
				return (IEnumerator<T>)(object)_items.GetEnumerator();
			}
		}

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

			var interfaces = typeof(string).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<string>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<string>)));
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
		public void CharArrayConstructorWorks() {
			Assert.AreEqual(new string(new[]{'a', 'b', 'C'}), "abC");
		}

		[Test]
		public void CharArrayWithStartIndexAndLengthConstructorWorks() {
			Assert.AreEqual(new string(new[]{'a', 'b', 'c', 'D'}, 1, 2), "bc");
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
			Assert.AreEqual(string.Concat(1), "1");
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

			Assert.IsTrue(string.Equals("abcd", "abcd"));
			Assert.IsFalse(string.Equals("abcd", "abce"));
			Assert.IsFalse(string.Equals("abcd", "ABCD"));
			Assert.IsTrue(string.Equals("abcd", "abcd"));
			Assert.IsFalse(string.Equals("abcd", "abce"));
			Assert.IsFalse(string.Equals("abcd", "ABCD"));
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

			var arr1 = new object[] { "a" };
			var arr2 = new object[] { "a", "b" };
			var arr3 = new object[] { "a", "b", "c" };
			var arr4 = new object[] { "a", "b", "c", "d" };
			Assert.AreEqual(string.Format("x{0}", arr1), "xa");
			Assert.AreEqual(string.Format("x{0}{1}", arr2), "xab");
			Assert.AreEqual(string.Format("x{0}{1}{2}", arr3), "xabc");
			Assert.AreEqual(string.Format("x{0}{1}{2}{3}", arr4), "xabcd");
		}

		[Test]
		public void FormatWorksWithIFormattable() {
			Assert.AreEqual(string.Format("{0:F2}", 22.0 / 7.0), "3.14");
			Assert.AreEqual(string.Format("{0:FMT}", new MyFormattable()), "Formatted: FMT");
		}

		[Test]
		public void FormatCanUseEscapedBraces() {
			Assert.AreEqual(string.Format("{{0}}"), "{0}");
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
			Assert.AreEqual("a<\"&'>b".HtmlEncode(), "a&lt;&quot;&amp;&#39;&gt;b");
		}

		[Test]
		public void HtmlDecodeWorks() {
			Assert.AreEqual("abcd".HtmlDecode(), "abcd");
			Assert.AreEqual("&lt;abcd".HtmlDecode(), "<abcd");
			Assert.AreEqual("abcd&gt;".HtmlDecode(), "abcd>");
			Assert.AreEqual("a&lt;&quot;&amp;&#39;&gt;b".HtmlDecode(), "a<\"&'>b");
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
		public void IndexOfCharWithStartIndexAndCountWorks() {
			Assert.AreEqual("xxxxxabcxxx".IndexOf('c', 3, 8), 7);
			Assert.AreEqual("xxxxxabcxxx".IndexOf('c', 3, 5), 7);
			Assert.AreEqual("xxxxxabcxxx".IndexOf('c', 3, 4), -1);
		}

		[Test]
		public void IndexOfStringWithStartIndexWorks() {
			Assert.AreEqual("abcabc".IndexOf("bc", 3), 4);
			Assert.AreEqual("abcabc".IndexOf("bd", 3), -1);
		}

		[Test]
		public void IndexOfStringWithStartIndexAndCountWorks() {
			Assert.AreEqual("xxxxxabcxxx".IndexOf("abc", 3, 8), 5);
			Assert.AreEqual("xxxxxabcxxx".IndexOf("abc", 3, 5), 5);
			Assert.AreEqual("xxxxxabcxxx".IndexOf("abc", 3, 4), -1);
		}

		[Test]
		public void IndexOfAnyWorks() {
			Assert.AreEqual("abcd".IndexOfAny(new[]{'b'}), 1);
			Assert.AreEqual("abcd".IndexOfAny(new[]{'b', 'x'}), 1);
			Assert.AreEqual("abcd".IndexOfAny(new[]{'b', 'x', 'y'}), 1);
			Assert.AreEqual("abcd".IndexOfAny(new[]{'x', 'y'}), -1);
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
		public void LastIndexOfCharWithStartIndexAndCountWorks() {
			Assert.AreEqual("abcabc".LastIndexOf('b', 3, 3), 1);
			Assert.AreEqual("abcabc".LastIndexOf('b', 3, 2), -1);
			Assert.AreEqual("abcabc".LastIndexOf('d', 3, 3), -1);
		}

		[Test]
		public void LastIndexOfStringWithStartIndexAndCountWorks() {
			Assert.AreEqual("xbcxxxbc".LastIndexOf("bc", 3, 3), 1);
			Assert.AreEqual("xbcxxxbc".LastIndexOf("bc", 3, 2), -1);
			Assert.AreEqual("xbcxxxbc".LastIndexOf("bd", 3, 3), -1);
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

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(string.LocaleFormat("x"), "x");
			Assert.AreEqual(string.LocaleFormat("x{0}", "a"), "xa");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}", "a", "b"), "xab");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}{2}", "a", "b", "c"), "xabc");
			Assert.AreEqual(string.LocaleFormat("x{0}{1}{2}{3}", "a", "b", "c", "d"), "xabcd");
		}

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
			Assert.AreEqual("abcabcabc".Replace("ab", "x"), "xcxcxc");
		}

		[Test]
		public void ReplaceCharWorks() {
			Assert.AreEqual("abcabcabc".Replace('a', 'x'), "xbcxbcxbc");
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
		public void JsSplitWithStringAndLimitWorks() {
			Assert.AreEqual("abcaxbcabce".JsSplit("bc", 2), new[] { "a", "ax" });
		}

		[Test]
		public void JsSplitWithCharAndLimitWorks() {
			Assert.AreEqual("abcabcabc".JsSplit('b', 2), new[] { "a", "ca" });
		}

		[Test]
		public void SplitWithCharsAndLimitWorks() {
			Assert.AreEqual("abcabcabc".Split(new[] { 'b' }, 2), new[] { "a", "cabcabc" });
		}

		[Test]
		public void SplitWithCharsAndStringSplitOptionsAndLimitWorks() {
			Assert.AreEqual("abxcabcabc".Split(new[] { 'b', 'x' }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "cabcabc" });
		}

		[Test]
		public void SplitWithRegexWorks() {
			Assert.AreEqual("abcaxcaxc".Split(new Regex("b|x", "g")), new[] { "a", "ca", "ca", "c" });
		}

		[Test]
		public void JsSplitWithRegexAndLimitWorks() {
			Assert.AreEqual("abcaxcaxc".JsSplit(new Regex("b|x", "g"), 2), new[] { "a", "ca" });
		}

		[Test]
		public void SomeNetSplitTests() {
			Assert.AreEqual("axybcxzde".Split(new[] { "xy", "xz" }, StringSplitOptions.None), new[] { "a", "bc", "de" });
			Assert.AreEqual("axybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.None), new[] { "a", "bc", "de", "" });
			Assert.AreEqual("xzaxybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.None), new[] { "", "a", "bc", "de", "" });
			Assert.AreEqual("xzaxyxzbcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.None), new[] { "", "a", "", "bc", "de", "" });
			Assert.AreEqual("xzaxyxzxybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.None), new[] { "", "a", "", "", "bc", "de", "" });

			Assert.AreEqual("axybcxzde".Split(new[] { "xy", "xz" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bc", "de" });
			Assert.AreEqual("axybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bc", "de" });
			Assert.AreEqual("xzaxybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bc", "de" });
			Assert.AreEqual("xzaxyxzbcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bc", "de" });
			Assert.AreEqual("xzaxyxzxybcxzdexz".Split(new[] { "xy", "xz" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bc", "de" });

			Assert.AreEqual("axybcxzde".Split(new[] { "xy", "xz" }, 100, StringSplitOptions.None), new[] { "a", "bc", "de" });
			Assert.AreEqual("axybcxzdexz".Split(new[] { "xy", "xz" }, 100, StringSplitOptions.None), new[] { "a", "bc", "de", "" });
			Assert.AreEqual("xzaxybcxzdexz".Split(new[] { "xy", "xz" }, 100, StringSplitOptions.None), new[] { "", "a", "bc", "de", "" });
			Assert.AreEqual("xzaxyxzbcxzdexz".Split(new[] { "xy", "xz" }, 100, StringSplitOptions.None), new[] { "", "a", "", "bc", "de", "" });
			Assert.AreEqual("xzaxyxzxybcxzdexz".Split(new[] { "xy", "xz" }, 100, StringSplitOptions.None), new[] { "", "a", "", "", "bc", "de", "" });

			Assert.AreEqual("axybcxzde".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.None), new[] { "a", "bcxzde" });
			Assert.AreEqual("axybcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.None), new[] { "a", "bcxzdexz" });
			Assert.AreEqual("axyxzbcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.None), new[] { "a", "xzbcxzdexz" });
			Assert.AreEqual("xzaxybcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.None), new[] { "", "axybcxzdexz" });

			Assert.AreEqual("axybcxzde".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bcxzde" });
			Assert.AreEqual("axybcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bcxzdexz" });
			Assert.AreEqual("axyxzbcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bcxzdexz" });
			Assert.AreEqual("xzaxyxzbcxzdexz".Split(new[] { "xy", "xz" }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "bcxzdexz" });
		}

		[Test]
		public void SplitWithCharsWorks() {
			Assert.AreEqual("Lorem Ipsum, dolor[sit amet".Split(new[] { ',', ' ', '[' }), new[] { "Lorem", "Ipsum", "", "dolor", "sit", "amet" });
			Assert.AreEqual("Lorem Ipsum, dolor[sit amet".Split(new[] { ',', ' ', '[' }, StringSplitOptions.None), new[] { "Lorem", "Ipsum", "", "dolor", "sit", "amet" });
			Assert.AreEqual("Lorem Ipsum, dolor[sit amet".Split(new[] { ',', ' ', '[' }, StringSplitOptions.RemoveEmptyEntries), new[] { "Lorem", "Ipsum", "dolor", "sit", "amet" });
		}

		[Test]
		public void SplitWithStringsWorks() {
			Assert.AreEqual("a is b if b is c and c isifis d if d is e".Split(new[] { "is", "if" }, StringSplitOptions.None), new[] { "a ", " b ", " b ", " c and c ", "", "", " d ", " d ", " e" });
			Assert.AreEqual("a is b if b is c and c isifis d if d is e".Split(new[] { "is", "if" }, StringSplitOptions.RemoveEmptyEntries), new[] { "a ", " b ", " b ", " c and c ", " d ", " d ", " e" });
		}

		[Test]
		public void SplitWithStringsAndLimitWorks() {
			Assert.AreEqual("abcbcabcabc".Split(new[] { "bc" }, 2, StringSplitOptions.RemoveEmptyEntries), new[] { "a", "abcabc" });
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
		public void SubstringWithLengthWorks() {
			Assert.AreEqual("abcde".Substring(2, 2), "cd");
		}

		[Test]
		public void JsSubstringWithEndIndexWorks() {
			Assert.AreEqual("abcde".JsSubstring(2, 4), "cd");
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
		public void TrimCharsWorks() {
			Assert.AreEqual(",., aa, aa,... ".Trim(',', '.', ' '), "aa, aa");
		}

		[Test]
		public void TrimStartCharsWorks() {
			Assert.AreEqual(",., aa, aa,... ".TrimStart(',', '.', ' '), "aa, aa,... ");
		}

		[Test]
		public void TrimEndCharsWorks() {
			Assert.AreEqual(",., aa, aa,... ".TrimEnd(',', '.', ' '), ",., aa, aa");
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

		[Test]
		public void JoinWorks() {
			Assert.AreEqual(string.Join(", ", new[] { "a", "ab", "abc", "abcd" }), "a, ab, abc, abcd");
			Assert.AreEqual(string.Join(", ", new[] { "a", "ab", "abc", "abcd" }, 1, 2), "ab, abc");

			IEnumerable<int> intValues = new MyEnumerable<int>(new[] { 1, 5, 6 });
			Assert.AreEqual(String.Join(", ", intValues), "1, 5, 6");
			IEnumerable<string> stringValues = new MyEnumerable<string>(new[] { "a", "ab", "abc", "abcd" });
			Assert.AreEqual(String.Join(", ", stringValues), "a, ab, abc, abcd");

			// TODO: c# makes it False but js false
			Assert.AreEqual(String.Join(", ", new Object[] { "a", 1, "abc", false }), "a, 1, abc, false");// False");
		}

		[Test]
		public void ContainsWorks() {
			string text = "Lorem ipsum dolor sit amet";
			Assert.IsTrue(text.Contains("Lorem"));
			Assert.IsFalse(text.Contains("lorem"));
			Assert.IsTrue(text.Contains(text));
		}

		[Test]
		public void ToCharArrayWorks() {
			string text = "Lorem sit dolor";
			Assert.AreEqual(text.ToCharArray(), new[] {'L', 'o', 'r', 'e', 'm', ' ', 's', 'i', 't', ' ', 'd', 'o', 'l', 'o', 'r'});
		}
	}
}
