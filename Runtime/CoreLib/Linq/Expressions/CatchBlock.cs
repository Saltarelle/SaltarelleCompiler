using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class CatchBlock {
		public ParameterExpression Variable { get; private set; }
		public Type Test { get; private set; }
		public Expression Body { get; private set; }
		public Expression Filter { get; private set; }

		public CatchBlock Update(ParameterExpression variable, Expression filter, Expression body) { return null; }

		internal CatchBlock() {}
	}
}