// KeyValuePair.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[Serializable]
	[Imported]
	public struct KeyValuePair<TKey, TValue> {
		public KeyValuePair(TKey key, TValue value) {
		} 

		public TKey Key {
			get {
				return default(TKey);
			}
		}

		public TValue Value {
			get {
				return default(TValue);
			}
		}
	}
}
