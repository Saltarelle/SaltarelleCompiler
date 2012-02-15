using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler {
	public class ReadOnlySet<T> : ISet<T> {
		private ISet<T> _wrapped;

		public IEnumerator<T> GetEnumerator() { return _wrapped.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public bool Add(T item) { throw new InvalidOperationException("Read-only"); }
		void ICollection<T>.Add(T item) { throw new InvalidOperationException("Read-only"); }
		public void UnionWith(IEnumerable<T> other) { throw new InvalidOperationException("Read-only"); }
		public void IntersectWith(IEnumerable<T> other) { throw new InvalidOperationException("Read-only"); }
		public void ExceptWith(IEnumerable<T> other) { throw new InvalidOperationException("Read-only"); }
		public void SymmetricExceptWith(IEnumerable<T> other) { throw new InvalidOperationException("Read-only"); }
		public bool IsSubsetOf(IEnumerable<T> other) { return _wrapped.IsSubsetOf(other); }
		public bool IsSupersetOf(IEnumerable<T> other) { return _wrapped.IsSupersetOf(other); }
		public bool IsProperSupersetOf(IEnumerable<T> other) { return _wrapped.IsProperSupersetOf(other); }
		public bool IsProperSubsetOf(IEnumerable<T> other) { return _wrapped.IsProperSubsetOf(other); }
		public bool Overlaps(IEnumerable<T> other) { return _wrapped.Overlaps(other); }
		public bool SetEquals(IEnumerable<T> other) { return _wrapped.SetEquals(other); }
		public void Clear() { throw new InvalidOperationException("Read-only"); }
		public bool Contains(T item) { return _wrapped.Contains(item); }
		public void CopyTo(T[] array, int arrayIndex) { _wrapped.CopyTo(array, arrayIndex); }
		public bool Remove(T item) { throw new InvalidOperationException("Read-only"); }
		public int Count { get { return _wrapped.Count; } }
		public bool IsReadOnly { get { return true; } }

		public ReadOnlySet(ISet<T> wrapped) {
			_wrapped = wrapped;
		}
	}
}
