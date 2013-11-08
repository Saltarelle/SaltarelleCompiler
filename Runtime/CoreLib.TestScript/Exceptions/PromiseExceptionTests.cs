using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class PromiseExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(PromiseException).FullName, "ss.PromiseException", "Name");
			Assert.IsTrue(typeof(PromiseException).IsClass, "IsClass");
			Assert.AreEqual(typeof(PromiseException).BaseType, typeof(Exception), "BaseType");
			object d = new PromiseException(new object[0]);
			Assert.IsTrue(d is PromiseException, "is PromiseException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(PromiseException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void ArgumentsOnlyConstructorWorks() {
			var args = new object[] { "a", 1 };
			var ex = new PromiseException(args);
			Assert.IsTrue((object)ex is PromiseException, "is PromiseException");
			Assert.AreEqual(ex.Arguments, args, "Arguments");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "a", "Message");
		}

		[Test]
		public void ArgumentsAndMessageConstructorWorks() {
			var args = new object[] { "a", 1 };
			var ex = new PromiseException(args, "Some message");
			Assert.IsTrue((object)ex is PromiseException, "is PromiseException");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Arguments, args, "Arguments");
			Assert.AreEqual(ex.Message, "Some message", "Message");
		}

		[Test]
		public void ArgumentsAndMessageAndInnerExceptionConstructorWorks() {
			var inner = new Exception("a");
			var args = new object[] { "a", 1 };
			var ex = new PromiseException(args, "Some message", inner);
			Assert.IsTrue((object)ex is PromiseException, "is PromiseException");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, inner), "InnerException");
			Assert.AreEqual(ex.Arguments, args, "Arguments");
			Assert.AreEqual(ex.Message, "Some message", "Message");
		}
	}
}
