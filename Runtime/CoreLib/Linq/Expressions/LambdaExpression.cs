using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public abstract class LambdaExpression : Expression {
		public ReadOnlyCollection<ParameterExpression> Parameters { get; private set; }
		public string Name { get; private set; }
		public Expression Body { get; private set; }
		public Expression ReturnType { get; private set; }
		public bool TailCall { get; private set; }

		internal LambdaExpression() {}
	}
}