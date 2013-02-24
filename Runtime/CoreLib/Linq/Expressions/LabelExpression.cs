using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class LabelExpression : Expression {
		public LabelTarget Target { get; private set; }
		public Expression DefaultValue { get; private set; }

		public LabelExpression Update(LabelTarget target, Expression defaultValue) { return null; }

		internal LabelExpression() {}
	}
}