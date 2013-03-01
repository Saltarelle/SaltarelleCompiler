using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.btype === 1"), Serializable]
	public sealed class MemberMemberBinding : MemberBinding {
		public ReadOnlyCollection<MemberBinding> Bindings { get; private set; }

		internal MemberMemberBinding() {}
	}
}