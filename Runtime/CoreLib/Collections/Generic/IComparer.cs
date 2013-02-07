using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	/// <summary>Defines a method that a type implements to compare two objects.</summary>
	/// <typeparam name="T">The type of objects to compare.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
	[ScriptNamespace("ss")]
	[IncludeGenericArguments(false)]
	public interface IComparer<in T> {
		/// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
		/// <returns>A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.</returns>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		int Compare(T x, T y);
	}
}
