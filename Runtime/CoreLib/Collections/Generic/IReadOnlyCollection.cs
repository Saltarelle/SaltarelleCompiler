// IReadOnlyCollection.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[ScriptName("IReadOnlyCollection")]
	[Imported(ObeysTypeSystem = true)]
	public interface IReadOnlyCollection<T> : IEnumerable<T> {
		int Count { [InlineCode("{$System.Script}.count({this})", GeneratedMethodName = "get_count")] get; }

		[InlineCode("{$System.Script}.contains({this}, {item})", GeneratedMethodName = "contains")]
		bool Contains(T item);
	}
}
