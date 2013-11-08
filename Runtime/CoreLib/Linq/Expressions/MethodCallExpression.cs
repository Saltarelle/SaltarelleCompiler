using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 6"), Serializable]
	public sealed class MethodCallExpression : Expression {
		public MethodInfo Method { get; private set; }
		[ScriptName("obj")]
		public Expression Object { get; private set; }
		[ScriptName("args")]
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		internal MethodCallExpression() {}
	}
}