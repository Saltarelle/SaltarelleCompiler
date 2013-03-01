using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 18"), Serializable]
	public sealed class Expression<TDelegate> : LambdaExpression {
		public Expression<TDelegate> Update(Expression body, IEnumerable<ParameterExpression> parameters) { return null; }

		internal Expression() {}
	}
}