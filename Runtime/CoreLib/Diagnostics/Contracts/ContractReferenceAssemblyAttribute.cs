using System.Runtime.CompilerServices;

namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// Attribute that specifies that an assembly is a reference assembly with contracts.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NonScriptable]
	public sealed class ContractReferenceAssemblyAttribute : Attribute {
	}
}