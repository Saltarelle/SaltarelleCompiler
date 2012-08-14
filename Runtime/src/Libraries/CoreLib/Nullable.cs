// Nullable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {

	[IgnoreGenericArguments]
    [ScriptNamespace("ss")]
	[ScriptName("Nullable")]
    [Imported(IsRealType = true)]
    public struct Nullable<T> where T : struct {
		[InlineCode("{value}")]
        public Nullable(T value) {
        }

        public bool HasValue {
			[InlineCode("ss.isValue({this})")]
            get {
                return false;
            }
        }

		public T Value {
			[InlineCode("{$System.Nullable`1}.unbox({this})")]
            get {
                return default(T);
            }
        }

		[NonScriptable]
		public T GetValueOrDefault() {
			return default(T);
		}

		[NonScriptable]
		public T GetValueOrDefault(T defaultValue) {
			return default(T);
		}

        public static implicit operator T?(T value) {
            return null;
        }

        public static explicit operator T(T? value) {
            return default(T);
        }
    }
}
