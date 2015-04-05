namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// Types marked with this attribute specify that a separate type contains the contracts for this type.
	/// </summary>
	[Conditional("CONTRACTS_FULL")]
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class ContractClassAttribute : Attribute {
		public ContractClassAttribute(Type typeContainingContracts) {
			TypeContainingContracts = typeContainingContracts;
		}

		public Type TypeContainingContracts { get; private set; }
	}
}