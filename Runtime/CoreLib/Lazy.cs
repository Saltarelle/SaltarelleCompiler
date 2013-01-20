// Action.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public class Lazy<T> {
		[InlineCode("new {$System.Lazy`1}(function() {{ return {T}.createInstance(); }})")]
		public Lazy() {
		}

		[InlineCode("new {$System.Lazy`1}(function() {{ return {T}.createInstance(); }})")]
		public Lazy(bool b) {
		}

		[AlternateSignature]
		public Lazy(Func<T> valueFactory) {
		}

		public Lazy(Func<T> valueFactory, bool b) {
		}

		[IntrinsicProperty]
		public bool IsValueCreated { get { return false; } }

		public T Value { [ScriptName("value")] get { return default(T); } }
	}
}
