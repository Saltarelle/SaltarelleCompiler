using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class ArithmeticExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ArithmeticException).FullName, "ss.ArithmeticException", "Name");
			Assert.IsTrue(typeof(ArithmeticException).IsClass, "IsClass");
			Assert.AreEqual(typeof(ArithmeticException).BaseType, typeof(Exception), "BaseType");
			object d = new ArithmeticException();
			Assert.IsTrue(d is ArithmeticException, "is DivideByZeroException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(ArithmeticException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new ArithmeticException();
			Assert.IsTrue((object)ex is ArithmeticException, "is ArithmeticException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Overflow or underflow in the arithmetic operation.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new ArithmeticException("The message");
			Assert.IsTrue((object)ex is ArithmeticException, "is OverflowException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new ArithmeticException("The message", inner);
			Assert.IsTrue((object)ex is ArithmeticException, "is OverflowException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
