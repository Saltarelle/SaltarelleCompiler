using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 9"), Serializable]
	public sealed class ConstantExpression : Expression {
		public object Value { get; private set; }

		internal ConstantExpression() {}
	}
}