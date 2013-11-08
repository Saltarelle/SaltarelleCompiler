using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 38"), Serializable]
	public sealed class ParameterExpression : Expression {
		public string Name { get; private set; }

		internal ParameterExpression() {}
	}
}