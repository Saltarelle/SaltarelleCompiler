using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
		TValue this[TKey key] { get; }

		#warning TODO: These members should be IEnumerable, but in order to do that we'd need some kind of special handling to allow a class to implement both IDictionary and IReadOnlyDictionary
		ICollection<TKey> Keys { get; }

		ICollection<TValue> Values { get; }

		bool ContainsKey(TKey key);

		bool TryGetValue(TKey key, out TValue value);

		int Count { get; }
	}
}