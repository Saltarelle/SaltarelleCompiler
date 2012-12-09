using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IgnoreGenericArguments]
	[ScriptNamespace("ss")]
	public abstract class EqualityComparer<T> : IEqualityComparer<T> {
		[IntrinsicProperty]
		[ScriptName("def")]
		public static EqualityComparer<T> Default { get { return null; } }

		public abstract bool Equals(T x, T y);
		public abstract int GetHashCode(T obj);

		bool IEqualityComparer.Equals(object x, object y) { return false; }
		int IEqualityComparer.GetHashCode(object obj) { return 0; }
	}
}