namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// Types marked with this attribute specify that they are a contract for the type that is the argument of the constructor.
	/// </summary>
	[Conditional("CONTRACTS_FULL")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ContractClassForAttribute : Attribute {
		public ContractClassForAttribute(Type typeContractsAreFor) {
			TypeContractsAreFor = typeContractsAreFor;
		}

		public Type TypeContractsAreFor { get; private set; }
	}
}