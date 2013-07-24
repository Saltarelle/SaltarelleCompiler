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
        public void ZeroWorks()
        {
            var ts = TimeSpan.Zero;
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
			Assert.IsTrue((object)time is TimeSpan, "ticks type");
			Assert.AreEqual(time.Ticks, 34567, "ticks value");

			time = new TimeSpan(10, 20, 5);
			Assert.IsTrue((object)time is TimeSpan, "h, m, s type");
			Assert.AreEqual(time.Ticks, 372050000000, "h, m, s value");

			time = new TimeSpan(15, 10, 20, 5);
			Assert.IsTrue((object)time is TimeSpan, "d, h, m, s type");
			Assert.AreEqual(time.Ticks, 13332050000000, "d, h, m, s value");

			time = new TimeSpan(15, 10, 20, 5, 14);
			Assert.IsTrue((object)time is TimeSpan, "full type");
			Assert.AreEqual(time.Ticks, 13332050140000, "full value");
		}

		[Test]
		public void FactoryMethodsWork() {
			var time = TimeSpan.FromDays(3);
			Assert.IsTrue((object)time is TimeSpan, "FromDays type");
			Assert.AreEqual(time.Ticks, 2592000000000, "FromDays value");

			time = TimeSpan.FromHours(3);
			Assert.IsTrue((object)time is TimeSpan, "FromHours type");
			Assert.AreEqual(time.Ticks, 108000000000, "FromHours value");

			time = TimeSpan.FromMinutes(3);
			Assert.IsTrue((object)time is TimeSpan, "FromMinutes type");
			Assert.AreEqual(time.Ticks, 1800000000, "FromMinutes value");

			time = TimeSpan.FromSeconds(3);
			Assert.IsTrue((object)time is TimeSpan, "FromSeconds type");
			Assert.AreEqual(time.Ticks, 30000000, "FromSeconds value");

			time = TimeSpan.FromMilliseconds(3);
			Assert.IsTrue((object)time is TimeSpan, "FromMilliseconds type");
			Assert.AreEqual(time.Ticks, 30000, "FromMilliseconds value");

			time = TimeSpan.FromTicks(3);
			Assert.IsTrue((object)time is TimeSpan, "FromTicks type");
			Assert.AreEqual(time.Ticks, 3, "FromTicks value");
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
			var time2 = new TimeSpan(15, 10, 20, 5, 14);
			var time3 = new TimeSpan(14, 10, 20, 5, 14);
			var time4 = new TimeSpan(15, 11, 20, 5, 14);
			Assert.AreEqual(0, time1.CompareTo(time1));
			Assert.AreEqual(0, time1.CompareTo(time2));
			Assert.AreEqual(1, time1.CompareTo(time3));
			Assert.AreEqual(-1, time1.CompareTo(time4));
		}

		[Test]
		public void CompareWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(15, 10, 20, 5, 14);
			var time3 = new TimeSpan(14, 10, 20, 5, 14);
			var time4 = new TimeSpan(15, 11, 20, 5, 14);
			Assert.AreEqual(0, TimeSpan.Compare(time1, time1));
			Assert.AreEqual(0, TimeSpan.Compare(time1, time2));
			Assert.AreEqual(1, TimeSpan.Compare(time1, time3));
			Assert.AreEqual(-1, TimeSpan.Compare(time1, time4));
		}

		[Test]
		public void StaticEqualsWorks() {
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(14, 10, 20, 5, 14);
			var time3 = new TimeSpan(15, 10, 20, 5, 14);

			Assert.IsFalse(TimeSpan.Equals(time1, time2));
			Assert.IsTrue (TimeSpan.Equals(time1, time3));
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

		[Test]
		public void AddWorks() {
			var time1 = new TimeSpan(2, 3, 4, 5, 6);
			var time2 = new TimeSpan(3, 4, 5, 6, 7);
			TimeSpan actual = time1.Add(time2);
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, ((((((5 * 24) + 7) * 60) + 9) * 60) + 11) * 1000 + 13, "TotalMilliseconds should be correct");
		}

		[Test]
		public void SubtractWorks() {
			var time1 = new TimeSpan(4, 3, 7, 2, 6);
			var time2 = new TimeSpan(3, 4, 5, 6, 7);
			TimeSpan actual = time1.Subtract(time2);
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, ((((((1 * 24) - 1) * 60) + 2) * 60) - 4) * 1000 - 1, "TotalMilliseconds should be correct");
		}

		[Test]
		public void DurationWorks() {
			var time1 = new TimeSpan(-3, -2, -1, -5, -4);
			var time2 = new TimeSpan( 2,  1,  5,  4,  3);
			TimeSpan actual1 = time1.Duration();
			TimeSpan actual2 = time2.Duration();
			Assert.IsTrue((object)time1 is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual1.TotalMilliseconds, (((((3 * 24) + 2) * 60 + 1) * 60) + 5) * 1000 + 4, "Negative should be negated");
			Assert.AreEqual(actual2.TotalMilliseconds, (((((2 * 24) + 1) * 60 + 5) * 60) + 4) * 1000 + 3, "Positive should be preserved");
		}

		[Test]
		public void NegateWorks() {
			var time = new TimeSpan(-3, 2, -1, 5, -4);
			TimeSpan actual = time.Negate();
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, (((((3 * 24) - 2) * 60 + 1) * 60) - 5) * 1000 + 4, "Ticks should be correct");
		}

		private void AssertAlmostEqual(double d1, double d2) {
			var diff = d2 - d1;
			if (diff < 0)
				diff = -diff;
			Assert.IsTrue(diff < 1e-8);
		}

		[Test]
		public void ComparisonOperatorsWork() {
#pragma warning disable 1718
			var time1 = new TimeSpan(15, 10, 20, 5, 14);
			var time2 = new TimeSpan(15, 10, 20, 5, 14);
			var time3 = new TimeSpan(14, 10, 20, 5, 14);
			var time4 = new TimeSpan(15, 11, 20, 5, 14);

			Assert.IsFalse(time1 > time2, "> 1");
			Assert.IsTrue (time1 > time3, "> 2");
			Assert.IsFalse(time1 > time4, "> 3");

			Assert.IsTrue (time1 >= time2, ">= 1");
			Assert.IsTrue (time1 >= time3, ">= 2");
			Assert.IsFalse(time1 >= time4, ">= 3");

			Assert.IsFalse(time1 < time2, "< 1");
			Assert.IsFalse(time1 < time3, "< 2");
			Assert.IsTrue (time1 < time4, "< 3");

			Assert.IsTrue (time1 <= time2, "<= 1");
			Assert.IsFalse(time1 <= time3, "<= 2");
			Assert.IsTrue (time1 <= time4, "<= 3");

			Assert.IsTrue (time1 == time1, "== 1");
			Assert.IsTrue (time1 == time2, "== 2");
			Assert.IsFalse(time1 == time3, "== 3");
			Assert.IsFalse(time1 == time4, "== 4");

			Assert.IsFalse(time1 != time1, "!= 1");
			Assert.IsFalse(time1 != time2, "!= 2");
			Assert.IsTrue (time1 != time3, "!= 3");
			Assert.IsTrue (time1 != time4, "!= 4");
#pragma warning restore 1718
		}

		[Test]
		public void AdditionOperatorWorks() {
			var time1 = new TimeSpan(2, 3, 4, 5, 6);
			var time2 = new TimeSpan(3, 4, 5, 6, 7);
			TimeSpan actual = time1 + time2;
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, ((((((5 * 24) + 7) * 60) + 9) * 60) + 11) * 1000 + 13, "TotalMilliseconds should be correct");
		}

		[Test]
		public void SubtractionOperatorWorks() {
			var time1 = new TimeSpan(4, 3, 7, 2, 6);
			var time2 = new TimeSpan(3, 4, 5, 6, 7);
			TimeSpan actual = time1 - time2;
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, ((((((1 * 24) - 1) * 60) + 2) * 60) - 4) * 1000 - 1, "TotalMilliseconds should be correct");
		}

		[Test]
		public void UnaryPlusWorks() {
			var time = new TimeSpan(-3, 2, -1, 5, -4);
			TimeSpan actual = +time;
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, (((((-3 * 24) + 2) * 60 - 1) * 60) + 5) * 1000 - 4, "Ticks should be correct");
		}

		[Test]
		public void UnaryMinusWorks() {
			var time = new TimeSpan(-3, 2, -1, 5, -4);
			TimeSpan actual = -time;
			Assert.IsTrue((object)actual is TimeSpan, "Should be TimeSpan");
			Assert.AreEqual(actual.TotalMilliseconds, (((((3 * 24) - 2) * 60 + 1) * 60) - 5) * 1000 + 4, "Ticks should be correct");
		}
	}
}
