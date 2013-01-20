// IDisposable.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

    // Script Equivalent: IDisposable
    [ScriptNamespace("ss")]
    [Imported(ObeysTypeSystem = true)]
    public interface IDisposable {
        void Dispose();
    }
}
