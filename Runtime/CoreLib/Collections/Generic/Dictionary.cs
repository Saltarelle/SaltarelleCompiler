using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[ScriptNamespace("ss")]
	public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue> {
		[InlineCode("new ({$System.Type}.makeGenericType({$System.Collections.Generic.Dictionary`2}, [{TKey}, {TValue}]))()")]
		public Dictionary(int capacity) {}

		[InlineCode("new ({$System.Type}.makeGenericType({$System.Collections.Generic.Dictionary`2}, [{TKey}, {TValue}]))({{}}, {comparer})")]
		public Dictionary(int capacity, IEqualityComparer<TKey> comparer) {}

		[AlternateSignature]
		public Dictionary() {}

		[AlternateSignature]
		public Dictionary(JsDictionary<TKey, TValue> dictionary) {}

		[AlternateSignature]
		public Dictionary(JsDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {}

		[AlternateSignature]
		public Dictionary(IDictionary<TKey, TValue> dictionary) {}

		[InlineCode("new ({$System.Type}.makeGenericType({$System.Collections.Generic.Dictionary`2}, [{TKey}, {TValue}]))({{}}, {comparer})")]
		public Dictionary(IEqualityComparer<TKey> comparer) {}

		public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {}

		[IntrinsicProperty]
		public IEqualityComparer<TKey> Comparer { get { return null; } }

		public int Count { get { return 0; } }

		public new ICollection<TKey> Keys { get { return null; } }

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
