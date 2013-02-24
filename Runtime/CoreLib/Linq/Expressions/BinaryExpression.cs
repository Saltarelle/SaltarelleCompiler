using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class BinaryExpression : Expression {
		public Expression Left { get; private set; }
		public Expression Right { get; private set; }
		public MethodInfo Method { get; private set; }
		public LambdaExpression Conversion { get; private set; }
		public bool IsLifted { get; private set; }
		public bool IsLiftedToNull { get; private set; }

		public BinaryExpression Update(Expression left, LambdaExpression conversion, Expression right) { return null; }

		internal BinaryExpression() {}
	}
}