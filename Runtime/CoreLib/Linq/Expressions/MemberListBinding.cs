using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.btype === 2"), Serializable]
	public sealed class MemberListBinding : MemberBinding {
		public ReadOnlyCollection<ElementInit> Initializers { get; private set; }

		internal MemberListBinding() {}
	}
}