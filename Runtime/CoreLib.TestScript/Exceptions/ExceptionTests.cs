using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class ExceptionTests {
		class MyException : Exception {
			private readonly string _message;
			private readonly Exception _innerException;

			public MyException(string message, Exception innerException) {
				_message = message;
				_innerException = innerException;
			}

			public override string Message { get { return _message; } }
			public override Exception InnerException { get { return _innerException; } }
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Exception).FullName, "ss.Exception", "Name");
			Assert.IsTrue(typeof(Exception).IsClass, "IsClass");
			Assert.AreEqual(typeof(Exception).BaseType, typeof(object), "BaseType");
			object d = new Exception();
			Assert.IsTrue(d is Exception);

			var interfaces = typeof(Exception).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new Exception();
			Assert.IsTrue((object)ex is Exception, "is Exception");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "An error occurred.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new Exception("The message");
			Assert.IsTrue((object)ex is Exception, "is Exception");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new Exception("The message", inner);
			Assert.IsTrue((object)ex is Exception, "is Exception");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void MessagePropertyCanBeOverridden() {
			var ex = (Exception)new MyException("Test message", null);
			Assert.AreEqual(ex.Message, "Test message");
		}

		[Test]
		public void InnerExceptionPropertyCanBeOverridden() {
			var inner = new Exception("a");
			var ex = (Exception)new MyException("Test message", inner);
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner));
		}
	}
}
