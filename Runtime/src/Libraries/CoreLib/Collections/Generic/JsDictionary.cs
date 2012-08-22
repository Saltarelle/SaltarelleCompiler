// Dictionary.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
    /// <summary>
    /// The JsDictionary data type which is mapped to the Object type in Javascript.
    /// </summary>
    [Imported]
    [IgnoreNamespace]
	[IgnoreGenericArguments]
    [ScriptName("Object")]
    public sealed class JsDictionary<TKey, TValue> {
		[InlineCode("{{}}")]
        public JsDictionary() {
        }

		public JsDictionary(params object[] nameValuePairs) {
		}

		public int Count {
			[InlineCode("{$System.Object}.getKeyCount({this})")]
            get {
                return 0;
            }
        }

        public ICollection<TKey> Keys {
			[InlineCode("{$System.Object}.keys({this})")]
            get {
                return null;
            }
        }

        [IntrinsicProperty]
        public TValue this[TKey key] {
            get {
                return default(TValue);
            }
            set {
            }
        }

        [InlineCode("{$System.Object}.clearKeys({this})")]
		public void Clear() {
        }

        [InlineCode("{$System.Object}.keyExists({this}, {key})")]
		public bool ContainsKey(TKey key) {
            return false;
        }

        [ScriptSkip]
		public static JsDictionary<TKey, TValue> GetDictionary(object o) {
            return null;
        }

        [InlineCode("{$System.Object}.getObjectEnumerator({this})")]
		public ObjectEnumerator<TKey, TValue> GetEnumerator() {
            return null;
        }

        [InlineCode("delete {this}[{key}]")]
		public void Remove(TKey key) {
        }
    }
}
