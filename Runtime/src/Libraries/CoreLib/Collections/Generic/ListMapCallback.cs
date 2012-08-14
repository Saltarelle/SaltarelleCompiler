// ListMapCallback.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
    public delegate TTarget ListMapCallback<TSource, TTarget>(TSource value, int index, ICollection<TSource> list);
}
