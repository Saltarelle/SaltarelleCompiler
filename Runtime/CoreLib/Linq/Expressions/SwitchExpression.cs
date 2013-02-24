using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class SwitchExpression : Expression {
		public Expression SwitchValue { get; private set; }
		public ReadOnlyCollection<SwitchCase> Cases { get; private set; }
		public Expression DefaultBody { get; private set; }
		public MethodInfo Comparison { get; private set; }

		public SwitchExpression Update(Expression switchValue, IEnumerable<SwitchCase> cases, Expression defaultBody) { return null; }

		internal SwitchExpression() {}
	}
}