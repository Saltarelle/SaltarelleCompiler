using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections {
	[Obsolete("Don't use this interface, use IList<object> instead", true)]
    [ScriptNamespace("ss")]
    [ScriptName("IList")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IList : ICollection {
	}
}
