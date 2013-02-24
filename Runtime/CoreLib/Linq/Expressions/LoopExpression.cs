using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class LoopExpression : Expression {
		public Expression Body { get; private set; }
		public LabelTarget BreakLabel { get; private set; }
		public LabelTarget ContinueLabel { get; private set; }

		public LoopExpression Update(LabelTarget breakLabel, LabelTarget continueLabel, Expression body) { return null; }

		internal LoopExpression() {}
	}
}