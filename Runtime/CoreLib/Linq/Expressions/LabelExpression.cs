using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 56"), Serializable]
	public sealed class LabelExpression : Expression {
		public Expression DefaultValue { get; private set; }
		public LabelTarget Target { get; private set; }

		internal LabelExpression() {}
	}
}