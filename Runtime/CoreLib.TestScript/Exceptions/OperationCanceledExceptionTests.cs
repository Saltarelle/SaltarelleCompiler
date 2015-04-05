using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class OperationCanceledExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(OperationCanceledException).FullName, "ss.OperationCanceledException", "Name");
			Assert.IsTrue(typeof(OperationCanceledException).IsClass, "IsClass");
			Assert.AreEqual(typeof(OperationCanceledException).BaseType, typeof(Exception), "BaseType");
			object d = new OperationCanceledException();
			Assert.IsTrue(d is OperationCanceledException, "is OperationCanceledException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(OperationCanceledException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new OperationCanceledException();
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Operation was canceled.", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void CancellationTokenOnlyConstructorWorks() {
			var ct = new CancellationToken();
			var ex = new OperationCanceledException(ct);
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Operation was canceled.", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, ct), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void MessageOnlyConstructorWorks() {
			var ex = new OperationCanceledException("Some message");
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void MessageAndInnerExceptionConstructorWorks() {
			var innerException = new Exception();
			var ex = new OperationCanceledException("Some message", innerException);
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, innerException), "InnerException");
		}

		[Test]
		public void MessageAndCancellationTokenConstructorWorks() {
			var ct = new CancellationToken();
			var ex = new OperationCanceledException("Some message", ct);
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, ct), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void MessageAndInnerExceptionAndCancellationTokenConstructorWorks() {
			var ct = new CancellationToken();
			var innerException = new Exception();
			var ex = new OperationCanceledException("Some message", innerException, ct);
			Assert.IsTrue((object)ex is OperationCanceledException, "is OperationCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, ct), "CancellationToken");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, innerException), "InnerException");
		}
	}
}
