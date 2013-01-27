// Stack.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {

	/// <summary>
	/// The Stack data type which is mapped to the Array type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Array")]
	[IncludeGenericArguments(false)]
	public sealed class Stack<T> {

		[IntrinsicProperty]
		[ScriptName("length")]
		public int Count {
			get {
				return 0;
			}
		}

		[InlineCode("{$System.Script}.clear({this})")]
		public void Clear() {
		}

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(T item) {
			return false;
		}

		[InlineCode("{$System.Script}.arrayPeekBack({this})")]
		public T Peek() {
			return default(T);
		}

		public T Pop() {
			return default(T);
		}

		public void Push(T item) {
		}
	}
}
