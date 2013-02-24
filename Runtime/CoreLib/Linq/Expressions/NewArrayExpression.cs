using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class NewArrayExpression : Expression {
		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public NewArrayExpression Update(IEnumerable<Expression> expressions) { return null; }

		internal NewArrayExpression() {}
	}
}