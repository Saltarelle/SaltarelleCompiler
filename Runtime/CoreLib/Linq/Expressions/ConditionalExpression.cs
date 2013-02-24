using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class ConditionalExpression : Expression {
		public Expression Test { get; private set; }
		public Expression IfTrue { get; private set; }
		public Expression IfFalse { get; private set; }

		public ConditionalExpression Update(Expression test, Expression ifTrue, Expression ifFalse) { return null; }

		internal ConditionalExpression() {}
	}
}