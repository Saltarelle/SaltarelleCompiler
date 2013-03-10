using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.ntype === 24"), Serializable]
	public sealed class MemberInitExpression : Expression {
		public NewExpression NewExpression { get; private set; }
		public ReadOnlyCollection<MemberBinding> Bindings { get; private set; }

		internal MemberInitExpression() {}
	}
}