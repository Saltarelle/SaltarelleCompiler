using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 17"), Serializable]
	public sealed class InvocationExpression : Expression {
		public Expression Expression { get; private set; }
		[ScriptName("args")]
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		internal InvocationExpression() {}
	}
}