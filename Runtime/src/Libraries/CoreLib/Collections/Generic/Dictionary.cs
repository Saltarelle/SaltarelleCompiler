using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[ScriptNamespace("ss")]
	[Imported(IsRealType = true)]
	public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>  {
		[NonScriptable]
		public Dictionary(int capacity) {}

		[ScriptName("")]
		public Dictionary() {}
		[ScriptName("")]
		public Dictionary(JsDictionary<TKey, TValue> dictionary) {}
		[ScriptName("")]
		public Dictionary(IDictionary<TKey, TValue> dictionary) {}

		public int Count { get { return 0; } }

		public ICollection<TKey> Keys { get { return null; } }

		public ICollection<TValue> Values { get { return null; } }

		public TValue this[TKey key] { get { return default(TValue); } set {} }

		public void Add(TKey key, TValue value) {}

		public void Clear() {}

		public bool ContainsKey(TKey key) { return false; }

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {return null; }

		IEnumerator IEnumerable.GetEnumerator() { return null; }

		public bool Remove(TKey key) { return false; }

		public bool TryGetValue(TKey key, out TValue value) { value = default(TValue); return false; }
	}
}
