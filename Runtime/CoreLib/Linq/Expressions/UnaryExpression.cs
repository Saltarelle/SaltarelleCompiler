using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class UnaryExpression : Expression {
		public Expression Operand { get; private set; }
		public MethodInfo Method { get; private set; }
		public bool IsLifted { get; private set; }
		public bool IsLiftedToNull { get; private set; }

		public UnaryExpression Update(Expression operand) { return null; }

		internal UnaryExpression() {}
	}
}