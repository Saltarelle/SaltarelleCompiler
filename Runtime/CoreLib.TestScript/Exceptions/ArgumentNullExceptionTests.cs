using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class ArgumentNullExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ArgumentNullException).FullName, "ss.ArgumentNullException", "Name");
			Assert.IsTrue(typeof(ArgumentNullException).IsClass, "IsClass");
			Assert.AreEqual(typeof(ArgumentNullException).BaseType, typeof(ArgumentException), "BaseType");
			object d = new ArgumentNullException();
			Assert.IsTrue(d is ArgumentNullException, "is ArgumentNullException");
			Assert.IsTrue(d is ArgumentException, "is ArgumentException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(ArgumentNullException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new ArgumentNullException();
			Assert.IsTrue((object)ex is ArgumentNullException, "is ArgumentNullException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Value cannot be null.");
		}

		[Test]
		public void ConstructorWithParamNameWorks() {
			var ex = new ArgumentNullException("someParam");
			Assert.IsTrue((object)ex is ArgumentNullException, "is ArgumentNullException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Value cannot be null.\nParameter name: someParam");
		}

		[Test]
		public void ConstructorWithParamNameAndMessageWorks() {
			var ex = new ArgumentNullException("someParam", "The message");
			Assert.IsTrue((object)ex is ArgumentNullException, "is ArgumentNullException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new ArgumentNullException("The message", inner);
			Assert.IsTrue((object)ex is ArgumentNullException, "is ArgumentException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
