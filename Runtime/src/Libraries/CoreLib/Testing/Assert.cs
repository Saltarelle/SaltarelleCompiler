// Assert.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace System.Testing {

    [IgnoreNamespace]
	[Imported]
	[GlobalMethods]
    public static class Assert {
        [ScriptName("equal")]
        public static void AreLooseEqual(object actual, object expected) {
        }

        [ScriptName("equal")]
        public static void AreLooseEqual(object actual, object expected, string message) {
        }

        [ScriptName("notEqual")]
        public static void AreNotEqual(object actual, object expected) {
        }

        [ScriptName("notEqual")]
        public static void AreNotEqual(object actual, object expected, string message) {
        }

        [ScriptName("strictEqual")]
        public static void AreStrictEqual(object actual, object expected) {
        }

        [ScriptName("strictEqual")]
        public static void AreStrictEqual(object actual, object expected, string message) {
        }

        [ScriptName("notStrictEqual")]
        public static void AreNotStrictEqual(object actual, object expected) {
        }

        [ScriptName("notStrictEqual")]
        public static void AreNotStrictEqual(object actual, object expected, string message) {
        }

        [ScriptName("deepEqual")]
        public static void AreEqual(object actual, object expected) {
        }

        [ScriptName("deepEqual")]
        public static void AreEqual(object actual, object expected, string message) {
        }

        [ScriptName("notDeepEqual")]
        public static void AreNotDeepEqual(object actual, object expected) {
        }

        [ScriptName("notDeepEqual")]
        public static void AreNotDeepEqual(object actual, object expected, string message) {
        }

        [ScriptName("expect")]
        public static void ExpectAsserts(int assertions) {
        }

        [ScriptName("ok")]
        public static void IsTrue(bool condition) {
        }

        [ScriptName("ok")]
        public static void IsTrue(bool condition, string message) {
        }

        [InlineCode("ok(!({condition}))")]
        public static void IsFalse(bool condition) {
        }

        [InlineCode("ok(!({condition}), {message})")]
        public static void IsFalse(bool condition, string message) {
        }

        [ScriptName("ok")]
        public static void OK(bool condition) {
        }

        [ScriptName("ok")]
        public static void OK(bool condition, string message) {
        }

		[ScriptName("throws")]
		public static void Throws(Action block, Type expected, string message) {
		}

		[ScriptName("throws")]
		public static void Throws(Action block, Regex expected, string message) {
		}

		[ScriptName("throws")]
		public static void Throws(Action block, Func<object, bool> expected, string message) {
		}

		[ScriptName("throws")]
		public static void Throws(Action block) {
		}

		[ScriptName("throws")]
		public static void Throws(Action block, string message) {
		}
    }
}
