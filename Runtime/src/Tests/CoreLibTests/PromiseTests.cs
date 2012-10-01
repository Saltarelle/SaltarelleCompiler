using System;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;
using System.Testing;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibTests {
	[TestFixture]
	public class PromiseTests {
		[Imported]
		private class SimplePromise : IPromise {
			public void Then(Delegate doneHandler) {}
			public void Then(Delegate doneHandler, Delegate failHandler) {}
			public void Then(Delegate doneHandler, Delegate failHandler, Delegate progressHandler) {}

			[ExpandParams]
			public void Resolve(params object[] args) {}

			[ExpandParams]
			public void Reject(params object[] args) {}
		}

		[InlineCode("new Promise()")]
		private SimplePromise CreatePromise() {
			return null;
		}

		[AsyncTest]
		public void TaskFromPromiseWithoutResultFactoryWorksWhenPromiseCompletes() {
			var promise = CreatePromise();
			var task = Task.FromPromise(promise);
			Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running after being created");
			bool continuationRun = false;
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "ContinueWith parameter should be correct");
				continuationRun = true;
			});

			Window.SetTimeout(() => {
				Assert.IsFalse(continuationRun, "Continuation should not be run too early.");
				Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running before promise is completed.");
				promise.Resolve(42, "result 123", 101);
			}, 100);

			Window.SetTimeout(() => {
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed after promise");
				Assert.IsTrue(continuationRun, "Continuation should have been run after promise was completed.");
				Assert.AreEqual(task.Result, new object[] { 42, "result 123", 101 }, "The result should be correct");

				QUnit.Start();
			}, 200);
		}

		[AsyncTest]
		public void TaskFromPromiseWithResultIndexWorksWhenPromiseCompletes() {
			var promise = CreatePromise();
			var tasks = new Task<int>[] {
				Task.FromPromise<int>(promise, 0),
				Task.FromPromise<int>(promise, 1),
				Task.FromPromise<int>(promise, 2),
				Task.FromPromise<int>(promise, -3),
				Task.FromPromise<int>(promise, -2),
				Task.FromPromise<int>(promise, -1),
			};
			var continuationsRun = new bool[6];
			Assert.IsTrue(tasks.Every(t => t.Status == TaskStatus.Running), "Tasks should be running after being created");
			tasks.ForEach((x, i, _) => x.ContinueWith(t => {
				Assert.IsTrue(t == x, "ContinueWith parameter should be correct");
				continuationsRun[i] = true;
			}));

			Window.SetTimeout(() => {
				Assert.IsFalse(continuationsRun.Some(x => x), "Continuations should not be run too early.");
				Assert.IsTrue(tasks.Every(t => t.Status == TaskStatus.Running), "Tasks should be running before promise is completed.");
				promise.Resolve(10, 42, 38);
			}, 100);

			Window.SetTimeout(() => {
				Assert.IsTrue(tasks.Every(t => t.Status == TaskStatus.RanToCompletion), "Task should be completed after promise");
				Assert.IsTrue(continuationsRun.Every(x => x), "Continuations should have been run after promise was completed.");
				Assert.AreEqual(tasks[0].Result, 10, "Task 0 result should be correct");
				Assert.AreEqual(tasks[1].Result, 42, "Task 1 result should be correct");
				Assert.AreEqual(tasks[2].Result, 38, "Task 2 result should be correct");
				Assert.AreEqual(tasks[3].Result, 10, "Task 3 result should be correct");
				Assert.AreEqual(tasks[4].Result, 42, "Task 4 result should be correct");
				Assert.AreEqual(tasks[5].Result, 38, "Task 5 result should be correct");
				QUnit.Start();
			}, 200);
		}

		[AsyncTest]
		public void TaskFromPromiseWithResultFactoryWorksWhenPromiseCompletes() {
			var promise = CreatePromise();
			var task = Task.FromPromise(promise, (int i, string s, int j) => new { i, s, j });
			Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running after being created");
			bool continuationRun = false;
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "ContinueWith parameter should be correct");
				continuationRun = true;
			});

			Window.SetTimeout(() => {
				Assert.IsFalse(continuationRun, "Continuation should not be run too early.");
				Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running before promise is completed.");
				promise.Resolve(42, "result 123", 101);
			}, 100);

			Window.SetTimeout(() => {
				Assert.AreEqual(task.Status, TaskStatus.RanToCompletion, "Task should be completed after promise");
				Assert.IsTrue(continuationRun, "Continuation should have been run after promise was completed.");
				Assert.AreEqual(task.Result, new { i = 42, s = "result 123", j = 101 });
				QUnit.Start();
			}, 200);
		}

		[AsyncTest]
		public void TaskFromPromiseWorksWhenPromiseFails() {
			var promise = CreatePromise();
			var task = Task.FromPromise(promise);
			Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running after being created");
			bool continuationRun = false;
			task.ContinueWith(t => {
				Assert.IsTrue(t == task, "ContinueWith parameter should be correct");
				continuationRun = true;
			});

			Window.SetTimeout(() => {
				Assert.IsFalse(continuationRun, "Continuation should not be run too early.");
				Assert.AreEqual(task.Status, TaskStatus.Running, "Task should be running before promise is completed.");
				promise.Reject(42, "result 123", 101);
			}, 100);

			Window.SetTimeout(() => {
				Assert.AreEqual(task.Status, TaskStatus.Faulted, "Task should have faulted after the promise was rejected.");
				Assert.IsTrue(continuationRun, "Continuation should have been run after promise was rejected.");
				Assert.IsTrue((object)task.Exception is AggregateException, "Exception should be an AggregateException");
				Assert.AreEqual(task.Exception.InnerExceptions.Length, 1, "Exception should have one inner exception");
				Assert.IsTrue(task.Exception.InnerExceptions[0] is PromiseException, "Inner exception should be a PromiseException");
				Assert.AreEqual(((PromiseException)task.Exception.InnerExceptions[0]).Arguments, new object[] { 42, "result 123", 101 }, "The PromiseException arguments should be correct");

				QUnit.Start();
			}, 200);
		}

