using System;
using System.Collections.Generic;
using System.Threading;
using QUnit;

#pragma warning disable 183, 1718

namespace CoreLib.TestScript.Threading {
	[TestFixture]
	public class CancellationTokenTests {
		[Test]
		public void TypePropertiesForCancellationTokenSourceAreCorrect() {
			Assert.AreEqual(typeof(CancellationTokenSource).FullName, "ss.CancellationTokenSource", "FullName");
			Assert.IsTrue(typeof(CancellationTokenSource).IsClass, "IsClass should be true");
			object cts = new CancellationTokenSource();
			Assert.IsTrue(cts is CancellationTokenSource);
			Assert.IsTrue(cts is IDisposable);

			var interfaces = typeof(CancellationTokenSource).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 1, "Interface count should be 1");
			Assert.IsTrue(interfaces.Contains(typeof(IDisposable)), "Interfaces should contain IDisposable");
		}

		[Test]
		public void TypePropertiesForCancellationTokenAreCorrect() {
			Assert.AreEqual(typeof(CancellationToken).FullName, "ss.CancellationToken", "FullName");
			Assert.IsFalse(typeof(CancellationToken).IsClass, "IsClass should be false");

			Assert.IsTrue(new CancellationToken() is CancellationToken);
			Assert.IsTrue(CancellationToken.None is CancellationToken);
			Assert.IsTrue(new CancellationTokenSource().Token is CancellationToken);
		}

