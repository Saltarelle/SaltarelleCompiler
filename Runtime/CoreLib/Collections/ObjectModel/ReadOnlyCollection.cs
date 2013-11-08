using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel {
	[IncludeGenericArguments(false)]
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	[ScriptName("Array")]
	public class ReadOnlyCollection<T> : IList<T> {
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> instance.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> instance.
		/// </returns>
		[IntrinsicProperty, ScriptName("length")]
		public int Count { get; private set; }

		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		/// </returns>
		/// <param name="index">The zero-based index of the element to get.</param>
		[IntrinsicProperty]
		public T this[int index] { get { return default(T); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> class that is a read-only wrapper around the specified list.
		/// </summary>
		/// <param name="list">The list to wrap.</param>
		[InlineCode("{list}")]
		public ReadOnlyCollection(IList<T> list) {
		}

		/// <summary>
		/// Determines whether an element is in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>.
		/// </summary>
		/// <returns>
		/// true if <paramref name="value"/> is found in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>; otherwise, false.
		/// </returns>
		/// <param name="value">The object to locate in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>. The value can be null for reference types.</param>
		[InlineCode("{$System.Script}.contains({this}, {value})")]
		public bool Contains(T value) {
			return false;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IEnumerator`1"/> for the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>.
		/// </returns>
		[EnumerateAsArray]
		[InlineCode("{$System.Script}.getEnumerator({this})")]
		public IEnumerator<T> GetEnumerator() {
			return null;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>.
		/// </summary>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/> within the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>, if found; otherwise, -1.
		/// </returns>
		/// <param name="value">The object to locate in the <see cref="T:System.Collections.Generic.List`1"/>. The value can be null for reference types.</param>
		[InlineCode("{$System.Script}.indexOf({this}, {value})")]
		public int IndexOf(T value) {
			return 0;
		}

		T IList<T>.this[int index] {
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

		void ICollection<T>.Add(T value) {
		}

		void ICollection<T>.Clear() {
		}

		void IList<T>.Insert(int index, T value) {
		}

		bool ICollection<T>.Remove(T value) {
			return false;
		}

		void IList<T>.RemoveAt(int index) {
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}
	}
}