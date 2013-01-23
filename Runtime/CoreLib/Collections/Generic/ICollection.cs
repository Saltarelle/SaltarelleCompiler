// ICollection.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
	[ScriptNamespace("ss")]
	[ScriptName("ICollection")]
	[Imported(ObeysTypeSystem = true)]
	public interface ICollection<T> : IEnumerable<T> {
		int Count { [InlineCode("{$System.Script}.count({this})", GeneratedMethodName = "get_count")] get; }

		[InlineCode("{$System.Script}.add({this}, {item})", GeneratedMethodName = "add")]
		void Add(T item);

		[InlineCode("{$System.Script}.clear({this})", GeneratedMethodName = "clear")]
		void Clear();

		[InlineCode("{$System.Script}.contains({this}, {item})", GeneratedMethodName = "contains")]
		bool Contains(T item);

		[InlineCode("{$System.Script}.remove({this}, {item})", GeneratedMethodName = "remove")]
		bool Remove(T item);
	}
}
