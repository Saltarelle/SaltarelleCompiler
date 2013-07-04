using System;
using System.Diagnostics;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class StopwatchTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var watch = new Stopwatch();
			Assert.AreEqual(typeof(Stopwatch).FullName, "ss.Stopwatch");
			Assert.IsTrue(typeof(Stopwatch).IsClass);
			Assert.IsTrue(watch is Stopwatch);
		}

		[Test]
		public void DefaultConstructorWorks() {
			try {
				var watch = new Stopwatch();
			}
			catch (Exception) {
				Assert.Fail("Failed to create Stopwatch instance!");
			}

			Assert.ExpectAsserts(0);
		}

		[Test]
		public void ConstantsWorks() {
			Assert.AreEqual(Stopwatch.Frequency, 10000000);
			Assert.IsFalse(Stopwatch.IsHighResolution);
		}

		[Test]
		public void StartNewWorks() {
			var watch = Stopwatch.StartNew();
			Assert.IsTrue(watch.IsRunning);
		}

		[Test]
		public void StartAndStopWorks() {
			var watch = new Stopwatch();
			Assert.IsFalse(watch.IsRunning);
			watch.Start();
			Assert.IsTrue(watch.IsRunning);
			watch.Stop();
			Assert.IsFalse(watch.IsRunning);
		}

		[Test]
		public void ElapsedWorks() {
			var watch = new Stopwatch();
			Assert.AreEqual(watch.ElapsedTicks, 0);
			Assert.AreEqual(watch.ElapsedMilliseconds, 0);
			Assert.AreEqual(watch.Elapsed, new TimeSpan());
			watch.Start();
			DateTime before = DateTime.Now;
			while ((DateTime.Now - before) < 1000) {
			}
			watch.Stop();
			Assert.AreEqual(watch.ElapsedMilliseconds, 1000);
			Assert.AreEqual(watch.Elapsed, new TimeSpan(0, 0, 1));
			Assert.AreEqual(watch.ElapsedTicks, Stopwatch.Frequency);
		}
	}
}
