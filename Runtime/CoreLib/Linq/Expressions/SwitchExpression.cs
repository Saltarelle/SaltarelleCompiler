using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 59"), Serializable]
	public sealed class SwitchExpression : Expression {
		public Expression SwitchValue { get; private set; }
		public ReadOnlyCollection<SwitchCase> Cases { get; private set; }
		public Expression DefaultBody { get; private set; }
		public MethodInfo Comparison { get; private set; }

		internal SwitchExpression() {}
	}
}