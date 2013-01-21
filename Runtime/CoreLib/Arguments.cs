// Arguments.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// Provides access to the arguments of the current function.
	/// </summary>
	[Imported]
	public static class Arguments {
		/// <summary>
		/// Retrieves the number of actual arguments passed to the function.
		/// </summary>
		/// <returns>The count of arguments.</returns>
		public static int Length {
			[InlineCode("arguments.length")]
			get {
				return 0;
			}
		}

		/// <summary>
		/// Retrieves the specified actual argument value passed to the
		/// function by index.
		/// </summary>
		/// <param name="index">The index of the argument to retrieve.</param>
		/// <returns>The value of the specified argument.</returns>
		[InlineCode("arguments[{index}]")]
		public static object GetArgument(int index) {
			return null;
		}

		[InlineCode("Array.toArray(arguments)")]
		public static Array ToArray() {
			return null;
		}
	}
}
