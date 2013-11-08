using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class KeyNotFoundExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(KeyNotFoundException).FullName, "ss.KeyNotFoundException", "Name");
			Assert.IsTrue(typeof(KeyNotFoundException).IsClass, "IsClass");
			Assert.AreEqual(typeof(KeyNotFoundException).BaseType, typeof(Exception), "BaseType");
			object d = new KeyNotFoundException();
			Assert.IsTrue(d is KeyNotFoundException, "is KeyNotFoundException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(KeyNotFoundException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new KeyNotFoundException();
			Assert.IsTrue((object)ex is KeyNotFoundException, "is KeyNotFoundException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Key not found.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new KeyNotFoundException("The message");
			Assert.IsTrue((object)ex is KeyNotFoundException, "is KeyNotFoundException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new KeyNotFoundException("The message", inner);
			Assert.IsTrue((object)ex is KeyNotFoundException, "is KeyNotFoundException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
