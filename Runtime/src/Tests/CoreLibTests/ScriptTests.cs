using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class ScriptTests {
		[Test]
		public void BooleanWorks() {
			Assert.AreStrictEqual(Script.Boolean(0), false);
			Assert.AreStrictEqual(Script.Boolean(""), false);
			Assert.AreStrictEqual(Script.Boolean("1"), true);
		}

		[Test]
		public void EvalWorks() {
			Assert.AreEqual(Script.Eval("2 + 3"), 5);
		}

		private static object Undefined { [InlineCode("undefined")] get { return null; } }

		[Test]
		public void IsNullWorks() {
			Assert.IsTrue(Script.IsNull(null));
			Assert.IsFalse(Script.IsNull(Undefined));
			Assert.IsFalse(Script.IsNull(3));
		}

		[Test]
		public void IsNullOrUndefinedWorks() {
			Assert.IsTrue(Script.IsNullOrUndefined(null));
			Assert.IsTrue(Script.IsNullOrUndefined(Undefined));
			Assert.IsFalse(Script.IsNullOrUndefined(3));
		}

		[Test]
		public void IsValueWorks() {
			Assert.IsFalse(Script.IsValue(null));
			Assert.IsFalse(Script.IsValue(Undefined));
			Assert.IsTrue(Script.IsValue(3));
		}

		[Test]
		public void UndefinedWorks() {
			Assert.IsTrue(Script.IsUndefined(Script.Undefined));
		}
	}
}
