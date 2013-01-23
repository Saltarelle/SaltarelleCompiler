using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
	[ScriptNamespace("ss")]
	[ScriptName("IList")]
	[Imported(ObeysTypeSystem = true)]
	public interface IList<T> : ICollection<T> {
		T this[int index] {
			[InlineCode("{$System.Script}.getItem({this}, {index})", GeneratedMethodName = "get_item")] get;
			[InlineCode("{$System.Script}.setItem({this}, {index}, {value})", GeneratedMethodName = "set_item")] set;
		}

		[InlineCode("{$System.Script}.indexOf({this}, {item})", GeneratedMethodName = "indexOf")]
		int IndexOf(T item);

		[InlineCode("{$System.Script}.insert({this}, {index}, {item})", GeneratedMethodName = "insert")]
		void Insert(int index, T item);

		[InlineCode("{$System.Script}.removeAt({this}, {index})", GeneratedMethodName = "removeAt")]
		void RemoveAt(int index);
	}
}
