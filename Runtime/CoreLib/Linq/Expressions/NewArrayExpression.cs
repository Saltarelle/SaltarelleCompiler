using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 32 || {this}.ntype === 33"), Serializable]
	public sealed class NewArrayExpression : Expression {
		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		internal NewArrayExpression() {}
	}
}