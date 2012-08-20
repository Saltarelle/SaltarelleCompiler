// IEnumerable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	/// <summary>
	/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead.
	/// </summary>
	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
    public interface IEqualityComparer {
		/// <summary>
		/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead. When implementing <see cref="IEqualityComparer{T}"/>, just provide a dummy implementation for this method.
		/// </summary>
        bool Equals(object x, object y);

		/// <summary>
		/// Don't use. Use <see cref="IEqualityComparer{T}"/> instead. When implementing <see cref="IEqualityComparer{T}"/>, just provide a dummy implementation for this method.
		/// </summary>
		int GetHashCode(object obj);
    }

	[IgnoreGenericArguments]
    [ScriptNamespace("ss")]
    [ScriptName("IEqualityComparer")]
	[Imported(IsRealType = true)]
    public interface IEqualityComparer<in T> : IEqualityComparer {
        bool Equals(T x, T y);
		int GetHashCode(T obj);
    }
}
