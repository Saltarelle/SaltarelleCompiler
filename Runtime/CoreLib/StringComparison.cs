// StringComparison.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {
  [Imported]
	public enum StringComparison {
		CurrentCulture,
		CurrentCultureIgnoreCase,
		InvariantCulture,
		InvariantCultureIgnoreCase,
		Ordinal,
		OrdinalIgnoreCase
	}
}
