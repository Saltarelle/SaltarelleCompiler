using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 58"), Serializable]
	public sealed class LoopExpression : Expression {
		public Expression Body { get; private set; }
		public LabelTarget BreakLabel { get; private set; }
		public LabelTarget ContinueLabel { get; private set; }

		internal LoopExpression() {}
	}
}