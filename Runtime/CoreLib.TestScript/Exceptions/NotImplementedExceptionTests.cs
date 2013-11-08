using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class NotImplementedExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(NotImplementedException).FullName, "ss.NotImplementedException", "Name");
			Assert.IsTrue(typeof(NotImplementedException).IsClass, "IsClass");
			Assert.AreEqual(typeof(NotImplementedException).BaseType, typeof(Exception), "BaseType");
			object d = new NotImplementedException();
			Assert.IsTrue(d is NotImplementedException, "is NotImplementedException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(NotImplementedException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new NotImplementedException();
			Assert.IsTrue((object)ex is NotImplementedException, "is NotImplementedException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The method or operation is not implemented.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new NotImplementedException("The message");
			Assert.IsTrue((object)ex is NotImplementedException, "is NotImplementedException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new NotImplementedException("The message", inner);
			Assert.IsTrue((object)ex is NotImplementedException, "is NotImplementedException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
