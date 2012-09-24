using System;
using System.Collections.Generic;
using System.Html;
using System.Testing;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibTests {
	[TestFixture]
	public class TaskTests {
		[AsyncTest]
		public void DelayAndContinueWithWorkWithNoResultAndNoException() {
			bool done = false;
			var delay = Task.Delay(100);
			Assert.AreEqual(delay.Status, TaskStatus.Running, "delay should be running at point 1");
			Task continuedTask = null;
			continuedTask = delay.ContinueWith(t => {
				Assert.IsTrue(t == delay, "argument to delay.ContinueWith callback should be correct");
				Assert.AreEqual(delay.Status, TaskStatus.RanToCompletion, "delay should have run to completion at point 2");
				Assert.AreEqual(delay.Exception, null, "delay should not have an exception");

				Assert.AreEqual(continuedTask.Status, TaskStatus.Running, "continuedTask should be running at point 2");
			});
			Assert.IsFalse(delay == continuedTask, "delay and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.RanToCompletion, "continuedTask should have run to completion at point 3");
				Assert.AreEqual(continuedTask.Exception, null, "continuedTask should not have an exception");

				done = true;
			});

			Window.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				QUnit.Start();
			}, 200);
		}

		[AsyncTest]
		public void TaskThatThrowsAnException() {
			bool done = false;
			var delay = Task.Delay(100);
			Assert.AreEqual(delay.Status, TaskStatus.Running, "delay should be running at point 1");
			Task continuedTask = null;
			continuedTask = delay.ContinueWith(t => {
				Script.Eval("throw 'This is a test message'");
			});
			Assert.IsFalse(delay == continuedTask, "delay and continuedTask should not be the same");
			continuedTask.ContinueWith(t => {
				Assert.IsTrue(t == continuedTask, "argument to continuedTask.ContinueWith callback should be correct");
				Assert.AreEqual(continuedTask.Status, TaskStatus.Faulted, "continuedTask should have run to completion at point 3");
				Assert.AreNotEqual(continuedTask.Exception, null, "continuedTask should have an exception");
				Assert.IsTrue(continuedTask.Exception is Exception);
				Assert.AreEqual(continuedTask.Exception.Message, "This is a test message");

				done = true;
			});

			Window.SetTimeout(() => {
				Assert.IsTrue(done, "We should not time out");
				QUnit.Start();
			}, 200);
		}
	}
}
