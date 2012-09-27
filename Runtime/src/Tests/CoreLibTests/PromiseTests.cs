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

		[Test]
		public void TaskFromPromiseWithoutResultWorksWhenPromiseCompletes() {
			Assert.IsTrue(false, "TODO");
		}

		[Test]
		public void TaskFromPromiseWithResultIndexWorksWhenPromiseCompletes() {
			Assert.IsTrue(false, "TODO");
		}

		[Test]
		public void TaskFromPromiseWithResultFactoryWorksWhenPromiseCompletes() {
			Assert.IsTrue(false, "TODO");
		}

		[Test]
		public void TaskFromPromiseWorksWhenPromiseFails() {
			Assert.IsTrue(false, "TODO");
		}

		[Test]
		public void CompletingPromiseCanBeAwaited() {
			Assert.IsTrue(false, "TODO");
		}

		[Test]
		public void FailingPromiseCanBeAwaited() {
			Assert.IsTrue(false, "TODO");
		}
	}
}