		[Test]
		public void TypePropertiesForCancellationTokenRegistrationAreCorrect() {
			Assert.AreEqual(typeof(CancellationTokenRegistration).FullName, "ss.CancellationTokenRegistration", "FullName");
			Assert.IsFalse(typeof(CancellationTokenRegistration).IsClass, "IsClass should be false");

			object ctr = new CancellationTokenRegistration();
			Assert.IsTrue(ctr is CancellationTokenRegistration);
			Assert.IsTrue(ctr is IDisposable);

			var interfaces = typeof(CancellationTokenRegistration).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2, "Interface count should be 2");
			Assert.IsTrue(interfaces.Contains(typeof(IDisposable)), "Interfaces should contain IDisposable");
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<CancellationTokenRegistration>)), "Interfaces should contain IEquatable");
		}

		[Test]
		public void CancellationTokenCreatedWithDefaultConstructorIsNotCanceledAndCannotBe() {
			var ct = new CancellationToken();
			Assert.IsFalse(ct.CanBeCanceled, "CanBeCanceled");
			Assert.IsFalse(ct.IsCancellationRequested, "IsCancellationRequested");
			ct.ThrowIfCancellationRequested();
		}

		[Test]
		public void CancellationTokenCreatedWithFalseArgumentToConstructorIsNotCanceledAndCannotBe() {
			var ct = new CancellationToken(false);
			Assert.IsFalse(ct.CanBeCanceled, "CanBeCanceled");
			Assert.IsFalse(ct.IsCancellationRequested, "IsCancellationRequested");
			ct.ThrowIfCancellationRequested();
		}

		[Test]
		public void CancellationTokenCreatedWithTrueArgumentToConstructorIsCanceled() {
			var ct = new CancellationToken(true);
			Assert.IsTrue(ct.CanBeCanceled, "CanBeCanceled");
			Assert.IsTrue(ct.IsCancellationRequested, "IsCancellationRequested");
			Assert.Throws<OperationCanceledException>(() => ct.ThrowIfCancellationRequested());
		}

		[Test]
		public void CancellationTokenNoneIsNotCancelledAndCannotBe() {
			Assert.IsFalse(CancellationToken.None.CanBeCanceled, "CanBeCanceled");
			Assert.IsFalse(CancellationToken.None.IsCancellationRequested, "IsCancellationRequested");
			CancellationToken.None.ThrowIfCancellationRequested();
		}

		[Test]
		public void CreatingADefaultCancellationTokenReturnsACancellationTokenThatIsNotCancelled() {
			var ct = default(CancellationToken);
			Assert.IsFalse(ct.CanBeCanceled, "CanBeCanceled");
			Assert.IsFalse(ct.IsCancellationRequested, "IsCancellationRequested");
			ct.ThrowIfCancellationRequested();
		}

		[Test]
		public void ActivatorCreateForCancellationTokenReturnsACancellationTokenThatIsNotCancelled() {
			var ct = Activator.CreateInstance<CancellationToken>();
			Assert.IsFalse(ct.CanBeCanceled, "CanBeCanceled");
			Assert.IsFalse(ct.IsCancellationRequested, "IsCancellationRequested");
			ct.ThrowIfCancellationRequested();
		}

		[Test]
		public void CanBeCanceledIsTrueForTokenCreatedForCancellationTokenSource() {
			var cts = new CancellationTokenSource();
			Assert.IsTrue(cts.Token.CanBeCanceled, "cts.Token");
		}

		[Test]
		public void IsCancellationRequestedForTokenCreatedForCancellationTokenSourceIsSetByTheCancelMethod() {
			var cts = new CancellationTokenSource();
			Assert.IsFalse(cts.IsCancellationRequested, "cts.IsCancellationRequested false");
			Assert.IsFalse(cts.Token.IsCancellationRequested, "cts.Token.IsCancellationRequested false");
			cts.Cancel();
			Assert.IsTrue(cts.IsCancellationRequested, "cts.IsCancellationRequested true");
			Assert.IsTrue(cts.Token.IsCancellationRequested, "cts.Token.IsCancellationRequested true");
		}

		[Test]
		public void ThrowIfCancellationRequestedForTokenCreatedForCancellationTokenSourceThrowsAfterTheCancelMethodIsCalled() {
			var cts = new CancellationTokenSource();
			cts.Token.ThrowIfCancellationRequested();
			cts.Cancel();
			Assert.Throws<OperationCanceledException>(() => cts.Token.ThrowIfCancellationRequested(), "cts.Token.ThrowIfCancellationRequested");
		}

		[Test]
		public void CancelWithoutArgumentsWorks() {
			var ex1 = new Exception();
			var ex2 = new Exception();
			var cts = new CancellationTokenSource();
			var calledHandlers = new List<int>();
			cts.Token.Register(() => { calledHandlers.Add(0); });
			cts.Token.Register(() => { calledHandlers.Add(1); throw ex1; });
			cts.Token.Register(() => { calledHandlers.Add(2); });
			cts.Token.Register(() => { calledHandlers.Add(3); throw ex2; });
			cts.Token.Register(() => { calledHandlers.Add(4); });

			try {
				cts.Cancel();
				Assert.Fail("Should have thrown");
			}
			catch (AggregateException ex) {
				Assert.AreEqual(ex.InnerExceptions.Count, 2, "count");
				Assert.IsTrue(ex.InnerExceptions.Contains(ex1), "ex1");
				Assert.IsTrue(ex.InnerExceptions.Contains(ex2), "ex2");
			}

			Assert.IsTrue(calledHandlers.Contains(0) && calledHandlers.Contains(1) && calledHandlers.Contains(2) && calledHandlers.Contains(3) && calledHandlers.Contains(4));
		}

		[Test]
		public void CancelWorksWhenThrowOnFirstExceptionIsFalse() {
			var ex1 = new Exception();
			var ex2 = new Exception();
			var cts = new CancellationTokenSource();
			var calledHandlers = new List<int>();
			cts.Token.Register(() => { calledHandlers.Add(0); });
			cts.Token.Register(() => { calledHandlers.Add(1); throw ex1; });
			cts.Token.Register(() => { calledHandlers.Add(2); });
			cts.Token.Register(() => { calledHandlers.Add(3); throw ex2; });
			cts.Token.Register(() => { calledHandlers.Add(4); });

			try {
				cts.Cancel(false);
				Assert.Fail("Should have thrown");
			}
			catch (AggregateException ex) {
				Assert.AreEqual(ex.InnerExceptions.Count, 2, "ex count");
				Assert.IsTrue(ex.InnerExceptions.Contains(ex1), "ex1");
				Assert.IsTrue(ex.InnerExceptions.Contains(ex2), "ex2");
			}

			Assert.AreEqual(calledHandlers.Count, 5, "called handler count");
			Assert.IsTrue(calledHandlers.Contains(0) && calledHandlers.Contains(1) && calledHandlers.Contains(2) && calledHandlers.Contains(3) && calledHandlers.Contains(4));
		}

		[Test]
		public void CancelWorksWhenThrowOnFirstExceptionIsTrue() {
			var ex1 = new Exception();
			var ex2 = new Exception();
			var cts = new CancellationTokenSource();
			var calledHandlers = new List<int>();
			cts.Token.Register(() => { calledHandlers.Add(0); });
			cts.Token.Register(() => { calledHandlers.Add(1); throw ex1; });
			cts.Token.Register(() => { calledHandlers.Add(2); });
			cts.Token.Register(() => { calledHandlers.Add(3); throw ex2; });
			cts.Token.Register(() => { calledHandlers.Add(4); });

			try {
				cts.Cancel(true);
				Assert.Fail("Should have thrown");
			}
			catch (Exception ex) {
				Assert.IsTrue(object.ReferenceEquals(ex, ex1), "ex");
			}

			Assert.AreEqual(calledHandlers.Count, 2, "called handler count");
			Assert.IsTrue(calledHandlers.Contains(0) && calledHandlers.Contains(1));
		}

		[Test]
		public void RegisterOnACancelledSourceWithoutContextInvokesTheCallback() {
			var cts = new CancellationTokenSource();
			cts.Cancel();
			int state = 0;
			cts.Token.Register(() => state = 1);
			Assert.AreEqual(state, 1);
		}

		[Test(IsAsync = true)]
		public void RegisterOnNonACancelledSourceWithoutContextRegistersTheCallbackForExecution() {
			var cts = new CancellationTokenSource();
			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => cts.Cancel(), 100);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test]
		public void RegisterWithArgumentOnACancelledSourceInvokesTheCallback() {
			var cts = new CancellationTokenSource();
			var context = new object();
			cts.Cancel();
			int state = 0;
			cts.Token.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); state = 1; }, context);
			Assert.AreEqual(state, 1);
		}

		[Test(IsAsync = true)]
		public void RegisterWithArgumentOnANonCancelledSourcetRegistersTheCallbackForExecution() {
			var cts = new CancellationTokenSource();
			var context = new object();
			int state = 0;
			cts.Token.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); state = 1; }, context);

			Globals.SetTimeout(() => cts.Cancel(), 100);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test]
		public void RegisterOnACancelledSourceWithoutContextRethrowsAThrownException() {
			var ex1 = new Exception();
			var cts = new CancellationTokenSource();
			cts.Cancel();
			try {
				cts.Token.Register(() => { throw ex1; });
				Assert.Fail("Should have thrown");
			}
			catch (Exception ex) {
				Assert.IsTrue(ReferenceEquals(ex, ex1), "Exception");
			}
		}

		[Test]
		public void RegisterOnACancelledSourceWithContextRethrowsAThrownException() {
			var ex1 = new Exception();
			var context = new object();
			var cts = new CancellationTokenSource();
			cts.Cancel();
			try {
				cts.Token.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); throw ex1; }, context);
				Assert.Fail("Should have thrown");
			}
			catch (Exception ex) {
				Assert.IsTrue(ReferenceEquals(ex, ex1), "Exception");
			}
		}

		[Test]
		public void RegisterOverloadsWithUseSynchronizationContextWork() {
			var cts = new CancellationTokenSource();
			var context = new object();
			cts.Cancel();
			int numCalled = 0;
			cts.Token.Register(c => numCalled++, true);
			cts.Token.Register(c => numCalled++, false);
			cts.Token.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); numCalled++; }, context, true);
			cts.Token.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); numCalled++; }, context, false);
			Assert.AreEqual(numCalled, 4);
		}

		[Test(ExpectedAssertionCount = 0)]
		public void CancellationTokenSourceCanBeDisposed() {
			var cts = new CancellationTokenSource();
			cts.Dispose();
		}

		[Test(IsAsync = true)]
		public void ConstructorWithMillisecondsArgumentWorks() {
			var cts = new CancellationTokenSource(100);
			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void CancelAfterWithMillisecondsArgumentWorks() {
			var cts = new CancellationTokenSource();
			cts.CancelAfter(100);
			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void ConstructorWithTimeSpanArgumentWorks() {
			var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void CancelAfterWithTimeSpanArgumentWorks() {
			var cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromMilliseconds(100));

			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1);
				Engine.Start();
			}, 200);
		}

		[Test(IsAsync = true)]
		public void CancelAfterRunsAllCallbacksEvenIfSomeThrowExceptions() {
			var ex1 = new Exception();
			var ex2 = new Exception();
			var cts = new CancellationTokenSource();
			cts.CancelAfter(100);
			var calledHandlers = new List<int>();
			cts.Token.Register(() => { calledHandlers.Add(0); });
			cts.Token.Register(() => { calledHandlers.Add(1); throw ex1; });
			cts.Token.Register(() => { calledHandlers.Add(2); });
			cts.Token.Register(() => { calledHandlers.Add(3); throw ex2; });
			cts.Token.Register(() => { calledHandlers.Add(4); });

			Globals.SetTimeout(() => {
				Assert.IsTrue(calledHandlers.Contains(0) && calledHandlers.Contains(1) && calledHandlers.Contains(2) && calledHandlers.Contains(3) && calledHandlers.Contains(4));
				Engine.Start();
			}, 200);
		}

		[Test]
		public void RegisterOnCancellationTokenCreatedNonCancelledDoesNothing() {
			var ct = new CancellationToken(false);

			int state = 0;
			ct.Register(() => state = 1);

			Assert.AreEqual(state, 0);
		}

		[Test]
		public void RegisterOnCancellationTokenCreatedCancelledInvokesTheActionImmediately() {
			var ct = new CancellationToken(true);

			int state = 0;
			var context = new object();
			ct.Register(() => state = 1);
			Assert.AreEqual(state, 1, "state 1");
			ct.Register(c => { Assert.IsTrue(ReferenceEquals(context, c), "context"); state = 2; }, context);
			Assert.AreEqual(state, 2, "state 2");
		}

		[Test(IsAsync = true)]
		public void CancelAfterResetsTheCancelTimer() {
			var cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromMilliseconds(100));
			cts.CancelAfter(TimeSpan.FromMilliseconds(300));

			int state = 0;
			cts.Token.Register(() => state = 1);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 0, "#1");
			}, 200);

			Globals.SetTimeout(() => {
				Assert.AreEqual(state, 1, "#2");
				Engine.Start();
			}, 400);
		}

		[Test]
		public void DuplicateCancelDoesNotCauseCallbacksToBeCalledTwice() {
			var cts = new CancellationTokenSource();
			int calls = 0;
			cts.Token.Register(() => calls = 1);
			cts.Cancel();
			cts.Cancel();

			Assert.AreEqual(calls, 1);
		}

		[Test]
		public void RegistrationsCanBeCompared() {
			var cts = new CancellationTokenSource();
			var ctr1 = cts.Token.Register(() => {});
			var ctr2 = cts.Token.Register(() => {});

			Assert.IsTrue (ctr1.Equals(ctr1), "#1");
			Assert.IsFalse(ctr1.Equals(ctr2), "#2");
			Assert.IsTrue (ctr1.Equals((object)ctr1), "#3");
			Assert.IsFalse(ctr1.Equals((object)ctr2), "#4");

			Assert.IsTrue (ctr1 == ctr1, "#5");
			Assert.IsFalse(ctr1 == ctr2, "#6");
			Assert.IsFalse(ctr1 != ctr1, "#7");
			Assert.IsTrue (ctr1 != ctr2, "#8");
		}

		[Test]
		public void RegistrationsCanBeUnregistered() {
			var cts = new CancellationTokenSource();
			var calledHandlers = new List<int>();
			cts.Token.Register(() => { calledHandlers.Add(0); });
			var reg = cts.Token.Register(() => { calledHandlers.Add(1); });
			Assert.IsTrue(reg is CancellationTokenRegistration);

			cts.Token.Register(() => { calledHandlers.Add(2); });

			reg.Dispose();

			cts.Cancel();

			Assert.AreEqual(calledHandlers.Count, 2);
			Assert.IsTrue(calledHandlers.Contains(0) && calledHandlers.Contains(2));
		}

		[Test]
		public void CreatingADefaultCancellationTokenRegistrationReturnsARegistrationThatCanBeDisposedWithoutHarm() {
			var ct = default(CancellationTokenRegistration);
			Assert.IsNotNull(ct, "not null");
			Assert.IsTrue(ct is CancellationTokenRegistration, "is CancellationTokenRegistration");
			ct.Dispose();
		}

		[Test]
		public void LinkedSourceWithTwoTokensWorks() {
			{
				var cts1 = new CancellationTokenSource();
				var cts2 = new CancellationTokenSource();
				var linked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);

				Assert.IsFalse(linked.IsCancellationRequested, "#1");
				cts1.Cancel();
				Assert.IsTrue(linked.IsCancellationRequested, "#2");
			}

			{
				var cts1 = new CancellationTokenSource();
				var cts2 = new CancellationTokenSource();
				var linked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);

				Assert.IsFalse(linked.IsCancellationRequested, "#1");
				cts2.Cancel();
				Assert.IsTrue(linked.IsCancellationRequested, "#2");
			}
		}

		[Test]
		public void LinkedSourceWithThreeTokensWorks() {
			{
				var cts1 = new CancellationTokenSource();
				var cts2 = new CancellationTokenSource();
				var cts3 = new CancellationTokenSource();
				var linked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token, cts3.Token);

				Assert.IsFalse(linked.IsCancellationRequested, "#1 1");
				cts1.Cancel();
				Assert.IsTrue(linked.IsCancellationRequested, "#1 2");
			}

			{
				var cts1 = new CancellationTokenSource();
				var cts2 = new CancellationTokenSource();
				var cts3 = new CancellationTokenSource();
				var linked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token, cts3.Token);

				Assert.IsFalse(linked.IsCancellationRequested, "#2 1");
				cts2.Cancel();
				Assert.IsTrue(linked.IsCancellationRequested, "#2 2");
			}

			{
				var cts1 = new CancellationTokenSource();
				var cts2 = new CancellationTokenSource();
				var cts3 = new CancellationTokenSource();
				var linked = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token, cts3.Token);

				Assert.IsFalse(linked.IsCancellationRequested, "#3 1");
				cts3.Cancel();
				Assert.IsTrue(linked.IsCancellationRequested, "#3 2");
			}
		}
	}
}
