using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class TryExpression : Expression {
		public Expression Body { get; private set; }
		public ReadOnlyCollection<CatchBlock> Handlers { get; private set; }
		public Expression Finally { get; private set; }
		public Expression Fault { get; private set; }

		public TryExpression Update(Expression body, IEnumerable<CatchBlock> handlers, Expression @finally, Expression fault) { return null; }

		internal TryExpression() {}
	}
}