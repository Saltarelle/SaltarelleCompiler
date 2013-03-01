using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 55"), Serializable]
	public sealed class IndexExpression : Expression {
		[ScriptName("obj")]
		public Expression Object { get; private set; }
		public PropertyInfo Indexer { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		internal IndexExpression() {}
	}
}