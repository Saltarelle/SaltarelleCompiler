using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class BlockExpression : Expression {
		public ReadOnlyCollection<Expression> Expressions { get; private set; }
		public ReadOnlyCollection<ParameterExpression> Variables { get; private set; }
		public Expression Result { get; private set; }

		public BlockExpression Update(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) { return null; }

		internal BlockExpression() {}
	}
}

namespace System.Collections.ObjectModel {
}
