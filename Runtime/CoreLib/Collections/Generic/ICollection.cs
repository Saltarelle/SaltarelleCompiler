// ICollection.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[ScriptName("ICollection")]
	[Imported(ObeysTypeSystem = true)]
	public interface ICollection<T> : IReadOnlyCollection<T> {
		[InlineCode("{$System.Script}.add({this}, {item})", GeneratedMethodName = "add")]
		void Add(T item);

		[InlineCode("{$System.Script}.clear({this})", GeneratedMethodName = "clear")]
		void Clear();

		[InlineCode("{$System.Script}.remove({this}, {item})", GeneratedMethodName = "remove")]
		bool Remove(T item);
	}
}
