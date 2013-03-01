using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype == 50"), Serializable]
	public abstract class DynamicExpression : Expression {
		[ScriptName("dtype")]
		public DynamicExpressionType DynamicType { get; private set; }
		public Expression Expression { get; private set; }

		internal DynamicExpression() {}
	}
}