using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 61"), Serializable]
	public sealed class TryExpression : Expression {
		public Expression Body { get; private set; }
		public ReadOnlyCollection<CatchBlock> Handlers { get; private set; }
		[ScriptName("finallyExpr")]
		public Expression Finally { get; private set; }
		public Expression Fault { get; private set; }

		internal TryExpression() { }
	}
}