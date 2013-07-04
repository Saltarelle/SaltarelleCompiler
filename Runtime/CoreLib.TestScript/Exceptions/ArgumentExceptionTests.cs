using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class ArgumentExceptionTests {
		private const string DefaultMessage = "Value does not fall within the expected range.";

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ArgumentException).FullName, "ss.ArgumentException", "Name");
			Assert.IsTrue(typeof(ArgumentException).IsClass, "IsClass");
			Assert.AreEqual(typeof(ArgumentException).BaseType, typeof(Exception), "BaseType");
			object d = new ArgumentException();
			Assert.IsTrue(d is ArgumentException);
			Assert.IsTrue(d is Exception);

			var interfaces = typeof(ArgumentException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new ArgumentException();
			Assert.IsTrue((object)ex is ArgumentException, "is ArgumentException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, DefaultMessage);
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new ArgumentException("The message");
			Assert.IsTrue((object)ex is ArgumentException, "is ArgumentException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new ArgumentException("The message", inner);
			Assert.IsTrue((object)ex is ArgumentException, "is ArgumentException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndParamNameWorks() {
			var ex = new ArgumentException("The message", "someParam");
			Assert.IsTrue((object)ex is ArgumentException, "is ArgumentException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndParamNameAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new ArgumentException("The message", "someParam", inner);
			Assert.IsTrue((object)ex is ArgumentException, "is ArgumentException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
