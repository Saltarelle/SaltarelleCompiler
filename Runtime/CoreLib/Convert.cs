using System.Runtime.CompilerServices;

namespace System {
	public static class Convert {
		[InlineCode("{$System.Script}.enc64({inArray})")]
		public static string ToBase64String(byte[] inArray) {
			return null;
		}

		[InlineCode("{$System.Script}.enc64({inArray}, {options})")]
		public static string ToBase64String(byte[] inArray, Base64FormattingOptions options) {
			return null;
		}

		[InlineCode("{$System.Script}.enc64({inArray}.slice({offset}, {offset} + {length}))")]
		public static string ToBase64String(byte[] inArray, int offset, int length) {
			return null;
		}

		[InlineCode("{$System.Script}.enc64({inArray}.slice({offset}, {offset} + {length}), {options})")]
		public static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options) {
			return null;
		}

		[InlineCode("{$System.Script}.dec64({s})")]
		public static byte[] FromBase64String(string s) {
			return null;
		}
	}
}
