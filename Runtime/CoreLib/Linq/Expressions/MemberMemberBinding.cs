using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MemberMemberBinding : MemberBinding {
		public ReadOnlyCollection<MemberBinding> Bindings { get; private set; }

		public MemberMemberBinding Update(IEnumerable<MemberBinding> bindings) { return null; }

		internal MemberMemberBinding() {}
	}
}