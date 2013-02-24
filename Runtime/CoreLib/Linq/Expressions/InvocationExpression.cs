using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class InvocationExpression : Expression {
		public Expression Expression { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		public InvocationExpression Update(Expression expression, IEnumerable<Expression> arguments) { return null; }

		internal InvocationExpression() {}
	}
}