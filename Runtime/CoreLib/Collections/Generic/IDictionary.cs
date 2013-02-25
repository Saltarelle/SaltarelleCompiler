using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
	{
		[ScriptName("dictAdd")]
		void Add(TKey key, TValue value);

		new TValue this[TKey key] { [ScriptName("get_item")] get; set; }

		new ICollection<TKey> Keys { [ScriptName("get_keys")] get; }

		new ICollection<TValue> Values { [ScriptName("get_values")] get; }

		[ScriptName("dictRemove")]
		bool Remove(TKey key);
	}
}