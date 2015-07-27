namespace Saltarelle.Compiler {
	/// <summary>
	/// Describes the kind of lift that is supposed to be performed by <see cref="IRuntimeLibrary.Lift"/>
	/// </summary>
	public enum LiftType {
		/// <summary>
		/// A regular lift, ie. return null if any of the operands is null.
		/// </summary>
		Regular,
		/// <summary>
		/// A comparison lift (except except for == / !=), ie. return false if any of the operands is null.
		/// </summary>
		Comparison,
		/// <summary>
		/// A lift of the == operator, ie. return true if all values are null, otherwise false if any value is null.
		/// </summary>
		Equality,
		/// <summary>
		/// A lift of the != operator, ie. return false if all values are null, otherwise true if any value is null.
		/// </summary>
		Inequality,
	}
}