﻿namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// Allows a field f to be used in the method contracts for a method m when f has less visibility than m.
	/// For instance, if the method is public, but the field is private.
	/// </summary>
	[Conditional("CONTRACTS_FULL")]
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ContractPublicPropertyNameAttribute : Attribute
	{
		public ContractPublicPropertyNameAttribute(String name)
		{
			Name = name;
		}

		public String Name { get; private set; }
	}
}