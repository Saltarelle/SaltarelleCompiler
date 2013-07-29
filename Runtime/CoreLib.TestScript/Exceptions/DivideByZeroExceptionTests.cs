using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class DivideByZeroExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(DivideByZeroException).FullName, "ss.DivideByZeroException", "Name");
			Assert.IsTrue(typeof(DivideByZeroException).IsClass, "IsClass");
			Assert.AreEqual(typeof(DivideByZeroException).BaseType, typeof(Exception), "BaseType");
			object d = new DivideByZeroException();
			Assert.IsTrue(d is DivideByZeroException, "is DivideByZeroException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(DivideByZeroException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new DivideByZeroException();
			Assert.IsTrue((object)ex is DivideByZeroException, "is DivideByZeroException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Invalid format.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new DivideByZeroException("The message");
			Assert.IsTrue((object)ex is DivideByZeroException, "is DivideByZeroException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new DivideByZeroException("The message", inner);
			Assert.IsTrue((object)ex is DivideByZeroException, "is DivideByZeroException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
