using System.Runtime.CompilerServices;

namespace System.Reflection {
	/// <summary>
	/// Specifies flags that control binding and the way in which the search for members and types is conducted by reflection.
	/// </summary>
	[Imported]
	[Flags]
	public enum BindingFlags {
		Default  = 0,
		Instance = 4,
		Static   = 8,
	}
}