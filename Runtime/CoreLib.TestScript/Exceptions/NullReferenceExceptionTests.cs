using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class NullReferenceExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(NullReferenceException).FullName, "ss.NullReferenceException", "Name");
			Assert.IsTrue(typeof(NullReferenceException).IsClass, "IsClass");
			Assert.AreEqual(typeof(NullReferenceException).BaseType, typeof(Exception), "BaseType");
			object d = new NullReferenceException();
			Assert.IsTrue(d is NullReferenceException, "is NullReferenceException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(NullReferenceException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new NullReferenceException();
			Assert.IsTrue((object)ex is NullReferenceException, "is NullReferenceException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Object is null.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new NullReferenceException("The message");
			Assert.IsTrue((object)ex is NullReferenceException, "is NullReferenceException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new NullReferenceException("The message", inner);
			Assert.IsTrue((object)ex is NullReferenceException, "is NullReferenceException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test(ExpectedAssertionCount = 0)]
		public void AccessingAFieldOnANullObjectCausesANullReferenceException() {
			try {
				dynamic d = null;
				#pragma warning disable 219
				int x = d.someField;
				#pragma warning restore 219
				Assert.Fail("A NullReferenceException should have been thrown");
			}
			catch (NullReferenceException) {
			}
			catch (Exception ex) {
				Assert.Fail("Expected NullReferenceException, got type " + ex.GetType());
			}
		}
	}
}
