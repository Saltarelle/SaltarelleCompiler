using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class NotSupportedExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(NotSupportedException).FullName, "ss.NotSupportedException", "Name");
			Assert.IsTrue(typeof(NotSupportedException).IsClass, "IsClass");
			Assert.AreEqual(typeof(NotSupportedException).BaseType, typeof(Exception), "BaseType");
			object d = new NotSupportedException();
			Assert.IsTrue(d is NotSupportedException, "is NotSupportedException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(NotSupportedException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new NotSupportedException();
			Assert.IsTrue((object)ex is NotSupportedException, "is NotSupportedException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Specified method is not supported.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new NotSupportedException("The message");
			Assert.IsTrue((object)ex is NotSupportedException, "is NotSupportedException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new NotSupportedException("The message", inner);
			Assert.IsTrue((object)ex is NotSupportedException, "is NotSupportedException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
