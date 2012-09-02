// IEnumerable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
    [ScriptNamespace("ss")]
    [ScriptName("IEqualityComparer")]
	[Imported(IsRealType = true)]
    public interface IEqualityComparer<in T> : IEqualityComparer {
        bool Equals(T x, T y);
		int GetHashCode(T obj);
    }
}
