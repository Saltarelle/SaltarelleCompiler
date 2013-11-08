using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class ArgumentOutOfRangeExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ArgumentOutOfRangeException).FullName, "ss.ArgumentOutOfRangeException", "Name");
			Assert.IsTrue(typeof(ArgumentOutOfRangeException).IsClass, "IsClass");
			Assert.AreEqual(typeof(ArgumentOutOfRangeException).BaseType, typeof(ArgumentException), "BaseType");
			object d = new ArgumentOutOfRangeException();
			Assert.IsTrue(d is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.IsTrue(d is ArgumentException, "is ArgumentException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(ArgumentOutOfRangeException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new ArgumentOutOfRangeException();
			Assert.IsTrue((object)ex is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.IsTrue(ex.ActualValue == null, "ActualValue");
			Assert.AreEqual(ex.Message, "Value is out of range.");
		}

		[Test]
		public void ConstructorWithParamNameWorks() {
			var ex = new ArgumentOutOfRangeException("someParam");
			Assert.IsTrue((object)ex is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.IsTrue(ex.ActualValue == null, "ActualValue");
			Assert.AreEqual(ex.Message, "Value is out of range.\nParameter name: someParam");
		}

		[Test]
		public void ConstructorWithParamNameAndMessageWorks() {
			var ex = new ArgumentOutOfRangeException("someParam", "The message");
			Assert.IsTrue((object)ex is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.IsTrue(ex.ActualValue == null, "ActualValue");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new ArgumentOutOfRangeException("The message", inner);
			Assert.IsTrue((object)ex is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.IsTrue(ex.ParamName == null, "ParamName");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.IsTrue(ex.ActualValue == null, "ActualValue");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithParamNameAndActualValueAndMessageWorks() {
			var ex = new ArgumentOutOfRangeException("someParam", 42, "The message");
			Assert.IsTrue((object)ex is ArgumentOutOfRangeException, "is ArgumentOutOfRangeException");
			Assert.AreEqual(ex.ParamName, "someParam", "ParamName");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.ActualValue, 42, "ActualValue");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test(ExpectedAssertionCount = 0)]
		public void RangeErrorIsConvertedToArgumentOutOfRangeException() {
			int size = -1;
			try {
				#pragma warning disable 219
				var arr = new int[size];
				#pragma warning restore 219
				Assert.Fail("Should throw");
			}
			catch (ArgumentOutOfRangeException) {
			}
			catch (Exception ex) {
				Assert.Fail("Expected ArgumentOutOfRangeException, got " + ex.GetType());
			}
		}
	}
}
