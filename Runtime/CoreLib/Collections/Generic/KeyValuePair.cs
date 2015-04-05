// KeyValuePair.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public struct KeyValuePair<TKey, TValue> {
		[InlineCode("{{ key: {key}, value: {value} }}")]
		public KeyValuePair(TKey key, TValue value) {
		}

		[InlineCode("{{ key: {$System.Script}.getDefaultValue({TKey}), value: {$System.Script}.getDefaultValue({TValue}) }}")]
		public KeyValuePair(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor x) {
		}

		[IntrinsicProperty]
		public TKey Key {
			get {
				return default(TKey);
			}
		}

		[IntrinsicProperty]
		public TValue Value {
			get {
				return default(TValue);
			}
		}
	}
}
