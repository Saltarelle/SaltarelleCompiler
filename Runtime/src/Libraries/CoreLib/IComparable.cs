using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public interface IComparable<in T> {
		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		[InlineCode("{$System.Script}.compare({this}, {other})", GeneratedMethodName = "compareTo")]
		int CompareTo(T other);
	}
}
