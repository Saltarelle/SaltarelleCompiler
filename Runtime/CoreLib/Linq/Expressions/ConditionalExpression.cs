using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 8"), Serializable]
	public sealed class ConditionalExpression : Expression {
		public Expression Test { get; private set; }
		public Expression IfTrue { get; private set; }
		public Expression IfFalse { get; private set; }

		internal ConditionalExpression() {}
	}
}