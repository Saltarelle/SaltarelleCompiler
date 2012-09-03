// RegularExpression.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions {
	/// <summary>
    /// Equivalent to the RegExp type in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported(IsRealType = true)]
    [ScriptName("RegExp")]
    public sealed class Regex {
		[ScriptName("")]
        public Regex(string pattern) {
        }

		[ScriptName("")]
        public Regex(string pattern, string flags) {
        }

        [IntrinsicProperty]
        public int LastIndex {
            get {
                return 0;
            }
            set {
            }
        }

        [IntrinsicProperty]
        public bool Global {
            get {
                return false;
            }
        }

        [IntrinsicProperty]
        public bool IgnoreCase {
            get {
                return false;
            }
        }

        [IntrinsicProperty]
        public bool Multiline {
            get {
                return false;
            }
        }

        [IntrinsicProperty]
		[ScriptName("source")]
        public string Pattern {
            get {
                return null;
            }
        }

        [IntrinsicProperty]
        public string Source {
            get {
                return null;
            }
        }

        public RegexMatch Exec(string s) {
            return null;
        }

        public bool Test(string s) {
            return false;
        }
    }
}
