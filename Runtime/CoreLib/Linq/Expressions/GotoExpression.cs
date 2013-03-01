using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 53"), Serializable]
	public sealed class GotoExpression : Expression {
		public GotoExpressionKind Kind { get; private set; }
		public Expression Value { get; private set; }
		public LabelTarget Target { get; private set; }

		internal GotoExpression() {}
	}
}