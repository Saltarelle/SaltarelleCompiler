using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq {
	[Imported]
	[IgnoreGenericArguments]
	public interface ILookup<TKey, TElement> : IEnumerable<Grouping<TKey, TElement>> {
		int Count { [ScriptName("count")] get; }
		LinqJSEnumerable<TElement> this[TKey key] { [ScriptName("get")] get; }
		bool Contains(TKey key);
	}
}