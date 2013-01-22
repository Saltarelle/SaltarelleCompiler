	// Object.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// Equivalent to the Object type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	public class Object {

		/// <summary>
		/// Retrieves the type associated with an object instance.
		/// </summary>
		/// <returns>The type of the object.</returns>
		[InlineCode("{$System.Script}.getInstanceType({this})")]
		public Type GetType() {
			return null;
		}

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <returns>The string representation of the object.</returns>
		public virtual string ToString() {
			return null;
		}

		/// <summary>
		/// Converts an object to its culture-sensitive string representation.
		/// </summary>
		/// <returns>The culture-sensitive string representation of the object.</returns>
		public virtual string ToLocaleString() {
			return null;
		}

		[InlineCode("{$System.Script}.equals({this}, {o})", GeneratedMethodName = "equals", NonVirtualCode = "{this} === {o}")]
		public virtual bool Equals(object o) {
			return false;
		}

		[InlineCode("{$System.Script}.getHashCode({this})", GeneratedMethodName = "getHashCode", NonVirtualCode = "0")]
		public virtual int GetHashCode() {
			return 0;
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool Equals(object a, object b) {
			return false;
		}

		/// <summary>
		/// Returns an array of a given object's own enumerable properties, in the same order as that provided by a for-in loop (the difference being that a for-in loop enumerates properties in the prototype chain as well).
		/// </summary>
		/// <param name="obj">Object to investigate</param>
		public static string[] Keys(object obj) {
			return null;
		}

		/// <summary>
		/// Returns an array of all properties (enumerable or not) found directly upon a given object.
		/// </summary>
		/// <param name="obj">Object to investigate</param>
		public static string[] GetOwnPropertyNames(object obj) {
			return null;
		}
	}
}
