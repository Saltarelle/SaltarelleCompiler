// Record.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {

    [Imported]
    [ScriptNamespace("ss")]
    public abstract class Record {

        [ScriptSkip]
        public static implicit operator JsDictionary(Record r) {
            return null;
        }

        [ScriptSkip]
        public static implicit operator JsDictionary<string, object>(Record r) {
            return null;
        }

        [ScriptSkip]
        public static implicit operator Record(JsDictionary d) {
            return null;
        }

        [ScriptSkip]
        public static implicit operator Record(JsDictionary<string, object> d) {
            return null;
        }
    }
}
