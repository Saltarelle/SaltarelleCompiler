using System;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class TimeSpanTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(TimeSpan).FullName, "ss.TimeSpan");
			Assert.IsFalse(typeof(TimeSpan).IsClass);
			Assert.IsTrue(typeof(IComparable<TimeSpan>).IsAssignableFrom(typeof(TimeSpan)));
			Assert.IsTrue(typeof(IEquatable<TimeSpan>).IsAssignableFrom(typeof(TimeSpan)));
			object d = new TimeSpan();
			Assert.IsTrue(d is TimeSpan);
			Assert.IsTrue(d is IComparable<TimeSpan>);
			Assert.IsTrue(d is IEquatable<TimeSpan>);

			var interfaces = typeof(TimeSpan).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<DateTime>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<DateTime>)));
		}

		[Test]
		public void DefaultConstructorWorks() {
			var time = new TimeSpan();
			Assert.AreEqual(time.Ticks, 0);
		}

		[Test]
		public void DefaultValueWorks() {
			var ts = default(TimeSpan);
			Assert.AreEqual(ts.Ticks, 0);
		}

		[Test]
		public void CreatingInstanceReturnsTimeSpanWithZeroValue() {
			var ts = Activator.CreateInstance<TimeSpan>();
			Assert.AreEqual(ts.Ticks, 0);
		}

		[Test]
		public void ParameterConstructorsWorks() {
			var time = new TimeSpan(34567L);
			Assert.AreEqual(time.Ticks, 34567);
			time = new TimeSpan(10, 20, 5);
			Assert.AreEqual(time.Ticks, 372050000000);
			time = new TimeSpan(15, 10, 20, 5);
			Assert.AreEqual(time.Ticks, 13332050000000);
			time = new TimeSpan(15, 10, 20, 5, 14);
			Assert.AreEqual(time.Ticks, 13332050140000);
		}

		[Test]
		public void PropertiesWork() {
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
			Assert.AreEqual(time.Ticks, 15 * TimeSpan.TicksPerDay + 10 * TimeSpan.TicksPerHour + 20 * TimeSpan.TicksPerMinute + 5 * TimeSpan.TicksPerSecond + 14 * TimeSpan.TicksPerMillisecond);
		}

		[Test]
		public void CompareToWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 14);
			var time3 = new TimeSpan(15, 11, 20, 5, 14);
			Assert.AreEqual(0, time1.CompareTo(time1));
			Assert.AreEqual(1, time1.CompareTo(time2));
			Assert.AreEqual(-1, time1.CompareTo(time3));
		}

		[Test]
		public void EqualsWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 14);
			var time3 = new TimeSpan(15, 10, 20, 5, 14);

			Assert.IsFalse(time1.Equals(time2));
			Assert.IsTrue (time1.Equals(time3));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 14);
			var time3 = new TimeSpan(15, 10, 20, 5, 14);

			Assert.IsFalse(((IEquatable<TimeSpan>)time1).Equals(time2));
			Assert.IsTrue (((IEquatable<TimeSpan>)time1).Equals(time3));
		}

		[Test]
		public void ToStringWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 2);
			var time3 = new TimeSpan(15, 11, 20, 6, 14);
			var time4 = new TimeSpan(1, 2, 3);
			Assert.AreEqual(time1.ToString(), "15.10:20:05.0140000");
			Assert.AreEqual(time2.ToString(), "14.10:20:05.0020000");
			Assert.AreEqual(time3.ToString(), "15.11:20:06.0140000");
			Assert.AreEqual(time4.ToString(), "01:02:03");
		}

		private void AssertAlmostEqual(double d1, double d2) {
			var diff = d2 - d1;
			if (diff < 0)
				diff = -diff;
			Assert.IsTrue(diff < 1e-8);
		}
	}
}
