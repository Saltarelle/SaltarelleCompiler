using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 23"), Serializable]
	public sealed class MemberExpression : Expression {
		public MemberInfo Member { get; private set; }
		public Expression Expression { get; private set; }

		internal MemberExpression() {}
	}
}