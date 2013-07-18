using System;
using System.Diagnostics;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class StopwatchTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Stopwatch).FullName, "ss.Stopwatch", "Class name");
			Assert.IsTrue(typeof(Stopwatch).IsClass, "IsClass");
			var watch = new Stopwatch();
			Assert.IsTrue((object)watch is Stopwatch, "is StopWatch");

			Assert.AreEqual(0, typeof(Stopwatch).GetInterfaces().Length, "Interfaces should be empty");
		}

		[Test]
		public void DefaultConstructorWorks() {
			var watch = new Stopwatch();
			Assert.IsTrue((object)watch is Stopwatch, "is Stopwatch");
			Assert.IsFalse(watch.IsRunning, "IsRunning");
		}

		[Test]
		public void ConstantsWorks() {
			Assert.IsTrue(Stopwatch.Frequency > 1000, "Frequency");
			Assert.AreEqual(Type.GetScriptType(Stopwatch.IsHighResolution), "boolean", "IsHighResolution");
		}

		[Test]
		public void StartNewWorks() {
			var watch = Stopwatch.StartNew();
			Assert.IsTrue((object)watch is Stopwatch, "is Stopwatch");
			Assert.IsTrue(watch.IsRunning, "IsRunning");
		}

		[Test]
		public void StartAndStopWork() {
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
			bool hasIncreased = false;
			while ((DateTime.Now - before) < 200) {
				if (watch.ElapsedTicks > 0) {
					hasIncreased = true;
				}
			}
			watch.Stop();
			Assert.IsTrue(hasIncreased, "Times should increase inside the loop");
			Assert.IsTrue(watch.ElapsedMilliseconds > 150 && watch.ElapsedMilliseconds < 250, "ElapsedMilliseconds");
			Assert.IsTrue(watch.Elapsed == new TimeSpan(0, 0, 0, 0, (int)watch.ElapsedMilliseconds), "Elapsed");
			var value = (double)watch.ElapsedTicks / Stopwatch.Frequency;
			Assert.IsTrue(value > 0.15 && value < 0.25, "Ticks");
		}

		[Test]
		public void GetTimestampWorks() {
			long t1 = Stopwatch.GetTimestamp();
			Assert.IsTrue((object)t1 is long, "is long");

			DateTime before = DateTime.Now;
			while ((DateTime.Now - before) < 50) {
			}
			long t2 = Stopwatch.GetTimestamp();
			Assert.IsTrue(t2 > t1, "Should increase");
		}
	}
}
