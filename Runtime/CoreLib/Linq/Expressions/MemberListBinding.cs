using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MemberListBinding : MemberBinding {
		public ReadOnlyCollection<ElementInit> Initializers { get; private set; }

		public MemberListBinding Update(IEnumerable<ElementInit> initializers) { return null; }

		internal MemberListBinding() {}
	}
}