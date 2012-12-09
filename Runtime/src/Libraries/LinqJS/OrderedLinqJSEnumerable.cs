using System.Runtime.CompilerServices;

namespace System.Linq {
	[Imported]
	[IgnoreGenericArguments]
	public class OrderedLinqJSEnumerable<TSource> : LinqJSEnumerable<TSource> {
		internal OrderedLinqJSEnumerable() {}

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> ThenByDescending<TKey>(Func<TSource, TKey> keySelector) { return null; }
	}
}