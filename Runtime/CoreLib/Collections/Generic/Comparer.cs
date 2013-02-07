using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	/// <summary>Provides a base class for implementations of the <see cref="T:System.Collections.Generic.IComparer`1"/> generic interface.</summary>
	/// <typeparam name="T">The type of objects to compare.</typeparam>
	[ScriptNamespace("ss")]
	[IncludeGenericArguments(false)]
	public abstract class Comparer<T> {
		/// <summary>Returns a default sort order comparer for the type specified by the generic argument.</summary>
		/// <returns>An object that inherits <see cref="T:System.Collections.Generic.Comparer`1"/> and serves as a sort order comparer for type <typeparamref name="T"/>.</returns>
		[IntrinsicProperty]
		[ScriptName("def")]
		public static Comparer<T> Default { get { return null; } }

		/// <summary>When overridden in a derived class, performs a comparison of two objects of the same type and returns a value indicating whether one object is less than, equal to, or greater than the other.</summary>
		/// <returns>A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>.Zero <paramref name="x"/> equals <paramref name="y"/>.Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
		/// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
		public abstract int Compare(T x, T y);

		public static Comparer<T> Create(Comparison<T> comparison) { return null; }
	}
}
