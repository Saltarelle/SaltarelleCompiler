using System.Runtime.CompilerServices;

namespace System.Diagnostics.Contracts {
	[ScriptNamespace("ss")]
	public enum ContractFailureKind {
		Precondition,
		Postcondition,
		PostconditionOnException,
		Invariant,
		Assert,
		Assume,
	}
}