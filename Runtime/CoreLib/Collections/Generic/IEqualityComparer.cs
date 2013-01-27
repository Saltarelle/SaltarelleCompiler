// IEnumerable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[ScriptName("IEqualityComparer")]
	[Imported(ObeysTypeSystem = true)]
	public interface IEqualityComparer<in T> : IEqualityComparer {
		[ScriptName("areEqual")]
		bool Equals(T x, T y);
		[ScriptName("getObjectHashCode")]
		int GetHashCode(T obj);
	}
}
