using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MemberExpression : Expression {
		public MemberInfo Member { get; private set; }
		public Expression Expression { get; private set; }

		public MemberExpression Update(Expression expression) { return null; }

		internal MemberExpression() {}
	}
}