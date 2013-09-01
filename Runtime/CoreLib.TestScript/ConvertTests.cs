using System;
using System.Globalization;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ConvertTests {
		private void AssertAlmostEqual(double d1, double d2) {
			var diff = d2 - d1;
			if (diff < 0)
				diff = -diff;
			Assert.IsTrue(diff < 1e-6);
		}

		[Test]
		public void ToSingleWorks() {
			Assert.AreEqual(Convert.ToSingle(true), 1f);
			Assert.AreEqual(Convert.ToSingle(false), 0f);
			AssertAlmostEqual(Convert.ToSingle(1.45f), 1.45);
			Assert.AreEqual(Convert.ToSingle((byte)244), 244f);
			Assert.AreEqual(Convert.ToSingle((sbyte)-14), -14f);
			Assert.AreEqual(Convert.ToSingle(14.5m), 14.5f);
			Assert.IsTrue(Math.Abs(Convert.ToSingle(0.45) - 0.45f) < 0.1f);
			Assert.AreEqual(Convert.ToSingle((int)-14), -14f);
			Assert.AreEqual(Convert.ToSingle((uint)165), 165f);
			Assert.AreEqual(Convert.ToSingle((short)-13), -13f);
			Assert.AreEqual(Convert.ToSingle((ushort)13), 13f);
			Assert.AreEqual(Convert.ToSingle((long)-2345), -2345f);
			Assert.AreEqual(Convert.ToSingle((ulong)2345), 2345f);
			Assert.AreEqual(Convert.ToSingle((object)-1456), -1456f);
			Assert.IsTrue(
				Math.Abs(Convert.ToSingle("-45.245", NumberFormatInfo.InvariantInfo) + 45.245f) < 0.1f);
			Assert.IsTrue(Math.Abs(
				Convert.ToSingle((object)"-45.245", NumberFormatInfo.InvariantInfo) + 45.245f) < 0.1f);
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual(Convert.ToString("Hello world!"), "Hello world!");
		}

		[Test]
		public void ToInt32Works() {
			Assert.AreEqual(Convert.ToInt32("3590"), 3590);
			Assert.AreEqual(Convert.ToInt32("-3590"), -3590);
		}

		[Test]
		public void ToBooleanWorks() {
			Assert.AreEqual(Convert.ToBoolean("true"), true);
			Assert.AreEqual(Convert.ToBoolean("True"), true);
		}

		[Test]
		public void ToDoubleWorks() {
			Assert.AreEqual(Convert.ToDouble("34.6405904", CultureInfo.InvariantCulture), 34.6405904);
			Assert.AreEqual(Convert.ToDouble("-34.6405904", CultureInfo.InvariantCulture), -34.6405904);
		}

		[Test]
		public void ToCharWorks() {
			Assert.AreEqual((int)Convert.ToChar("a"), 'a'.ToString());
		}
	}
}
