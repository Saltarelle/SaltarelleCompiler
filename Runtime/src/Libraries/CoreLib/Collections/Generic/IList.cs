using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
    [ScriptNamespace("ss")]
	[ScriptName("IList")]
	[Imported(ObeysTypeSystem = true)]
	public interface IList<T> : ICollection<T> {
		T this[int index] { get; set; }

		int IndexOf(T item);

		void Insert(int index, T item);

		void RemoveAt(int index);
	}
}
