using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions {
	/// <summary>
	/// This class represents a match returned by the <see cref="Regex.Exec"/> method.
	/// </summary>
	[Imported]
	public sealed class RegexMatch {
		[IntrinsicProperty]
		public int Index { get; set; }

		[IntrinsicProperty]
		public int Length { get; set; }

		[IntrinsicProperty]
		public string Input { get; set; }

		[IntrinsicProperty]
		public string this[int index] { get { return null; } set {} }

		[ScriptSkip]
		public static implicit operator string[](RegexMatch rm) {
			return null;
		}

		[ScriptSkip]
		public static explicit operator RegexMatch(string[] a) {
			return null;
		}
	}
}