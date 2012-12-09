// IEnumerable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace System.Collections {
	/// <summary>
	/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead.
	/// </summary>
    public interface IEqualityComparer {
		/// <summary>
		/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead. When implementing <see cref="IEqualityComparer{T}"/>, just provide a dummy implementation for this method.
		/// </summary>
		[NonScriptable]
		[EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object x, object y);

		/// <summary>
		/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead. When implementing <see cref="IEqualityComparer{T}"/>, just provide a dummy implementation for this method.
		/// </summary>
		[NonScriptable]
		[EditorBrowsable(EditorBrowsableState.Never)]
		int GetHashCode(object obj);
    }
}