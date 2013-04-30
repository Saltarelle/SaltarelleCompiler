// IFormatProvider.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IFormatProvider {
		Object GetFormat(Type formatType);
	}
}
