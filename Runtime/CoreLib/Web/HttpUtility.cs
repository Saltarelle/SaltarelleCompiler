using System.Runtime.CompilerServices;

namespace System.Web {
	[Imported, ScriptName("Object")]
	public static class HttpUtility {
		[InlineCode("{$System.Script}.htmlEncode({s})")]
		public static string HtmlEncode(string s) {
			return null;
		}

		[InlineCode("{$System.Script}.htmlDecode({s})")]
		public static string HtmlDecode(string s) {
			return null;
		}

		[InlineCode("{$System.Script}.htmlEncode({s})")]
		public static string HtmlAttributeEncode(string s) {
			return null;
		}

		[InlineCode("encodeURIComponent({s}).replace(/%20/g, '+')")]
		public static string UrlEncode(string s) {
			return null;
		}

		[InlineCode("encodeURI({s})")]
		public static string UrlPathEncode(string s) {
			return null;
		}

		[InlineCode("decodeURI({s}.replace('+', ' '))")]
		public static string UrlDecode(string s) {
			return null;
		}

		[InlineCode("{$System.Script}.jsEncode({s})")]
		public static string JavaScriptStringEncode(string s) {
			return null;
		}

		[InlineCode("{$System.Script}.jsEncode({s}, {addDoubleQuotes})")]
		public static string JavaScriptStringEncode(string s, bool addDoubleQuotes) {
			return null;
		}
	}
}
