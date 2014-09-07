using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Collections.Generic
{
	[Imported(ObeysTypeSystem = true)]
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	public class IteratorBlockEnumerator<T> : IEnumerator<T> {
		public T Current { get { return default(T); } }
		object IEnumerator.Current { get { return null; } }
		public bool MoveNext() { return false; }
		public void Reset() {}
		public void Dispose() {}
	}
}
