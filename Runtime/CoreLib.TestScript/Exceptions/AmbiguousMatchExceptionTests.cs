using System;
using System.Reflection;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class AmbiguousMatchExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(AmbiguousMatchException).FullName, "ss.AmbiguousMatchException", "Name");
			Assert.IsTrue(typeof(AmbiguousMatchException).IsClass, "IsClass");
			Assert.AreEqual(typeof(AmbiguousMatchException).BaseType, typeof(Exception), "BaseType");
			object d = new AmbiguousMatchException();
			Assert.IsTrue(d is AmbiguousMatchException, "is AmbiguousMatchException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(AmbiguousMatchException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new AmbiguousMatchException();
			Assert.IsTrue((object)ex is AmbiguousMatchException, "is AmbiguousMatchException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Ambiguous match.");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new AmbiguousMatchException("The message");
			Assert.IsTrue((object)ex is AmbiguousMatchException, "is AmbiguousMatchException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionWorks() {
			var inner = new Exception("a");
			var ex = new AmbiguousMatchException("The message", inner);
			Assert.IsTrue((object)ex is AmbiguousMatchException, "is AmbiguousMatchException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Message, "The message");
		}
	}
}