#if !NO_ASYNC
		[AsyncTest]
		public async void CompletingPromiseCanBeAwaited() {
			var promise = CreatePromise();
			object[] result = null;

			Window.SetTimeout(() => {
				Assert.IsTrue(result == null, "Await should not finish too early.");
				promise.Resolve(42, "result 123", 101);
			}, 100);

			Window.SetTimeout(() => {
				Assert.AreEqual(result, new object[] { 42, "result 123", 101 }, "The result should be correct");
				QUnit.Start();
			}, 200);

			result = await promise;
		}

		[AsyncTest]
		public async void FailingPromiseCanBeAwaited() {
			var promise = CreatePromise();
			bool continuationRun = false;

			Window.SetTimeout(() => {
				Assert.IsFalse(continuationRun, "Continuation should not be run too early.");
				promise.Reject(42, "result 123", 101);
			}, 100);

			Window.SetTimeout(() => {
				Assert.IsTrue(continuationRun, "Continuation should have been run after promise was rejected.");
				QUnit.Start();
			}, 200);

			try {
				await promise;
				Assert.IsTrue(false, "Await should throw");
			}
			catch (AggregateException ex) {
				Assert.AreEqual(ex.InnerExceptions.Length, 1, "Exception should have one inner exception");
				Assert.IsTrue(ex.InnerExceptions[0] is PromiseException, "Inner exception should be a PromiseException");
				Assert.AreEqual(((PromiseException)ex.InnerExceptions[0]).Arguments, new object[] { 42, "result 123", 101 }, "The PromiseException arguments should be correct");
			}
			catch (Exception ex) {
				Assert.IsTrue(false, "Thrown exception should have been an AggregateException, was " + ex.GetType().FullName);
			}
			continuationRun = true;
		}
#endif
	}
}
