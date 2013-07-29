using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class AggregateExceptionTests {
		private const string DefaultMessage = "One or more errors occurred.";

		private IEnumerable<T> MakeEnumerable<T>(params T[] arr) {
			foreach (var x in arr)
				yield return x;
		}
		
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(AggregateException).FullName, "ss.AggregateException", "Name");
			Assert.IsTrue(typeof(AggregateException).IsClass, "IsClass");
			Assert.AreEqual(typeof(AggregateException).BaseType, typeof(Exception), "BaseType");
			object d = new AggregateException();
			Assert.IsTrue(d is AggregateException);
			Assert.IsTrue(d is Exception);

			var interfaces = typeof(AggregateException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new AggregateException();
			Assert.IsTrue((object)ex is AggregateException, "is AggregateException");
			Assert.IsTrue((object)ex.InnerExceptions is ReadOnlyCollection<Exception>, "InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex.InnerExceptions.Count, 0, "InnerExceptions.Length");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, DefaultMessage, "Message");
		}

		[Test]
		public void ConstructorWithIEnumerableInnerExceptionsWorks() {
			var inner1 = new Exception("a");
			var inner2 = new Exception("b");

			var ex1 = new AggregateException(MakeEnumerable<Exception>());
			Assert.IsTrue((object)ex1 is AggregateException, "ex1 is AggregateException");
			Assert.IsTrue(ex1.InnerException == null, "ex1 InnerException");
			Assert.IsTrue((object)ex1.InnerExceptions is ReadOnlyCollection<Exception>, "ex1 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex1.InnerExceptions.Count, 0, "ex1 InnerExceptions.Length");
			Assert.AreEqual(ex1.Message, DefaultMessage, "ex1 Message");

			var ex2 = new AggregateException(MakeEnumerable(inner1));
			Assert.IsTrue((object)ex2 is AggregateException, "ex2 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex2.InnerException, inner1), "ex2 InnerException");
			Assert.IsTrue((object)ex2.InnerExceptions is ReadOnlyCollection<Exception>, "ex2 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex2.InnerExceptions.Count, 1, "ex2 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex2.InnerExceptions[0], inner1), "ex2 InnerExceptions[0]");
			Assert.AreEqual(ex2.Message, DefaultMessage, "ex2 Message");

			var ex3 = new AggregateException(MakeEnumerable(inner1, inner2));
			Assert.IsTrue((object)ex3 is AggregateException, "ex3 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex3.InnerException, inner1), "ex3 InnerException");
			Assert.IsTrue((object)ex3.InnerExceptions is ReadOnlyCollection<Exception>, "ex3 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex3.InnerExceptions.Count, 2, "ex3 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[0], inner1), "ex3 InnerExceptions[0]");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[1], inner2), "ex3 InnerExceptions[1]");
			Assert.AreEqual(ex3.Message, DefaultMessage, "ex3 Message");
		}

		[Test]
		public void ConstructorWithInnerExceptionArrayWorks() {
			var inner1 = new Exception("a");
			var inner2 = new Exception("b");

			var ex1 = new AggregateException(new Exception[0]);
			Assert.IsTrue((object)ex1 is AggregateException, "ex1 is AggregateException");
			Assert.IsTrue(ex1.InnerException == null, "ex1 InnerException");
			Assert.IsTrue((object)ex1.InnerExceptions is ReadOnlyCollection<Exception>, "ex1 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex1.InnerExceptions.Count, 0, "ex1 InnerExceptions.Length");
			Assert.AreEqual(ex1.Message, DefaultMessage, "ex1 Message");

			var ex2 = new AggregateException(inner1);
			Assert.IsTrue((object)ex2 is AggregateException, "ex2 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex2.InnerException, inner1), "ex2 InnerException");
			Assert.IsTrue((object)ex2.InnerExceptions is ReadOnlyCollection<Exception>, "ex2 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex2.InnerExceptions.Count, 1, "ex2 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex2.InnerExceptions[0], inner1), "ex2 InnerExceptions[0]");
			Assert.AreEqual(ex2.Message, DefaultMessage, "ex2 Message");

			var ex3 = new AggregateException(inner1, inner2);
			Assert.IsTrue((object)ex3 is AggregateException, "ex3 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex3.InnerException, inner1), "ex3 InnerException");
			Assert.IsTrue((object)ex3.InnerExceptions is ReadOnlyCollection<Exception>, "ex3 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex3.InnerExceptions.Count, 2, "ex3 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[0], inner1), "ex3 InnerExceptions[0]");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[1], inner2), "ex3 InnerExceptions[1]");
			Assert.AreEqual(ex3.Message, DefaultMessage, "ex3 Message");
		}

		[Test]
		public void ConstructorWithMessageWorks() {
			var ex = new AggregateException("Some message");
			Assert.IsTrue((object)ex is AggregateException, "is AggregateException");
			Assert.IsTrue((object)ex.InnerExceptions is ReadOnlyCollection<Exception>, "ex1 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex.InnerExceptions.Count, 0, "InnerExceptions.Length");
			Assert.IsTrue(ex.InnerException == null, "InnerException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
		}

		[Test]
		public void ConstructorWithMessageAndIEnumerableInnerExceptionsWorks() {
			var inner1 = new Exception("a");
			var inner2 = new Exception("b");

			var ex1 = new AggregateException("Message #1", MakeEnumerable<Exception>());
			Assert.IsTrue((object)ex1 is AggregateException, "ex1 is AggregateException");
			Assert.IsTrue(ex1.InnerException == null, "ex1 InnerException");
			Assert.IsTrue((object)ex1.InnerExceptions is ReadOnlyCollection<Exception>, "ex1 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex1.InnerExceptions.Count, 0, "ex1 InnerExceptions.Length");
			Assert.AreEqual(ex1.Message, "Message #1", "ex1 Message");

			var ex2 = new AggregateException("Message #2", MakeEnumerable(inner1));
			Assert.IsTrue((object)ex2 is AggregateException, "ex2 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex2.InnerException, inner1), "ex2 InnerException");
			Assert.IsTrue((object)ex2.InnerExceptions is ReadOnlyCollection<Exception>, "ex2 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex2.InnerExceptions.Count, 1, "ex2 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex2.InnerExceptions[0], inner1), "ex2 InnerExceptions[0]");
			Assert.AreEqual(ex2.Message, "Message #2", "ex2 Message");

			var ex3 = new AggregateException("Message #3", MakeEnumerable(inner1, inner2));
			Assert.IsTrue((object)ex3 is AggregateException, "ex3 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex3.InnerException, inner1), "ex3 InnerException");
			Assert.IsTrue((object)ex3.InnerExceptions is ReadOnlyCollection<Exception>, "ex3 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex3.InnerExceptions.Count, 2, "ex3 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[0], inner1), "ex3 InnerExceptions[0]");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[1], inner2), "ex3 InnerExceptions[1]");
			Assert.AreEqual(ex3.Message, "Message #3", "ex3 Message");
		}

		[Test]
		public void ConstructorWithMessageAndInnerExceptionArrayWorks() {
			var inner1 = new Exception("a");
			var inner2 = new Exception("b");

			var ex1 = new AggregateException("Message #1", new Exception[0]);
			Assert.IsTrue((object)ex1 is AggregateException, "ex1 is AggregateException");
			Assert.IsTrue(ex1.InnerException == null, "ex1 InnerException");
			Assert.IsTrue((object)ex1.InnerExceptions is ReadOnlyCollection<Exception>, "ex1 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex1.InnerExceptions.Count, 0, "ex1 InnerExceptions.Length");
			Assert.AreEqual(ex1.Message, "Message #1", "ex1 Message");

			var ex2 = new AggregateException("Message #2", inner1);
			Assert.IsTrue((object)ex2 is AggregateException, "ex2 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex2.InnerException, inner1), "ex2 InnerException");
			Assert.AreEqual(ex2.InnerExceptions.Count, 1, "ex2 InnerExceptions.Length");
			Assert.IsTrue((object)ex2.InnerExceptions is ReadOnlyCollection<Exception>, "ex2 InnerExceptions is ReadOnlyCollection");
			Assert.IsTrue(ReferenceEquals(ex2.InnerExceptions[0], inner1), "ex2 InnerExceptions[0]");
			Assert.AreEqual(ex2.Message, "Message #2", "ex2 Message");

			var ex3 = new AggregateException("Message #3", inner1, inner2);
			Assert.IsTrue((object)ex3 is AggregateException, "ex3 is AggregateException");
			Assert.IsTrue(ReferenceEquals(ex3.InnerException, inner1), "ex3 InnerException");
			Assert.IsTrue((object)ex3.InnerExceptions is ReadOnlyCollection<Exception>, "ex3 InnerExceptions is ReadOnlyCollection");
			Assert.AreEqual(ex3.InnerExceptions.Count, 2, "ex3 InnerExceptions.Length");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[0], inner1), "ex3 InnerExceptions[0]");
			Assert.IsTrue(ReferenceEquals(ex3.InnerExceptions[1], inner2), "ex3 InnerExceptions[1]");
			Assert.AreEqual(ex3.Message, "Message #3", "ex3 Message");
		}
	}
}
