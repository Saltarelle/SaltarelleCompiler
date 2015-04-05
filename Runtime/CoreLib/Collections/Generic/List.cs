// List.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {

	/// <summary>
	/// Equivalent to the Array type in Javascript.
	/// </summary>
	[IncludeGenericArguments(false)]
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Array")]
	public sealed class List<T> : IList<T> {

		[InlineCode("[]")]
		public List() {
		}

		[InlineCode("[]")]
		public List(int capacity) {
		}

		[InlineCode("[ {first}, {*rest} ]", NonExpandedFormCode = "[{first}].concat({rest})")]
		public List(T first, params T[] rest) {
		}

		[InlineCode("{$System.Script}.arrayClone({items})")]
		public List(T[] items) {
		}

		[InlineCode("{$System.Script}.arrayClone({items})")]
		public List(List<T> items) {
		}

		[InlineCode("{$System.Script}.arrayFromEnumerable({items})")]
		public List(IEnumerable<T> items) {
		}
		
		[IntrinsicProperty]
		[ScriptName("length")]
		public int Count {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public T this[int index] {
			get {
				return default(T);
			}
			set {
			}
		}

		// Not used because we will either be invoked from the interface or use our own property with the same name
		int ICollection<T>.Count {
			get {
				return 0;
			}
		}

		T IList<T>.this[int index] {
			get {
				return default(T);
			}
			set {
			}
		}

		[ScriptName("push")]
		public void Add(T item) {
		}

		void ICollection<T>.Add(T item) {
		}

		[InlineCode("{$System.Script}.arrayAddRange({this}, {items})")]
		public void AddRange(IEnumerable<T> items) {
		}

		[InlineCode("{$System.Script}.arrayClone({this})")]
		public List<T> Clone() {
			return null;
		}

		[InlineCode("{$System.Script}.clear({this})")]
		public void Clear() {
		}

		public List<T> Concat(params T[] objects) {
			return null;
		}

		[InlineCode("{$System.Script}.contains({this}, {item})")]
		public bool Contains(T item) {
			return false;
		}

		public bool Every(Func<T, int, List<T>, bool> callback) {
			return false;
		}

		public bool Every(Func<T, bool> callback) {
			return false;
		}

		[InlineCode("{$System.Script}.arrayExtract({this}, {index})")]
		public List<T> Extract(int index) {
			return null;
		}

		[InlineCode("{$System.Script}.arrayExtract({this}, {index}, {count})")]
		public List<T> Extract(int index, int count) {
			return null;
		}

		public Array Slice(int start) {
			return null;
		}

		public Array Slice(int start, int end) {
			return null;
		}

		public List<T> Filter(Func<T, int, List<T>, bool> callback) {
			return null;
		}

		public List<T> Filter(Func<T, bool> callback) {
			return null;
		}

		public void ForEach(Action<T, int, List<T>> callback) {
		}

		public void ForEach(Action<T> callback) {
		}

		[EnumerateAsArray]
		[InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<T> GetEnumerator() {
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}

		[InlineCode("{$System.Script}.indexOf({this}, {item})")]
		public int IndexOf(T item) {
			return 0;
		}

		[InlineCode("{$System.Script}.indexOfArray({this}, {item}, {startIndex})")]
		public int IndexOf(T item, int startIndex) {
			return 0;
		}

		[InlineCode("{$System.Script}.insert({this}, {index}, {item})")]
		public void Insert(int index, T item) {
		}

		[InlineCode("{$System.Script}.arrayInsertRange({this}, {index}, {items})")]
		public void InsertRange(int index, IEnumerable<T> items) {
		}

		public string Join() {
			return null;
		}

		public string Join(string delimiter) {
			return null;
		}

		[IncludeGenericArguments(false)]
		public List<TTarget> Map<TTarget>(Func<T, int, List<T>, TTarget> callback) {
			return null;
		}

		[IncludeGenericArguments(false)]
		public List<TTarget> Map<TTarget>(Func<T, TTarget> callback) {
			return null;
		}

		[InlineCode("{$System.Script}.remove({this}, {item})")]
		public bool Remove(T item) {
			return false;
		}

		[InlineCode("{$System.Script}.removeAt({this}, {index})")]
		public void RemoveAt(int index) {
		}

		[InlineCode("{$System.Script}.arrayRemoveRange({this}, {index}, {count})")]
		public void RemoveRange(int index, int count) {
		}

		public void Reverse() {
		}

		public bool Some(Func<T, int, List<T>, bool> callback) {
			return false;
		}

		public bool Some(Func<T, bool> callback) {
			return false;
		}

		public void Sort() {
		}

		public void Sort(Func<T, T, int> callback) {
		}

		[InlineCode("{this}.sort({$System.Script}.mkdel({comparer}, 'compare'))")]
		public void Sort(IComparer<T> comparer) {
		}

		[InlineCode("{$System.Array}.prototype.slice.call({this})")]
		public T[] ToArray() {
			return null;
		}
	}
}
