using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Saltarelle.Compiler {
	public static class TopologicalSorter {
		// Tarjan's algorithm: http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
		private class Algorithm<T> {
			private readonly ILookup<T, T> _edges;
			private readonly IEqualityComparer<T> _comparer;

			private Dictionary<T, int> _indices;
			private Dictionary<T, int> _lowlink;
			private List<IList<T>> _result;
			private Stack<T> _s;
			private int _index;

			public Algorithm(ILookup<T, T> edges, IEqualityComparer<T> comparer) {
				_edges = edges;
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

				foreach (var w in _edges[v]) {
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

		public static IList<IList<T>> FindAndTopologicallySortStronglyConnectedComponents<T>(IEnumerable<T> source, IEnumerable<Tuple<T, T>> edges, IEqualityComparer<T> comparer = null) {
			var result = new Algorithm<T>(edges.ToLookup(e => e.Item1, e => e.Item2), comparer).MainLoop(source);
			return result;
		}

		public static IList<IList<TSource>> FindAndTopologicallySortStronglyConnectedComponents<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<Tuple<TVertex, TVertex>> edges, IEqualityComparer<TVertex> comparer = null) {
			var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
			return FindAndTopologicallySortStronglyConnectedComponents(backref.Keys, edges, comparer).Select(l => (IList<TSource>)l.Select(t => backref[t]).ToList()).ToList();
		}

		public static IEnumerable<T> TopologicalSort<T>(IEnumerable<T> source, IEnumerable<Tuple<T, T>> edges, IEqualityComparer<T> comparer = null) {
			var result = FindAndTopologicallySortStronglyConnectedComponents(source, edges, comparer);
			if (result.Any(x => x.Count > 1))
				throw new InvalidOperationException("Cycles in graph");
			return result.Select(x => x[0]);
		}

		public static IEnumerable<TSource> TopologicalSort<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<Tuple<TVertex, TVertex>> edges, IEqualityComparer<TVertex> comparer = null) {
			var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
			return TopologicalSort(backref.Keys, edges, comparer).Select(t => backref[t]);
		}
	}
}
