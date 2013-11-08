using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported(TypeCheckCode = "{this}.btype === 0"), Serializable]
	public sealed class MemberAssignment : MemberBinding {
		public Expression Expression { get; private set; }

		internal MemberAssignment() {}
	}
}