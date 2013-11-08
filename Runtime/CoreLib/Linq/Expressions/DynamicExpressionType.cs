using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported]
	public enum DynamicExpressionType {
		MemberAccess,
		Invocation,
		Index
	}
}