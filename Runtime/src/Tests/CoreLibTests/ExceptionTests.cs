using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;
using System.Text;

#pragma warning disable 162	// CS0162: Unreachable code detected. Disable because we want to assert that code does not reach unreachable parts

namespace CoreLibTests {
	[TestFixture]
	public class ExceptionTests {
		public class E1 : Exception {
			public E1(string message) : base(message) { }
		}
		public class E2 : E1 {
			public E2(string message) : base(message) { }
		}

		[Test]
		public void ThrowingAndCatchingExceptionsWorks() {
			try {
				throw new E2("The message");
				Assert.Fail("Should not get to statement after throw");
			}
			catch (E2 e) {
				Assert.AreEqual(e.Message, "The message");
				return;
			}
			Assert.Fail("Should not get to statement after catch");
		}

		[Test]
		public void ExceptionOfWrongTypeIsNotCaught() {
			try {
				throw new E1("The message");
				Assert.Fail("Should not get to statement after throw");
			}
			catch (E2) {
				Assert.Fail("Should not catch E2");
			}
			catch (E1 e) {
				Assert.AreEqual(e.Message, "The message");
				return;
			}
			Assert.Fail("Should not get to statement after catch");
		}

		[Test]
		public void CanCatchExceptionAsBaseType() {
			try {
				throw new E2("The message");
				Assert.Fail("Should not get to statement after throw");
			}
			catch (E1 e) {
				Assert.AreEqual(e.Message, "The message");
				return;
			}
			Assert.Fail("Should not get to statement after catch");
		}

		[System.Runtime.CompilerServices.InlineCode("(function() {{ throw 'The message'; }})()")]
		private static void ThrowIt() {}

		[Test]
		public void CanCatchStringAsException() {
			try {
				ThrowIt();
				Assert.Fail("Should not get to statement after throw");
			}
			catch (E1) {
				Assert.Fail("Should not catch E1");
			}
			catch (Exception e) {
				Assert.AreEqual(e.Message, "The message");
				return;
			}
			Assert.Fail("Should not get to statement after catch");
		}

		[Test(ExpectedAssertionCount = 0)]
		public void CanCatchStringAsCatchAll() {
			try {
				ThrowIt();
				Assert.Fail("Should not get to statement after throw");
			}
			catch (E1) {
				Assert.Fail("Should not catch E1");
			}
			catch {
				return;
			}
			Assert.Fail("Should not get to statement after catch");
		}
	}
}
