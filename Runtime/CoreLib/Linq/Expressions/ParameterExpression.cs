using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class ParameterExpression : Expression {
		public string Name { get; private set; }
		public bool IsByRef { get; private set; }

		internal ParameterExpression() {}
	}
}