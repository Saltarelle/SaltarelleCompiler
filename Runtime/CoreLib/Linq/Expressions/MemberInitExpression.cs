using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MemberInitExpression : Expression {
		public NewExpression NewExpression { get; private set; }
		public ReadOnlyCollection<MemberBinding> Bindings { get; private set; }

		public MemberInitExpression Update(NewExpression newExpression, IEnumerable<MemberBinding> bindings) { return null; }

		internal MemberInitExpression() {}
	}
}