using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NodeJS.AssertModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("assert")]
	public static class Assert {
		public static void Fail(object actual, object expected, string message, string @operator) {}

        [ScriptName("equal")]
        public static void AreLooseEqual(object actual, object expected) {}

        [ScriptName("equal")]
        public static void AreLooseEqual(object actual, object expected, string message) {}

        [ScriptName("notEqual")]
        public static void AreNotEqual(object actual, object expected) {}

        [ScriptName("notEqual")]
        public static void AreNotEqual(object actual, object expected, string message) {}

        [ScriptName("strictEqual")]
        public static void AreStrictEqual(object actual, object expected) {}

        [ScriptName("strictEqual")]
        public static void AreStrictEqual(object actual, object expected, string message) {}

        [ScriptName("notStrictEqual")]
        public static void AreNotStrictEqual(object actual, object expected) {}

        [ScriptName("notStrictEqual")]
        public static void AreNotStrictEqual(object actual, object expected, string message) {}

        [ScriptName("deepEqual")]
        public static void AreEqual(object actual, object expected) {}

        [ScriptName("deepEqual")]
        public static void AreEqual(object actual, object expected, string message) {}

        [ScriptName("notDeepEqual")]
        public static void AreNotDeepEqual(object actual, object expected) {}

        [ScriptName("notDeepEqual")]
        public static void AreNotDeepEqual(object actual, object expected, string message) {}

        [ScriptName("expect")]
        public static void ExpectAsserts(int assertions) {}

        [ScriptName("ok")]
        public static void IsTrue(bool condition) {}

        [ScriptName("ok")]
        public static void IsTrue(bool condition, string message) {}

        [InlineCode("ok(!({condition}))")]
        public static void IsFalse(bool condition) {}

        [InlineCode("ok(!({condition}), {message})")]
        public static void IsFalse(bool condition, string message) {}

        [ScriptName("ok")]
        public static void OK(bool condition) {}

        [ScriptName("ok")]
        public static void OK(bool condition, string message) {}

		public static void Throws(Action block, Type expected, string message) {}

		public static void Throws(Action block, Regex expected, string message) {}

		public static void Throws(Action block, Func<object, bool> expected, string message) {}

		public static void Throws(Action block) {}

		public static void Throws(Action block, string message) {}

		public static void DoesNotThrow(Action block, Type expected, string message) {}

		public static void DoesNotThrow(Action block, Regex expected, string message) {}

		public static void DoesNotThrow(Action block, Func<object, bool> expected, string message) {}

		public static void DoesNotThrow(Action block) {}

		public static void DoesNotThrow(Action block, string message) {}

		public static void IfError(object value) {}
	}
}
