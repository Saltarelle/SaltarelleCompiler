// ICollection.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections {
    [ScriptNamespace("ss")]
    [ScriptName("ICollection")]
	[Obsolete("Don't use this interface, use ICollection<object> instead", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICollection : IEnumerable {
    }
}
