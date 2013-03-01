using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype == 50 && {this}.dtype === 0"), Serializable]
	public sealed class DynamicMemberExpression : DynamicExpression {
		public string Member { get; private set; }

		internal DynamicMemberExpression() {}
	}
}