using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq {
	[Imported]
	[IgnoreGenericArguments]
	public class Lookup<TKey, TElement> : ILookup<TKey, TElement> {
		internal Lookup() {}

		public int Count { get { return 0; } }

		public LinqJSEnumerable<TElement> this[TKey key] { get { return null; } }

		public bool Contains(TKey key) { return false; }

		public IEnumerator<Grouping<TKey, TElement>> GetEnumerator() {
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}
	}
}