// IEnumerable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
    [ScriptNamespace("ss")]
    [ScriptName("IEnumerable")]
	[Imported(IsRealType = true)]
    public interface IEnumerable<out T> : IEnumerable {
        new IEnumerator<T> GetEnumerator();
    }
}
