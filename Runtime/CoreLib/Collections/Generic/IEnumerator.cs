// IEnumerator.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
	[ScriptNamespace("ss")]
	[ScriptName("IEnumerator")]
	[Imported(ObeysTypeSystem = true)]
	public interface IEnumerator<out T> : IDisposable, IEnumerator {
		new T Current { get; }
		new bool MoveNext();
		new void Reset();
	}
}
