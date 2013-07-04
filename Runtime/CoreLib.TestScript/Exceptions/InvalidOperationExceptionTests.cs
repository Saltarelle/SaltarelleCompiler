using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class InvalidOperationExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(InvalidOperationException).FullName, "ss.InvalidOperationException", "Name");
			Assert.IsTrue(typeof(InvalidOperationException).IsClass, "IsClass");
			Assert.AreEqual(typeof(InvalidOperationException).BaseType, typeof(Exception), "BaseType");
			object d = new InvalidOperationException();
			Assert.IsTrue(d is InvalidOperationException, "is InvalidOperationException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(InvalidOperationException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new InvalidOperationException();
			Assert.IsTrue((object)ex is InvalidOperationException, "is InvalidOperationException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Operation is not valid due to the current state of the object.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new InvalidOperationException("The message");
			Assert.IsTrue((object)ex is InvalidOperationException, "is InvalidOperationException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new InvalidOperationException("The message", inner);
			Assert.IsTrue((object)ex is InvalidOperationException, "is InvalidOperationException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
