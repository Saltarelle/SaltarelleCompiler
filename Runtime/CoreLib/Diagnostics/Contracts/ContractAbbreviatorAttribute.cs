﻿namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// Enables writing abbreviations for contracts that get copied to other methods
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[Conditional("CONTRACTS_FULL")]
	public sealed class ContractAbbreviatorAttribute : Attribute
	{
	}
}