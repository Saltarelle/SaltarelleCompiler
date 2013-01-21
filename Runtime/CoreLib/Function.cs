// Function.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// Equivalent to the Function type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	public sealed class Function {
		/// <summary>
		/// Creates a new function with the specified implementation, and the
		/// set of named parameters.
		/// </summary>
		/// <param name="argNamesAndBody">Argument names, followed by the function body.</param>
		[ExpandParams]
		public Function(params string[] argNamesAndBody) {
		}

		/// <summary>
		/// Gets the number of parameters expected by the function.
		/// </summary>
		[IntrinsicProperty]
		public int Length {
			get {
				return 0;
			}
		}

		/// <summary>
		/// Invokes the function against the specified object instance.
		/// </summary>
		/// <param name="instance">The object used as the value of 'this' within the function.</param>
		/// <returns>Any return value returned from the function.</returns>
		public object Apply(object instance) {
			return null;
		}

		/// <summary>
		/// Invokes the function against the specified object instance.
		/// </summary>
		/// <param name="instance">The object used as the value of 'this' within the function.</param>
		/// <param name="arguments">The set of arguments to pass in into the function.</param>
		/// <returns>Any return value returned from the function.</returns>
		public object Apply(object instance, params object[] arguments) {
			return null;
		}

		/// <summary>
		/// Invokes the function against the specified object instance.
		/// </summary>
		/// <param name="instance">The object used as the value of 'this' within the function.</param>
		/// <returns>Any return value returned from the function.</returns>
		public object Call(object instance) {
			return null;
		}

		/// <summary>
		/// Invokes the function against the specified object instance.
		/// </summary>
		/// <param name="instance">The object used as the value of 'this' within the function.</param>
		/// <param name="arguments">One or more arguments to pass in into the function.</param>
		/// <returns>Any return value returned from the function.</returns>
		[ExpandParams]
		public object Call(object instance, params object[] arguments) {
			return null;
		}

		[ScriptSkip]
		public static explicit operator Function(Delegate d) {
			return null;
		}
	}
}
