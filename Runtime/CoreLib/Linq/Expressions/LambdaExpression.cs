using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 18"), Serializable]
	public abstract class LambdaExpression : Expression {
		[ScriptName("params")]
		public ReadOnlyCollection<ParameterExpression> Parameters { get; private set; }
		public Expression Body { get; private set; }
		public Expression ReturnType { get; private set; }

		internal LambdaExpression() {}
	}
}