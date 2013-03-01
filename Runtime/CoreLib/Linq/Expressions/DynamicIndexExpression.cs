using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype == 50 && {this}.dtype === 2"), Serializable]
	public sealed class DynamicIndexExpression : DynamicExpression {
		public Expression Argument { get; private set; }

		internal DynamicIndexExpression() {}
	}
}