using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public interface IHashable<in T> : IEquatable<T> {
		int GetHashCode();
	}
}
