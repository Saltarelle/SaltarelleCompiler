using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
		TValue this[TKey key] { get; }

		ICollection<TKey> Keys { get; }

		ICollection<TValue> Values { get; }

		bool ContainsKey(TKey key);

		bool TryGetValue(TKey key, out TValue value);

		int Count { get; }
	}
}