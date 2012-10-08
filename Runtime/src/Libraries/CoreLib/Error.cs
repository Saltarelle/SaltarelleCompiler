using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	/// <summary>
	/// Represents a JavaScript Error. Useful primarily for interop, within C# code it will always be wrapped to an Exception when caught.
	/// </summary>
	[Imported(IsRealType = true)]
	[IgnoreNamespace]
	[ScriptName("Error")]
	public class Error {
		[IntrinsicProperty]
		public string Message { get; set; }

		[IntrinsicProperty]
		public string Name { get; set; }

		/// <summary>
		/// Returns additional data associated with the error (equivalent to a property access in JS).
		/// </summary>
		[InlineCode("{this}[{name}]")]
		public object GetData(string name) { return null; }
	}
}
