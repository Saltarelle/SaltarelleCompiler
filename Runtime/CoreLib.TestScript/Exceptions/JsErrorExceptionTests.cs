using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class JsErrorExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(JsErrorException).FullName, "ss.JsErrorException", "Name");
			Assert.IsTrue(typeof(JsErrorException).IsClass, "IsClass");
			Assert.AreEqual(typeof(JsErrorException).BaseType, typeof(Exception), "BaseType");
			object d = new JsErrorException(new Error());
			Assert.IsTrue(d is JsErrorException, "is InvalidOperationException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(JsErrorException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void ErrorOnlyConstructorWorks() {
			var err = new Error { Message = "Some message" };
			var ex = new JsErrorException(err);
			Assert.IsTrue((object)ex is JsErrorException, "is JsErrorException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.IsTrue(ReferenceEquals(ex.Error, err), "Error");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.AreEqual(ex.Stack, err.Stack, "Stack");
		}

		[Test]
		public void ErrorAndMessageConstructorWorks() {
			var err = new Error { Message = "Some message" };
			var ex = new JsErrorException(err, "Overridden message");
			Assert.IsTrue((object)ex is JsErrorException, "is JsErrorException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.IsTrue(ReferenceEquals(ex.Error, err), "Error");
			Assert.AreEqual(ex.Message, "Overridden message", "Message");
			Assert.AreEqual(ex.Stack, err.Stack, "Stack");
		}

		[Test]
		public void ErrorAndMessageAndInnerExceptionConstructorWorks() {
			var inner = new Exception("a");
			var err = new Error { Message = "Some message" };
			var ex = new JsErrorException(err, "Overridden message", inner);
			Assert.IsTrue((object)ex is JsErrorException, "is JsErrorException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.IsTrue(ReferenceEquals(ex.Error, err), "Error");
			Assert.AreEqual(ex.Message, "Overridden message", "Message");
			Assert.AreEqual(ex.Stack, err.Stack, "Stack");
		}
	}
}
