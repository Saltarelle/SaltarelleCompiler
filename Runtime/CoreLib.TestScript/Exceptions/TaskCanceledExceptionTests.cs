using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using QUnit;

namespace CoreLib.TestScript.Exceptions {
	[TestFixture]
	public class TaskCanceledExceptionTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(TaskCanceledException).FullName, "ss.TaskCanceledException", "Name");
			Assert.IsTrue(typeof(TaskCanceledException).IsClass, "IsClass");
			Assert.AreEqual(typeof(TaskCanceledException).BaseType, typeof(OperationCanceledException), "BaseType");
			object d = new TaskCanceledException();
			Assert.IsTrue(d is TaskCanceledException, "is TaskCanceledException");
			Assert.IsTrue(d is OperationCanceledException, "is OperationCanceledException");
			Assert.IsTrue(d is Exception, "is Exception");

			var interfaces = typeof(TaskCanceledException).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var ex = new TaskCanceledException();
			Assert.IsTrue((object)ex is TaskCanceledException, "is TaskCanceledException");
			Assert.AreEqual(ex.Message, "A task was canceled.", "Message");
			Assert.IsNull(ex.Task, "Task");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void MessageOnlyConstructorWorks() {
			var ex = new TaskCanceledException("Some message");
			Assert.IsTrue((object)ex is TaskCanceledException, "is TaskCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsNull(ex.Task, "Task");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void TaskOnlyConstructorWorks() {
			var task = new TaskCompletionSource<int>().Task;
			var ex = new TaskCanceledException(task);
			Assert.IsTrue((object)ex is TaskCanceledException, "is TaskCanceledException");
			Assert.AreEqual(ex.Message, "A task was canceled.", "Message");
			Assert.IsTrue(ReferenceEquals(ex.Task, task), "Task");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsNull(ex.InnerException, "InnerException");
		}

		[Test]
		public void MessageAndInnerExceptionConstructorWorks() {
			var innerException = new Exception();
			var ex = new TaskCanceledException("Some message", innerException);
			Assert.IsTrue((object)ex is TaskCanceledException, "is TaskCanceledException");
			Assert.AreEqual(ex.Message, "Some message", "Message");
			Assert.IsNull(ex.Task, "Task");
			Assert.IsTrue(ReferenceEquals(ex.CancellationToken, CancellationToken.None), "CancellationToken");
			Assert.IsTrue(ReferenceEquals(ex.InnerException, innerException), "InnerException");
		}
	}
}
