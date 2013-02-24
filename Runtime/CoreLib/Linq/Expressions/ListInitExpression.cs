using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class ListInitExpression : Expression {
		public NewExpression NewExpression { get; private set; }
		public ReadOnlyCollection<ElementInit> Initializers { get; private set; }

		public ListInitExpression Update(NewExpression newExpression, IEnumerable<ElementInit> initializers) { return null; }

		internal ListInitExpression() {}
	}
}