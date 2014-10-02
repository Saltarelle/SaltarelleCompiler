using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
		TValue this[TKey key] { get; set; }

		ICollection<TKey> Keys { get; }

		ICollection<TValue> Values { get; }

		int Count { get; }

		bool ContainsKey(TKey key);

		void Add(TKey key, TValue value);

		bool Remove(TKey key);

		bool TryGetValue(TKey key, out TValue value);

		void Clear();
	}
}