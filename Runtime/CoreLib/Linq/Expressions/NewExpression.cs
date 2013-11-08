using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 31"), Serializable]
	public sealed class NewExpression : Expression {
		public ConstructorInfo Constructor { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }
		public ReadOnlyCollection<MemberInfo> Members { get; private set; }

		internal NewExpression() {}
	}
}