using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class TypeBinaryExpression : Expression {
		public Expression Expression { get; private set; }
		public Type TypeOperand { get; private set; }

		public TypeBinaryExpression Update(Expression expression) { return null; }

		internal TypeBinaryExpression() {}
	}
}