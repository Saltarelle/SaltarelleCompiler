// Array.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {
	[ScriptNamespace("ss")]
	[IncludeGenericArguments(false)]
	[Imported(ObeysTypeSystem = true)]
	public sealed class ObjectEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>> {
		public KeyValuePair<TKey, TValue> Current { get { return default(KeyValuePair<TKey, TValue>); } }
		object IEnumerator.Current { get { return null; } }

		public bool MoveNext() { return false; }
		public void Reset() {}
		public void Dispose() {}
	}
}
