using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class FormatExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(FormatException).FullName, "ss.FormatException", "Name");
			Assert.IsTrue(typeof(FormatException).IsClass, "IsClass");
			Assert.AreEqual(typeof(FormatException).BaseType, typeof(Exception), "BaseType");
			object d = new FormatException();
			Assert.IsTrue(d is FormatException, "is FormatException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(FormatException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new FormatException();
			Assert.IsTrue((object)ex is FormatException, "is FormatException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Invalid format.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new FormatException("The message");
			Assert.IsTrue((object)ex is FormatException, "is FormatException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new FormatException("The message", inner);
			Assert.IsTrue((object)ex is FormatException, "is FormatException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
