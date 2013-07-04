using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class InvalidCastExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(InvalidCastException).FullName, "ss.InvalidCastException", "Name");
			Assert.IsTrue(typeof(InvalidCastException).IsClass, "IsClass");
			Assert.AreEqual(typeof(InvalidCastException).BaseType, typeof(Exception), "BaseType");
			object d = new InvalidCastException();
			Assert.IsTrue(d is InvalidCastException, "is InvalidCastException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(InvalidCastException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new InvalidCastException();
			Assert.IsTrue((object)ex is InvalidCastException, "is InvalidCastException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The cast is not valid.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new InvalidCastException("The message");
			Assert.IsTrue((object)ex is InvalidCastException, "is InvalidCastException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new InvalidCastException("The message", inner);
			Assert.IsTrue((object)ex is InvalidCastException, "is InvalidCastException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
