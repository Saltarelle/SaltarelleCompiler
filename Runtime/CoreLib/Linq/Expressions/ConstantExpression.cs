using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class ConstantExpression : Expression {
		public object Value { get; private set; }

		internal ConstantExpression() {}
	}
}