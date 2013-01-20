// Array.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {
    [ScriptNamespace("ss")]
    [Imported(ObeysTypeSystem = true)]
    public sealed class ArrayEnumerator : IEnumerator<object> {
    	public object Current { get { return null; } }

    	public bool MoveNext() { return false; }

    	public void Reset() {}
    	public void Dispose() {}
    }

    [ScriptNamespace("ss")]
    [Imported(ObeysTypeSystem = true)]
	[IgnoreGenericArguments]
    public sealed class ArrayEnumerator<T> : IEnumerator<T> {
		object IEnumerator.Current { get { return default(T); } }

    	public T Current { get { return default(T); } }

    	public bool MoveNext() { return false; }

    	public void Reset() {}
    	public void Dispose() {}
    }
}
