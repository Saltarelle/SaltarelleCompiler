using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class MemberAssignment : MemberBinding {
		public Expression Expression { get; private set; }

		public MemberAssignment Update(Expression expression) { return null; }

		internal MemberAssignment() {}
	}
}