using System.Runtime.CompilerServices;

namespace System
{
	/// <summary>
	/// Specifies how mathematical rounding methods should process a number that is midway between two numbers.
	/// </summary>
	[Imported]
	public enum MidpointRounding
	{
		ToEven = 0,
		AwayFromZero = 1,
	}
}
