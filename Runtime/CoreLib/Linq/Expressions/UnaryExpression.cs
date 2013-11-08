using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "[4,10,11,28,29,30,34,40,44,49,54,60,62,77,78,79,80,82,83,84].indexOf({this}.ntype) >= 0"), Serializable]
	public sealed class UnaryExpression : Expression {
		public Expression Operand { get; private set; }
		public MethodInfo Method { get; private set; }

		internal UnaryExpression() {}
	}
}