using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace QUnit {
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

		[InlineCode("ok(false, {message})")]
		public static void Fail(string message) {
		}

		[ScriptName("ok")]
		public static void OK(bool condition) {
		}

		[ScriptName("ok")]
		public static void OK(bool condition, string message) {
		}

		[InlineCode("ok({o} === null)")]
		public static void IsNull(object o) {
		}

		[InlineCode("ok({o} === null, {message})")]
		public static void IsNull(object o, string message) {
		}

		[InlineCode("ok({$System.Script}.isNullOrUndefined({o}))")]
		public static void IsNullOrUndefined(object o) {
		}

		[InlineCode("ok({$System.Script}.isNullOrUndefined({o}), {message})")]
		public static void IsNullOrUndefined(object o, string message) {
		}

		[InlineCode("ok({$System.Script}.isValue({o}))")]
		public static void IsNotNull(object o) {
		}

		[InlineCode("ok({$System.Script}.isValue({o}), {message})")]
		public static void IsNotNull(object o, string message) {
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
