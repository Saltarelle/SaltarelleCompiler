// Nullable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[ScriptName("Nullable")]
	[Imported(ObeysTypeSystem = true)]
	public struct Nullable<T> where T : struct {
		[InlineCode("{value}")]
		public Nullable(T value) {
		}

		public bool HasValue {
			[InlineCode("{$System.Script}.isValue({this})")]
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

		[InlineCode("{$System.Script}.coalesce({this}, {defaultValue})")]
		public T GetValueOrDefault(T defaultValue) {
			return default(T);
		}

		[ScriptSkip]
		public static implicit operator T?(T value) {
			return null;
		}

		[InlineCode("{$System.Nullable`1}.unbox({value})")]
		public static explicit operator T(T? value) {
			return default(T);
		}
	}
}
