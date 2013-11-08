using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 47"), Serializable]
	public sealed class BlockExpression : Expression {
		public ReadOnlyCollection<Expression> Expressions { get; private set; }
		public ReadOnlyCollection<ParameterExpression> Variables { [InlineCode("{this}.variables || []")] get; [InlineCode("X")] private set; }
		public Expression Result { [InlineCode("{this}.expressions[{this}.expressions.length - 1]")] get; [InlineCode("X")] private set; }

		internal BlockExpression() {}
	}
}