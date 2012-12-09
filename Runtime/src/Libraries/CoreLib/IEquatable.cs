using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public interface IEquatable<in T> {
		[ScriptName("equalsT")]
		bool Equals(T other);
	}
}
