using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Saltarelle.Compiler.OOPEmulator {
	public static class Tarjan {
		private class Algorithm<T> {
			private readonly Func<T, IEnumerable<T>> _getDependencies;
			private readonly IEqualityComparer<T> _comparer;

			private Dictionary<T, int> _indices;
			private Dictionary<T, int> _lowlink;
			private List<IList<T>> _result;
			private Stack<T> _s;
			private int _index;

			public Algorithm(Func<T, IEnumerable<T>> getDependencies, IEqualityComparer<T> comparer) {
				_getDependencies = getDependencies;
				_comparer = comparer ?? EqualityComparer<T>.Default;
			}

			public List<IList<T>> MainLoop(IEnumerable<T> vertices) {
				_result = new List<IList<T>>();
				_indices = new Dictionary<T, int>(_comparer);
				_lowlink = new Dictionary<T, int>(_comparer);
				_s = new Stack<T>();
				_index = 0;
				foreach (var v in vertices) {
					if (!_indices.ContainsKey(v)) {
						StrongConnect(v);
					}
				}
				return _result;
			}

			private void StrongConnect(T v) {
				_indices[v] = _index;
				_lowlink[v] = _index;
				_index++;
				_s.Push(v);

				foreach (var w in _getDependencies(v)) {
					if (!_indices.ContainsKey(w)) {
						StrongConnect(w);
						_lowlink[v] = Math.Min(_lowlink[v], _lowlink[w]);
					}
					else if (_s.Contains(w)) {
						_lowlink[v] = Math.Min(_lowlink[v], _indices[w]);
					}
				}

				if (_lowlink[v] == _indices[v]) {
					var scc = new List<T>();
					T w;
					do {
						w = _s.Pop();
						scc.Add(w);
					} while (!_comparer.Equals(v, w));
					_result.Add(scc);
				}
			}
        }

		public static IList<IList<T>> FindAndTopologicallySortStronglyConnectedComponents<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, IEqualityComparer<T> comparer = null) {
			var result = new Algorithm<T>(getDependencies, comparer).MainLoop(source);
			result.Reverse();
			return result;
		} 
	}
}
