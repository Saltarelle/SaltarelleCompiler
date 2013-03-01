using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 22"), Serializable]
	public sealed class ListInitExpression : Expression {
		public NewExpression NewExpression { get; private set; }
		public ReadOnlyCollection<ElementInit> Initializers { get; private set; }

		internal ListInitExpression() {}
	}
}