// jQueryPosition.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Runtime.CompilerServices;
using System.Collections;

namespace jQueryApi {

    /// <summary>
    /// Provides information about the position of an element.
    /// </summary>
    [Imported]
	[Serializable]
    public sealed class jQueryPosition {
        public jQueryPosition() {
        }

        public jQueryPosition(int left, int top) {
        }

        /// <summary>
        /// Gets the left coordinate.
        /// </summary>
        public int Left {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Gets the top coordinate.
        /// </summary>

        public int Top {
            get {
                return 0;
            }
        }

        [ScriptSkip]
		public static implicit operator JsDictionary(jQueryPosition position) {
            return null;
        }
    }
}
