using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MethodCallExpression : Expression {
		public MethodInfo Method { get; private set; }
		public Expression Object { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		public MethodCallExpression Update(Expression @object, IEnumerable<Expression> arguments) { return null; }

		internal MethodCallExpression() {}
	}
}