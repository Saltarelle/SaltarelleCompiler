using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq {
	[Imported]
	[IgnoreGenericArguments]
	public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement> {
		TKey Key { [ScriptName("key")] get; }
	}
}