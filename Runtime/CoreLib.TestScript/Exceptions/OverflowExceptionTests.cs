using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class OverflowExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(OverflowException).FullName, "ss.OverflowException", "Name");
			Assert.IsTrue(typeof(OverflowException).IsClass, "IsClass");
			Assert.AreEqual(typeof(OverflowException).BaseType, typeof(ArithmeticException), "BaseType");
			object d = new OverflowException();
			Assert.IsTrue(d is OverflowException, "is OverflowException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(OverflowException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new OverflowException();
			Assert.IsTrue((object)ex is OverflowException, "is OverflowException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Arithmetic operation resulted in an overflow.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new OverflowException("The message");
			Assert.IsTrue((object)ex is OverflowException, "is OverflowException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new OverflowException("The message", inner);
			Assert.IsTrue((object)ex is OverflowException, "is OverflowException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
