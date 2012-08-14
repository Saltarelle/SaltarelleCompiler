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
		public string Input { get; set; }

		[IntrinsicProperty]
		public string this[int index] { get { return null; } set {} }
	}
}