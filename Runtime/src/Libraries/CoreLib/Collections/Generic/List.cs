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
    [Imported(IsRealType = true)]
    [ScriptName("Array")]
    public sealed class List<T> : IList<T> {

        public List() {
        }

        public List(int capacity) {
        }

        [InlineCode("[ {first}{,rest} ]")]
		public List(T first, params T[] rest) {
        }

        [InlineCode("{items}.clone()")]
		public List(T[] items) {
        }

        [InlineCode("{items}.clone()")]
		public List(List<T> items) {
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

        public void AddRange(T[] items) {
        }

		public void AddRange(IList<T> items) {
        }

        [IgnoreGenericArguments]
		public TAccumulated Aggregate<TAccumulated>(TAccumulated seed, ListAggregator<TAccumulated, T> aggregator) {
            return default(TAccumulated);
        }

        [IgnoreGenericArguments]
        public TAccumulated Aggregate<TAccumulated>(TAccumulated seed, ListItemAggregator<TAccumulated, T> aggregator) {
            return default(TAccumulated);
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

        public bool Every(ListFilterCallback<T> filterCallback) {
            return false;
        }

        public bool Every(ListItemFilterCallback<T> itemFilterCallback) {
            return false;
        }

        public List<T> Extract(int index) {
            return null;
        }

        public List<T> Extract(int index, int count) {
            return null;
        }

        public List<T> Filter(ListFilterCallback<T> filterCallback) {
            return null;
        }

        public List<T> Filter(ListItemFilterCallback<T> itemFilterCallback) {
            return null;
        }

        public void ForEach(ListCallback<T> callback) {
        }

        public void ForEach(ListItemCallback<T> itemCallback) {
        }

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

        public void InsertRange(int index, T[] items) {
        }

        public void InsertRange(int index, IList<T> items) {
        }

        public string Join() {
            return null;
        }

        public string Join(string delimiter) {
            return null;
        }

        [IgnoreGenericArguments]
		public List<TTarget> Map<TTarget>(ListMapCallback<T, TTarget> mapCallback) {
            return null;
        }

        [IgnoreGenericArguments]
        public List<TTarget> Map<TTarget>(ListItemMapCallback<T, TTarget> mapItemCallback) {
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

        public bool Some(ListFilterCallback<T> filterCallback) {
            return false;
        }

        public bool Some(ListItemFilterCallback<T> itemFilterCallback) {
            return false;
        }

        public void Sort() {
        }

        public void Sort(CompareCallback<T> compareCallback) {
        }

        [ScriptSkip]
        public static explicit operator Array(List<T> list) {
            return null;
        }

        [ScriptSkip]
		public static explicit operator List<T>(Array a) {
			return null;
		}
    }
}
