using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype == 50 && {this}.dtype === 1"), Serializable]
	public sealed class DynamicInvocationExpression : DynamicExpression {
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		internal DynamicInvocationExpression() {}
	}
}