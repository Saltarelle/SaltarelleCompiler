using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 45 || {this}.ntype === 81"), Serializable]
	public sealed class TypeBinaryExpression : Expression {
		public Expression Expression { get; private set; }
		public Type TypeOperand { get; private set; }

		internal TypeBinaryExpression() {}
	}
}