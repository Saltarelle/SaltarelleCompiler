using System;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class TimeSpanTests
	{
		[Test]
		public void TypePropertiesAreCorrect()
		{
			var time = new TimeSpan();
			Assert.AreEqual(typeof(TimeSpan).FullName, "ss.TimeSpan");
			Assert.IsTrue(typeof(TimeSpan).IsClass);
			Assert.IsTrue(time is TimeSpan);
		}

		[Test]
		public void DefaultConstructorWorks()
		{
			try
			{
				var time = new TimeSpan();
			}
			catch (Exception)
			{
				Assert.Fail("Failed to create Random instance!");
			}

			Assert.ExpectAsserts(0);
		}

		[Test]
		public void ParameterConstructorsWorks()
		{
			try
			{
				var time = new TimeSpan(34567L);
				time = new TimeSpan(10, 20, 5);
				time = new TimeSpan(15, 10, 20, 5);
				time = new TimeSpan(15, 10, 20, 5, 14);
			}
			catch (Exception)
			{
				Assert.Fail("Failed to create TimeSpan constructors with parameters!");
			}

			Assert.ExpectAsserts(0);
		}
		[Test]
		public void PropertiesWorks()
		{
			var time = new TimeSpan(15, 10, 20, 5, 14);
			Assert.AreEqual(time.Days, 15);
			Assert.AreEqual(time.Hours, 10);
			Assert.AreEqual(time.Minutes, 20);
			Assert.AreEqual(time.Seconds, 5);
			Assert.AreEqual(time.Milliseconds, 14);
			AssertAlmostEqual(time.TotalDays, 15.430613587962963d);
			AssertAlmostEqual(time.TotalHours, 370.33472611111108d);
			AssertAlmostEqual(time.TotalMinutes, 22220.083566666668d);
			AssertAlmostEqual(time.TotalSeconds, 1333205.014d);
			AssertAlmostEqual(time.TotalMilliseconds, 1333205014.0d);
			Assert.AreEqual(time.Ticks, 15 * TimeSpan.TicksPerDay +
				10 * TimeSpan.TicksPerHour + 20 * TimeSpan.TicksPerMinute +
				5 * TimeSpan.TicksPerSecond + 14 * TimeSpan.TicksPerMillisecond);
		}

		[Test]
		public void CompareToWorks()
		{
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 14);
			var time3 = new TimeSpan(15, 11, 20, 5, 14);
			Assert.AreEqual(0, time1.CompareTo(time1));
			Assert.AreEqual(1, time1.CompareTo(time2));
			Assert.AreEqual(-1, time1.CompareTo(time3));
		}

		[Test]
		public void ToStringWorks()
		{
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 2);
			var time3 = new TimeSpan(15, 11, 20, 6, 14);
			Assert.AreEqual("15.10:20:05.0140000", time1.ToString());
			Assert.AreEqual("14.10:20:05.0020000", time2.ToString());
			Assert.AreEqual("15.11:20:06.0140000", time3.ToString());
		}

		private void AssertAlmostEqual(double d1, double d2)
		{
			var diff = d2 - d1;
			if (diff < 0)
				diff = -diff;
			Assert.IsTrue(diff < 1e-8);
		}
	}
}
