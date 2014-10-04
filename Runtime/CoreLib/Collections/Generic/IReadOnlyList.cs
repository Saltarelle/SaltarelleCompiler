using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IReadOnlyList<T> : IReadOnlyCollection<T> {
		T this[int index] {
			[InlineCode("{$System.Script}.getItem({this}, {index})", GeneratedMethodName = "get_item")] get;
		}
	}
}
