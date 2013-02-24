using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class NewExpression : Expression {
		public ConstructorInfo Constructor { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }
		public ReadOnlyCollection<MemberInfo> Members { get; private set; }

		public NewExpression Update(IEnumerable<Expression> arguments) { return null; }

		internal NewExpression() {}
	}
}