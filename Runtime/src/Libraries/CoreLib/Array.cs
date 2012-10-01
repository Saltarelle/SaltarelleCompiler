// Array.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// Equivalent to the Array type in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported(IsRealType = true)]
    public sealed class Array : IEnumerable {

        [IntrinsicProperty]
        public int Length {
            get {
                return 0;
            }
        }

        [IntrinsicProperty]
        public object this[int index] {
            get {
                return null;
            }
            set {
            }
        }

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}

		public ArrayEnumerator GetEnumerator() {
			return null;
		}

        public string Join() {
            return null;
        }

        public string Join(string delimiter) {
            return null;
        }

        public static Array Parse(string s) {
            return null;
        }

        public void Reverse() {
        }

        public static Array ToArray(object o) {
            return null;
        }

		public object GetValue(params int[] indices) {
			return null;
		}

		public void SetValue(object value, params int[] indices) {
		}

		public int GetLength(int dimension) {
			return 0;
		}

		public int Rank { get { return 0; } }

		[InlineCode("0")]
		public int GetLowerBound(int dimension) {
			return 0;
		}

		[InlineCode("{this}.getLength({dimension}) - 1")]
		public int GetUpperBound(int dimension) {
			return 0;
		}
    }
}
