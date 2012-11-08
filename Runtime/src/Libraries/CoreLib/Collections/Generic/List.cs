// List.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Collections.Generic {

    /// <summary>
    /// Equivalent to the Array type in Javascript.
    /// </summary>
	[IgnoreGenericArguments]
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

        [InlineCode("[ {first}, {*rest} ]")]
		public List(T first, params T[] rest) {
        }

        [InlineCode("{items}.clone()")]
		public List(T[] items) {
        }

        [InlineCode("{items}.clone()")]
		public List(List<T> items) {
        }

		[InlineCode("{$System.Collections.Generic.List`1}.fromEnumerable({items})")]
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

        public void Add(T item) {
        }

		public void AddRange(IEnumerable<T> items) {
        }

        public List<T> Clone() {
            return null;
        }

        public void Clear() {
        }

        public List<T> Concat(params T[] objects) {
            return null;
        }

        public bool Contains(T item) {
            return false;
        }

        public bool Every(Func<T, int, List<T>, bool> callback) {
            return false;
        }

        public bool Every(Func<T, bool> callback) {
            return false;
        }

        public List<T> Extract(int index) {
            return null;
        }

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
        public IEnumerator<T> GetEnumerator() {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return null;
        }

        public int IndexOf(T item) {
            return 0;
        }

        public int IndexOf(T item, int startIndex) {
            return 0;
        }

        public void Insert(int index, T item) {
        }

        public void InsertRange(int index, IEnumerable<T> items) {
        }

        public string Join() {
            return null;
        }

        public string Join(string delimiter) {
            return null;
        }

        [IgnoreGenericArguments]
		public List<TTarget> Map<TTarget>(Func<T, int, List<T>, TTarget> callback) {
            return null;
        }

        [IgnoreGenericArguments]
        public List<TTarget> Map<TTarget>(Func<T, TTarget> callback) {
            return null;
        }

        public static List<T> Parse(string s) {
            return null;
        }

        public bool Remove(T item) {
            return false;
        }

        public void RemoveAt(int index) {
        }

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
    }
}
