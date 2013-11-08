using System.Runtime.CompilerServices;

namespace System {
	[IncludeGenericArguments(true)]
	[ScriptNamespace("ss")]
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
			[InlineCode("{$System.Script}.unbox({this})")]
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
