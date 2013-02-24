using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class GotoExpression : Expression {
		public Expression Value { get; private set; }
		public LabelTarget Target { get; private set; }
		public GotoExpressionKind Kind { get; private set; }

		public GotoExpression Update(LabelTarget target, Expression value) { return null; }

		internal GotoExpression() {}
	}
}