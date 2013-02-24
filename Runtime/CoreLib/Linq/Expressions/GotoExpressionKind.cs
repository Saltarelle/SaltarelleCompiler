using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported]
	public enum GotoExpressionKind {
		Goto,
		Return,
		Break,
		Continue,
	}
}