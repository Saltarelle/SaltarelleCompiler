using System;
using System.Threading.Tasks;
using QUnit;

namespace CoreLib.TestScript.Threading.Tasks {
	[TestFixture]
	public class AsyncTests {
#if !NO_ASYNC
		[Test(IsAsync = true)]
		public void AsyncVoid() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;

			Action someMethod = async () => {
				state = 1;
				await task;
				state = 2;
			};
			someMethod();
			Assert.AreEqual(state, 1, "Async method should start running after being invoked");

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1, "Async method should not continue past point 1 until task is finished");
			}, 100);

			Globals.SetTimeout(() => {
				tcs.SetResult(0);
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 2, "Async method should finish after the task is finished");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void AsyncTask() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;

			Func<Task> someMethod = async () => {
				state = 1;
				await task;
				state = 2;
			};
			Task asyncTask = someMethod();
			Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running immediately");
			Assert.AreEqual(state, 1, "Async method should start running after being invoked");

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running before awaited task is finished");
				Assert.AreEqual(state, 1, "Async method should not continue past point 1 until task is finished");
			}, 100);

			Globals.SetTimeout(() => {
				tcs.SetResult(0);
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.RanToCompletion, "asyncTask should run to completion");
				Assert.IsTrue(asyncTask.Exception == null, "asyncTask should not throw an exception");
				Assert.AreEqual(state, 2, "Async method should finish after the task is finished");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void AsyncTaskBodyThrowsException() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			var ex = new Exception("Some text");

			Func<Task> someMethod = async () => {
				state = 1;
				await task;
				state = 2;
				throw ex;
			};
			Task asyncTask = someMethod();
			Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running immediately");
			Assert.AreEqual(state, 1, "Async method should start running after being invoked");

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running before awaited task is finished");
				Assert.AreEqual(state, 1, "Async method should not continue past point 1 until task is finished");
			}, 100);

			Globals.SetTimeout(() => {
				tcs.SetResult(0);
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Faulted, "asyncTask should fault");
				Assert.IsTrue(asyncTask.Exception != null, "asyncTask should have an exception");
				Assert.IsTrue(asyncTask.Exception.InnerExceptions[0] == ex, "asyncTask should throw the correct exception");
				Assert.AreEqual(state, 2, "Async method should finish after the task is finished");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void AwaitTaskThatFaults() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			var ex = new Exception("Some text");

			Func<Task> someMethod = async () => {
				state = 1;
				await task;
				state = 2;
			};
			Task asyncTask = someMethod();
			Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running immediately");
			Assert.AreEqual(state, 1, "Async method should start running after being invoked");

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running before awaited task is finished");
				Assert.AreEqual(state, 1, "Async method should not continue past point 1 until task is finished");
			}, 100);

			Globals.SetTimeout(() => {
				tcs.SetException(ex);
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Faulted, "asyncTask should fault");
				Assert.IsTrue(asyncTask.Exception != null, "asyncTask should have an exception");
				Assert.IsTrue(asyncTask.Exception.InnerExceptions[0] == ex, "asyncTask should throw the correct exception");
				Assert.AreEqual(state, 1, "Async method should not have reach anything after the faulting await");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void AggregateExceptionsAreUnwrappedWhenAwaitingTask() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			var ex = new Exception("Some text");
			tcs.SetException(ex);

			Func<Task> someMethod = async () => {
				try {
					await task;
					Assert.Fail("Await should have thrown");
				}
				catch (Exception ex2) {
					Assert.IsTrue(ReferenceEquals(ex, ex2), "The exception should be correct");
				}
				state = 1;
			};
			someMethod();

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1, "Should have reached the termination state");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void AsyncTaskThatReturnsValue() {
			int state = 0;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;

			Func<Task<int>> someMethod = async () => {
				state = 1;
				await task;
				state = 2;
				return 42;
			};
			Task<int> asyncTask = someMethod();
			Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running immediately");
			Assert.AreEqual(state, 1, "Async method should start running after being invoked");

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.Running, "asyncTask should be running before awaited task is finished");
				Assert.AreEqual(state, 1, "Async method should not continue past point 1 until task is finished");
			}, 100);

			Globals.SetTimeout(() => {
				tcs.SetResult(0);
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(asyncTask.Status, TaskStatus.RanToCompletion, "asyncTask should run to completion");
				Assert.IsTrue(asyncTask.Exception == null, "asyncTask should not throw an exception");
				Assert.AreEqual(state, 2, "Async method should finish after the task is finished");
				Assert.AreEqual(asyncTask.Result, 42, "Result should be correct");
				Engine.Start();
			}, 300);
		}
#endif
	}
}
