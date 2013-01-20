using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;
using System.Threading.Tasks;

namespace CoreLib.TestScript {
	[TestFixture]
	public class TaskTests {
		private IEnumerable<T> MakeEnumerable<T>(params T[] args) {
			foreach (var a in args)
				yield return a;
		}
		
		[Test]
		public void TaskCompletionSourceTypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(TaskCompletionSource<int>).FullName, "ss.TaskCompletionSource", "FullName should be correct");
			Assert.IsTrue(typeof(TaskCompletionSource<int>).IsClass, "IsClass should be true");
			var tcs = new TaskCompletionSource<int>();
			Assert.IsTrue(tcs is TaskCompletionSource<int>);
		}

		[Test]
		public void TaskTypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Task).FullName, "ss.Task", "FullName for non-generic task should be correct");
			Assert.IsTrue(typeof(Task).IsClass, "IsClass for non-generic task should be true");
			Assert.AreEqual(typeof(Task<int>).FullName, "ss.Task", "FullName for generic task should be correct");
			Assert.IsTrue(typeof(Task<int>).IsClass, "IsClass for generic task should be true");
			
			var task = new TaskCompletionSource<int>().Task;
			Assert.IsTrue(task is Task<int>);
			Assert.IsTrue(task is Task);
			Assert.IsTrue(task is IDisposable);

			task.Dispose();	// Should not throw
		}

		[Test(IsAsync = true)]
		public void TaskCompletionSourceWorksWhenSettingResult() {
			bool callbackRun = false;
			var tcs = new TaskCompletionSource<int>();
			var task = tcs.Task;
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed when in the callback");
				Assert.AreEqual(task.Result, 1, "Result should be 1 after the callback");
				Assert.AreEqual(task.Exception, null, "Exception should be null in the callback");
				callbackRun = true;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "The task should be running before SetResult is called");
			Assert.IsFalse(callbackRun, "Callback should not be run before SetResult() is called");

			tcs.SetResult(1);
			Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed directly after SetResult() is called");
			Assert.AreEqual(task.Result, 1, "Result should be set immediately");
			Assert.AreEqual(task.Exception, null, "Exception should be null after SetResult()");

			Globals.SetTimeout(() => {
				Assert.IsTrue(callbackRun, "Callback should be run");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void TaskCompletionSourceWorksWhenSettingASingleException() {
			bool callbackRun = false;
			var tcs = new TaskCompletionSource<int>();
			var task = tcs.Task;
			var ex = new Exception("Some text");
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Task should be faulted in the callback");
				Assert.IsTrue((object)task.Exception is AggregateException);
				Assert.IsTrue(task.Exception.InnerExceptions[0] == ex, "The exception should be correct");
				Assert.Throws(() => { var x = task.Result; }, "Getting the result property in the callback should throw");
				callbackRun = true;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "The task should be running before the SetException() call");
			Assert.IsFalse(callbackRun, "Callback should not be run before SetException() is called");

			tcs.SetException(ex);
			Assert.AreEqual(task.Status, TaskStatus.Faulted, "The task should be faulted immediately after the SetException() call");
			Assert.IsTrue((object)task.Exception is AggregateException);
			Assert.IsTrue(task.Exception.InnerExceptions[0] == ex, "The exception should be correct immediately after SetException()");
			Assert.Throws(() => { var x = task.Result; }, "Getting the result property after SetException() should throw");

			Globals.SetTimeout(() => {
				Assert.IsTrue(callbackRun, "Callback should be run");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void TaskCompletionSourceWorksWhenSettingTwoExceptions() {
			bool callbackRun = false;
			var tcs = new TaskCompletionSource<int>();
			var task = tcs.Task;
			var ex1 = new Exception("Some text");
			var ex2 = new Exception("Some other text");
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Task should be faulted in the callback");
				Assert.IsTrue((object)task.Exception is AggregateException);
				Assert.IsTrue(task.Exception.InnerExceptions[0] == ex1, "InnerExceptions[0] should be correct in callback");
				Assert.IsTrue(task.Exception.InnerExceptions[1] == ex2, "InnerExceptions[1] should be correct in callback");
				Assert.Throws(() => { var x = task.Result; }, "Getting the result property in the callback should throw");
				callbackRun = true;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "The task should be running before the SetException() call");
			Assert.IsFalse(callbackRun, "Callback should not be run before SetException() is called");

			tcs.SetException(MakeEnumerable(new[] { ex1, ex2 }));
			Assert.AreEqual(task.Status, TaskStatus.Faulted, "The task should be faulted immediately after the SetException() call");
			Assert.IsTrue((object)task.Exception is AggregateException);
			Assert.IsTrue(task.Exception.InnerExceptions[0] == ex1, "InnerExceptions[0] should be correct immediately after SetException");
			Assert.IsTrue(task.Exception.InnerExceptions[1] == ex2, "InnerExceptions[1] should be correct immediately after SetException");
			Assert.Throws(() => { var x = task.Result; }, "Getting the result property after SetException() should throw");

			Globals.SetTimeout(() => {
				Assert.IsTrue(callbackRun, "Callback should be run");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void TaskCompletionSourceWorksWhenCancelling() {
			bool callbackRun = false;
			var tcs = new TaskCompletionSource<int>();
			var task = tcs.Task;
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.Canceled, "Task should be cancelled in the callback");
				Assert.IsTrue(task.Exception == null, "Exception should be null in the callback");
				Assert.Throws(() => { var x = task.Result; }, "Getting the result property in the callback should throw");
				callbackRun = true;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "The task should be running before the SetCanceled() call");
			Assert.IsFalse(callbackRun, "Callback should not be run before SetCanceled() is called");

			tcs.SetCanceled();
			Assert.AreEqual(task.Status, TaskStatus.Canceled, "The task should be cancelled immediately after the SetCanceled() call");
			Assert.IsTrue(task.Exception == null, "The exception should be correct immediately after SetCanceled()");
			Assert.Throws(() => { var x = task.Result; }, "Getting the result property after SetCanceled() should throw");

			Globals.SetTimeout(() => {
				Assert.IsTrue(callbackRun, "The callback should be run");
				Engine.Start();
			}, 100);
		}

		[Test]
		public void SetResultFailsWhenTheTaskIsCompleted() {
			var tcs = new TaskCompletionSource<int>();
			tcs.SetResult(1);
			Assert.Throws(() => tcs.SetResult(1));
		}

		[Test]
		public void SetCanceledFailsWhenTheTaskIsCompleted() {
			var tcs = new TaskCompletionSource<int>();
			tcs.SetCanceled();
			Assert.Throws(() => tcs.SetCanceled());
		}

		[Test]
		public void SetExceptionFailsWhenTheTaskIsCompleted() {
			var ex = new Exception();
			var tcs = new TaskCompletionSource<int>();
			tcs.SetException(ex);
			Assert.Throws(() => tcs.SetException(ex));
		}

		[Test]
		public void CompletedTaskHasCorrectIsXProperties() {
			var tcs = new TaskCompletionSource<int>();
			tcs.SetResult(1);
			Assert.IsTrue(tcs.Task.IsCompleted);
			Assert.IsFalse(tcs.Task.IsFaulted);
			Assert.IsFalse(tcs.Task.IsCanceled);
		}

		[Test]
		public void CancelledTaskHasCorrectIsXProperties() {
			var tcs = new TaskCompletionSource<int>();
			tcs.SetCanceled();
			Assert.IsTrue(tcs.Task.IsCompleted);
			Assert.IsFalse(tcs.Task.IsFaulted);
			Assert.IsTrue(tcs.Task.IsCanceled);
		}

		[Test]
		public void FaultedTaskHasCorrectIsXProperties() {
			var tcs = new TaskCompletionSource<int>();
			tcs.SetException(new Exception());
			Assert.IsTrue(tcs.Task.IsCompleted);
			Assert.IsTrue(tcs.Task.IsFaulted);
			Assert.IsFalse(tcs.Task.IsCanceled);
		}

		[Test]
		public void TrySetResultReturnsFalseWhenTheTaskIsCompleted() {
			var tcs = new TaskCompletionSource<int>();
			Assert.IsTrue(tcs.TrySetResult(1));
			Assert.IsFalse(tcs.TrySetResult(1));
		}

		[Test]
		public void TrySetCanceledReturnsFalseWhenTheTaskIsCompleted() {
			var tcs = new TaskCompletionSource<int>();
			Assert.IsTrue(tcs.TrySetCanceled());
			Assert.IsFalse(tcs.TrySetCanceled());
		}

		[Test]
		public void TrySetExceptionReturnsFalseWhenTheTaskIsCompleted() {
			var ex = new Exception();
			var tcs = new TaskCompletionSource<int>();
			Assert.IsTrue(tcs.TrySetException(ex));
			Assert.IsFalse(tcs.TrySetException(ex));
		}

		[Test(IsAsync = true)]
		public void ContinueWithForNonGenericTaskWorkWithNoResultAndNoException() {
			bool done = false;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			Task continuedTask = null;
			continuedTask = task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Exception, null, "task should not have an exception");

				Assert.AreEqual(continuedTask.Status, TaskStatus.Running, "continuedTask should be running at point 2");
			});
			Assert.IsFalse(task == continuedTask, "task and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.RanToCompletion, "continuedTask should have run to completion at point 3");
				Assert.AreEqual(continuedTask.Exception, null, "continuedTask should not have an exception");

				done = true;
			});

			tcs.SetResult(0);

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void ExceptionInTaskBodyAppearsInTheExceptionMemberForNonGenericTask() {
			bool done = false;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			Task continuedTask = null;
			continuedTask = task.ContinueWith(t => {
				Script.Eval("throw 'This is a test message'");
			});
			Assert.IsFalse(task == continuedTask, "task and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.Faulted, "continuedTask should have run to completion at point 3");
				Assert.AreNotEqual(continuedTask.Exception, null, "continuedTask should have an exception");
				Assert.IsTrue((object)continuedTask.Exception is AggregateException);
				Assert.AreEqual(continuedTask.Exception.InnerExceptions[0].Message, "This is a test message");

				done = true;
			});

			tcs.SetResult(0);

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void ContinueWithForNonGenericTaskCanReturnAValue() {
			bool done = false;
			var tcs = new TaskCompletionSource<int>();
			Task task = tcs.Task;
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			Task<int> continuedTask = null;
			continuedTask = task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Exception, null, "task should not have an exception");

				Assert.AreEqual(continuedTask.Status, TaskStatus.Running, "continuedTask should be running at point 2");

				return 42;
			});
			Assert.IsFalse(task == continuedTask, "task and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.RanToCompletion, "continuedTask should have run to completion at point 3");
				Assert.AreEqual(continuedTask.Exception, null, "continuedTask should not have an exception");
				Assert.AreEqual(t.Result, 42);

				done = true;
			});

			tcs.SetResult(0);

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void ContinueWithWithNoReturnValueForGenericTaskWorks() {
			bool done = false;
			var tcs = new TaskCompletionSource<int>();
			Task<int> task = tcs.Task;
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			Task continuedTask = null;
			continuedTask = task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Exception, null, "task should not have an exception");

				Assert.AreEqual(continuedTask.Status, TaskStatus.Running, "continuedTask should be running at point 2");
			});
			Assert.IsFalse(task == continuedTask, "task and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.RanToCompletion, "continuedTask should have run to completion at point 3");
				Assert.AreEqual(continuedTask.Exception, null, "continuedTask should not have an exception");

				done = true;
			});

			tcs.SetResult(0);

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void ContinueWithForGenericTaskCanReturnAValue() {
			bool done = false;
			var tcs = new TaskCompletionSource<int>();
			Task<int> task = tcs.Task;
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			Task<string> continuedTask = null;
			continuedTask = task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Exception, null, "task should not have an exception");

				Assert.AreEqual(continuedTask.Status, TaskStatus.Running, "continuedTask should be running at point 2");

				return t.Result + "_";
			});
			Assert.IsFalse((object)task == (object)continuedTask, "task and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.RanToCompletion, "continuedTask should have run to completion at point 3");
				Assert.AreEqual(continuedTask.Exception, null, "continuedTask should not have an exception");
				Assert.AreEqual(t.Result, "42_");

				done = true;
			});

			tcs.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void DelayWorks() {
			bool done = false;

			Globals.SetTimeout(() => {
				Assert.IsFalse(done, "Done should not be set too early");
			}, 50);

			var delay = Task.Delay(100);
			Assert.AreEqual(delay.Status, TaskStatus.Running, "delay should be running at point 1");
			
			delay.ContinueWith(t => {
				Assert.IsTrue(t == delay, "argument to delay.ContinueWith callback should be correct");
				Assert.AreEqual(delay.Status, TaskStatus.RanToCompletion, "delay should have run to completion at point 2");
				Assert.AreEqual(delay.Exception, null, "delay should not have an exception");
				done = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test]
		public void FromResultWorks() {
			var t = Task.FromResult(3);
			Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Task should be finished");
			Assert.AreEqual(t.Result, 3, "Result should be correct");
			Assert.AreEqual(t.Exception, null, "Exception should be null");
		}

		[Test(IsAsync = true)]
		public void RunWithoutResultWorks() {
			bool bodyRun = false, continuationRun = false;

			var task = Task.Run(() => {
				bodyRun = true;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.IsTrue(bodyRun, "Body should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Exception, null, "task should not have an exception");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void RunWithResultWorks() {
			bool bodyRun = false, continuationRun = false;

			var task = Task.Run(() => {
				bodyRun = true;
				return 42;
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.IsTrue(bodyRun, "Body should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "task should have run to completion at point 2");
				Assert.AreEqual(task.Result, 42);
				Assert.AreEqual(task.Exception, null, "task should not have an exception");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void RunWorksWhenBodyThrows() {
			bool bodyRun = false, continuationRun = false;

			var task = Task.Run(() => {
				bodyRun = true;
				Script.Eval("throw 'This is a test message'");
			});
			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running at point 1");
			
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "argument to task.ContinueWith callback should be correct");
				Assert.IsTrue(bodyRun, "Body should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "task should have faulted at point 2");
				Assert.IsTrue((object)task.Exception is AggregateException);
				Assert.AreEqual(task.Exception.InnerExceptions[0].Message, "This is a test message");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllParamArrayWithResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var task = Task.WhenAll(tcs1.Task, tcs2.Task, tcs3.Task);
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.RanToCompletion, "Task1 should have run to completion");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.RanToCompletion, "Task3 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Result, new[] { 101, 3, 42 }, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);
			tcs1.SetResult(101);
			tcs3.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllEnumerableWithResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var task = Task.WhenAll(MakeEnumerable(tcs1.Task, tcs2.Task, tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.RanToCompletion, "Task1 should have run to completion");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.RanToCompletion, "Task3 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Result, new[] { 101, 3, 42 }, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);
			tcs1.SetResult(101);
			tcs3.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllParamArrayWithoutResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var task = Task.WhenAll((Task)tcs1.Task, (Task)tcs2.Task, (Task)tcs3.Task);
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.RanToCompletion, "Task1 should have run to completion");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.RanToCompletion, "Task3 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);
			tcs1.SetResult(101);
			tcs3.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllEnumerableWithoutResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var task = Task.WhenAll(MakeEnumerable((Task)tcs1.Task, (Task)tcs2.Task, (Task)tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.RanToCompletion, "Task1 should have run to completion");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.RanToCompletion, "Task3 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);
			tcs1.SetResult(101);
			tcs3.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllShouldHaveAnErrorIfAnyIncludedTaskFaulted() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			var tcs4 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs4.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var ex1 = new Exception("exception 1");
			var ex2 = new Exception("exception 1");

			var task = Task.WhenAll(MakeEnumerable((Task)tcs1.Task, (Task)tcs2.Task, (Task)tcs3.Task, (Task)tcs4.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.RanToCompletion, "Task1 should have run to completion");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.Faulted, "Task2 should be faulted");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.Faulted, "Task3 should be faulted");
				Assert.AreEqual(tcs4.Task.Status, TaskStatus.Canceled, "Task4 should be cancelled");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.IsTrue(t.Exception is AggregateException, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Exception.InnerExceptions.Length, 2, "Should be 2 inner exceptions");
				Assert.IsTrue(t.Exception.InnerExceptions.Contains(ex1), "ex1 should be propagated");
				Assert.IsTrue(t.Exception.InnerExceptions.Contains(ex2), "ex2 should be propagated");
				Assert.AreEqual(t.Status, TaskStatus.Faulted, "Aggregate task should be faulted");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetException(ex1);
			tcs1.SetResult(101);
			tcs3.SetException(ex2);
			tcs4.SetCanceled();

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAllShouldBeCancelledIfNoTaskWasFaultedButSomeWasCancelled() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			tcs1.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs2.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });
			tcs3.Task.ContinueWith(_ => { Assert.IsFalse(continuationRun, "Continuation should not be run too early."); });

			var task = Task.WhenAll(tcs1.Task, tcs2.Task, tcs3.Task);
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs1.Task.Status, TaskStatus.Canceled, "Task1 should be cancelled");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");
				Assert.AreEqual(tcs3.Task.Status, TaskStatus.RanToCompletion, "Task3 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.Canceled, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);
			tcs1.SetCanceled();
			tcs3.SetResult(42);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 100);
		}

		[Test(IsAsync = true)]
		public void WhenAnyParamArrayWithResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();

			Task<Task<int>> task = Task.WhenAny(tcs1.Task, tcs2.Task, tcs3.Task);
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.IsTrue(t.Result == tcs2.Task, "Result should be correct");
				Assert.AreEqual(t.Result.Result, 3, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void WhenAnyEnumerableWithResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();

			Task<Task<int>> task = Task.WhenAny(MakeEnumerable(tcs1.Task, tcs2.Task, tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.IsTrue(t.Result == tcs2.Task, "Result should be correct");
				Assert.AreEqual(t.Result.Result, 3, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void WhenAnyParamArrayWithoutResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();

			Task<Task> task = Task.WhenAny((Task)tcs1.Task, (Task)tcs2.Task, (Task)tcs3.Task);
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.IsTrue(t.Result == tcs2.Task, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(t.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void WhenAnyEnumerableWithoutResultWorks() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();

			Task<Task> task = Task.WhenAny(MakeEnumerable((Task)tcs1.Task, (Task)tcs2.Task, (Task)tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.RanToCompletion, "Task2 should have run to completion");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.IsTrue(t.Result == tcs2.Task, "Result should be correct");
				Assert.AreEqual(t.Exception, null, "Exception for the aggregate task should be null");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Aggregate task should have run to completion");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetResult(3);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void WhenAnyFaultsIfTheFirstTaskFaulted() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();
			var ex = new Exception("Some text");

			Task<Task<int>> task = Task.WhenAny(MakeEnumerable(tcs1.Task, tcs2.Task, tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.Faulted, "Task2 should have faulted");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Exception.InnerExceptions.Length, 1, "There should be one inner exception");
				Assert.IsTrue(t.Exception.InnerExceptions[0] == ex, "Exception for the aggregate task should be correct");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Aggregate task should have faulted");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetException(ex);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void WhenAnyIsCancelledIfTheFirstTaskWasCancelled() {
			bool continuationRun = false;
			var tcs1 = new TaskCompletionSource<int>();
			var tcs2 = new TaskCompletionSource<int>();
			var tcs3 = new TaskCompletionSource<int>();

			Task<Task<int>> task = Task.WhenAny(MakeEnumerable(tcs1.Task, tcs2.Task, tcs3.Task));
			task.ContinueWith(t => {
				Assert.IsFalse(continuationRun, "Continuation should only be run once.");
				Assert.AreEqual(tcs2.Task.Status, TaskStatus.Canceled, "Task2 should be cancelled");

				Assert.IsTrue(task == t, "Callback parameter should be correct");

				Assert.AreEqual(t.Exception, null, "Aggregate task should not have exception");
				Assert.AreEqual(task.Status, TaskStatus.Canceled, "Aggregate task should be cancelled");

				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Running, "task should be running after creation.");

			tcs2.SetCanceled();

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should be run immediately");
				tcs1.SetResult(101);
				tcs3.SetResult(42);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "We should not time out");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ConstructorWithOnlyActionWorks() {
			bool taskRun = false, continuationRun = false;
			var task = new Task(() => {
				taskRun = true;
			});
			task.ContinueWith(t => {
				Assert.IsTrue(taskRun, "Task should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should have run to completion");
				Assert.IsTrue(task.Exception == null, "Exception should be null");
				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Created);

			Globals.SetTimeout(() => {
				Assert.IsFalse(taskRun, "Task should not be run before being started");
				task.Start();
				Assert.AreEqual(task.Status, TaskStatus.Running);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ConstructorWithActionAndStateWorks() {
			bool taskRun = false, continuationRun = false;
			var state = new object();
			var task = new Task(s => {
				Assert.IsTrue(state == s, "The state should be correct.");
				taskRun = true;
			}, state);
			task.ContinueWith(t => {
				Assert.IsTrue(taskRun, "Task should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should have run to completion");
				Assert.IsTrue(task.Exception == null, "Exception should be null");
				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Created);

			Globals.SetTimeout(() => {
				Assert.IsFalse(taskRun, "Task should not be run before being started");
				task.Start();
				Assert.AreEqual(task.Status, TaskStatus.Running);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ExceptionInManuallyCreatedTaskIsStoredOnTheTask() {
			bool taskRun = false, continuationRun = false;
			var ex = new Exception();
			var task = new Task(() => {
				taskRun = true;
				throw ex;
			});
			task.ContinueWith(t => {
				Assert.IsTrue(taskRun, "Task should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Task should be faulted");
				Assert.IsTrue((object)task.Exception is AggregateException, "Exception should be correct");
				Assert.AreEqual(task.Exception.InnerExceptions.Length, 1, "There should be one inner exception");
				Assert.IsTrue(task.Exception.InnerExceptions[0] == ex, "InnerException should be correct");
				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Created);

			Globals.SetTimeout(() => {
				Assert.IsFalse(taskRun, "Task should not be run before being started");
				task.Start();
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ConstructorWithOnlyFunctionWorks() {
			bool taskRun = false, continuationRun = false;
			var task = new Task<int>(() => {
				taskRun = true;
				return 42;
			});
			task.ContinueWith(t => {
				Assert.IsTrue(taskRun, "Task should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should have run to completion");
				Assert.AreEqual(task.Result, 42, "Result should be correct");
				Assert.IsTrue(task.Exception == null, "Exception should be null");
				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Created);

			Globals.SetTimeout(() => {
				Assert.IsFalse(taskRun, "Task should not be run before being started");
				task.Start();
				Assert.AreEqual(task.Status, TaskStatus.Running);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ConstructorWithFunctionAndStateWorks() {
			bool taskRun = false, continuationRun = false;
			var state = new object();
			var task = new Task<int>(s => {
				Assert.IsTrue(state == s, "The state should be correct.");
				taskRun = true;
				return 42;
			}, state);
			task.ContinueWith(t => {
				Assert.IsTrue(taskRun, "Task should be run before continuation");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should have run to completion");
				Assert.AreEqual(task.Result, 42, "Result should be correct");
				Assert.IsTrue(task.Exception == null, "Exception should be null");
				continuationRun = true;
			});

			Assert.AreEqual(task.Status, TaskStatus.Created);

			Globals.SetTimeout(() => {
				Assert.IsFalse(taskRun, "Task should not be run before being started");
				task.Start();
				Assert.AreEqual(task.Status, TaskStatus.Running);
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 200);
		}

		public void MethodWithDoneCallback(string s, int i, int timeout, Action done) {
			Assert.AreEqual(s, "expected string");
			Assert.AreEqual(i, 42);
			Globals.SetTimeout(done, timeout);
		}

		public void MethodWithDoneFunction(string s, int i, int timeout, Action<int> done, int j) {
			Assert.AreEqual(s, "expected string");
			Assert.AreEqual(i, 42);
			Globals.SetTimeout(() => done(i + j), timeout);
		}

		public void MethodWithDoneFunction2(string s, int i, int timeout, int j, Action<int> done) {
			Assert.AreEqual(s, "expected string");
			Assert.AreEqual(i, 42);
			Globals.SetTimeout(() => done(i + j), timeout);
		}

		public void FromDoneCallbackWithoutIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback(this, "methodWithDoneCallback", "expected string", 42, 200);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromDoneCallbackWithReturnValueWithoutIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback<int>(this, "methodWithDoneFunction2", "expected string", 42, 200, 15);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Result, 57);
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromDoneCallbackWithNonNegativeIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback(this, 3, "methodWithDoneCallback", "expected string", 42, 200);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromDoneCallbackWithNegativeIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback(this, -1, "methodWithDoneCallback", "expected string", 42, 200);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromDoneCallbackWithReturnValueWithNonNegativeIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback<int>(this, 3, "methodWithDoneFunction", "expected string", 42, 200, 15);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Result, 57);
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromDoneCallbackWithReturnValueWithNegativeIndexingWorks() {
			bool continuationRun = false;
			var task = Task.FromDoneCallback<int>(this, -2, "methodWithDoneFunction", "expected string", 42, 200, 17);
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Result, 59);
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[PreserveName]
		private void NodeAsyncHelper(int timeout, object[] callbackArgs, Delegate action) {
			Globals.SetTimeout(() => ((Function)action).Apply(this, callbackArgs), timeout);
		}

		[Test(IsAsync = true)]
		public void FromNodeWithoutResultWorks() {
			bool continuationRun = false;
			var task = Task.FromNode(this, "nodeAsyncHelper", 200, new object[] { null });
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromNodeWithoutResultFactoryWorks() {
			bool continuationRun = false;
			var task = Task.FromNode<int>(this, "nodeAsyncHelper", 200, new object[] { null, 42 });
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				Assert.AreEqual(task.Result, 42);
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromNodeWithResultFactoryWorks() {
			bool continuationRun = false;
			var task = Task.FromNode(this, (int i, string s) => new { i, s }, "nodeAsyncHelper", 200, new object[] { null, 42, "Test 123" });
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed");
				Assert.AreEqual(task.Result.i, 42);
				Assert.AreEqual(task.Result.s, "Test 123");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}

		[Test(IsAsync = true)]
		public void FromNodeWorksWhenTheCallbackIsInvokedWithAnError() {
			bool continuationRun = false;
			var err = new Error();
			var task = Task.FromNode(this, "nodeAsyncHelper", 200, new object[] { err });
			Assert.AreEqual(task.Status, TaskStatus.Running);
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "Callback parameter should be correct");
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Task should be faulted");
				Assert.IsTrue(task.Exception is AggregateException, "The exception should be an AggregateException");
				Assert.IsTrue(task.Exception.InnerExceptions[0] is JsErrorException, "The inner exception should be a JsErrorException");
				Assert.AreEqual(((JsErrorException)task.Exception.InnerExceptions[0]).Error, err, "The error in the JsErrorException should be correct");
				continuationRun = true;
			});

			Globals.SetTimeout(() => {
				Assert.IsFalse(task.IsCompleted, "Task should not be completed too early");
			}, 100);

			Globals.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "The continuation should be run");
				Engine.Start();
			}, 300);
		}
	}
}
