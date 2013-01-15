using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class MathTests {
		private void AssertAlmostEqual(double d1, double d2) {
			var diff = d2 - d1;
			if (diff < 0)
				diff = -diff;
			Assert.IsTrue(diff < 1e-8);
		}

		[Test]
		public void ConstantsWork() {
			AssertAlmostEqual(Math.E, 2.718281828459045);
			AssertAlmostEqual(Math.LN2, 0.6931471805599453);
			AssertAlmostEqual(Math.LN10, 2.302585092994046);
			AssertAlmostEqual(Math.LOG2E, 1.4426950408889634);
			AssertAlmostEqual(Math.LOG10E, 0.4342944819032518);
			AssertAlmostEqual(Math.PI, 3.141592653589793);
			AssertAlmostEqual(Math.SQRT1_2, 0.7071067811865476);
			AssertAlmostEqual(Math.SQRT2, 1.4142135623730951);
		}

		[Test]
		public void AbsOfDoubleWorks() {
			Assert.AreEqual(Math.Abs(-12.5), 12.5);
		}

		[Test]
		public void AbsOfIntWorks() {
			Assert.AreEqual(Math.Abs(-12), 12);
		}

		[Test]
		public void AbsOfLongWorks() {
			Assert.AreEqual(Math.Abs(-12L), 12L);
		}

		[Test]
		public void AcosWorks() {
			AssertAlmostEqual(Math.Acos(0.5), 1.0471975511965979);
		}

		[Test]
		public void AsinWorks() {
			AssertAlmostEqual(Math.Asin(0.5), 0.5235987755982989);
		}

		[Test]
		public void AtanWorks() {
			AssertAlmostEqual(Math.Atan(0.5), 0.4636476090008061);
		}

		[Test]
		public void Atan2Works() {
			AssertAlmostEqual(Math.Atan2(1, 2), 0.4636476090008061);
		}

		[Test]
		public void CeilingWorks() {
			Assert.AreEqual(Math.Ceiling(3.2), 4.0);
			Assert.AreEqual(Math.Ceiling(-3.2), -3.0);
		}

		[Test]
		public void CosWorks() {
			AssertAlmostEqual(Math.Cos(0.5), 0.8775825618903728);
		}

		[Test]
		public void ExpWorks() {
			AssertAlmostEqual(Math.Exp(0.5), 1.6487212707001282);
		}

		[Test]
		public void FloorWorks() {
			Assert.AreEqual(Math.Floor(3.6), 3.0);
			Assert.AreEqual(Math.Floor(-3.6), -4.0);
		}

		[Test]
		public void LogWorks() {
			AssertAlmostEqual(Math.Log(0.5), -0.6931471805599453);
		}

		[Test]
		public void MaxOfDoubleWorks() {
			Assert.AreEqual(Math.Max(1.0), 1.0);
			Assert.AreEqual(Math.Max(1.0, 3.0), 3.0);
			Assert.AreEqual(Math.Max(1.0, 3.0, 2.0), 3.0);
		}

		[Test]
		public void MaxOfIntWorks() {
			Assert.AreEqual(Math.Max(1), 1);
			Assert.AreEqual(Math.Max(1, 3), 3);
			Assert.AreEqual(Math.Max(1, 3, 2), 3);
		}

		[Test]
		public void MaxOfLongWorks() {
			Assert.AreEqual(Math.Max(1L), 1L);
			Assert.AreEqual(Math.Max(1L, 3L), 3L);
			Assert.AreEqual(Math.Max(1L, 3L, 2L), 3L);
		}

		[Test]
		public void MinOfDoubleWorks() {
			Assert.AreEqual(Math.Min(1.0), 1.0);
			Assert.AreEqual(Math.Min(3.0, 1.0), 1.0);
			Assert.AreEqual(Math.Min(3.0, 1.0, 2.0), 1.0);
		}

		[Test]
		public void MinOfIntWorks() {
			Assert.AreEqual(Math.Min(1), 1);
			Assert.AreEqual(Math.Min(3, 1), 1);
			Assert.AreEqual(Math.Min(3, 1, 2), 1);
		}

		[Test]
		public void MinOfLongWorks() {
			Assert.AreEqual(Math.Min(1L), 1L);
			Assert.AreEqual(Math.Min(3L, 1L), 1L);
			Assert.AreEqual(Math.Min(3L, 1L, 2L), 1L);
		}

		[Test]
		public void PowWorks() {
			AssertAlmostEqual(Math.Pow(3, 0.5), 1.7320508075688772);
		}

		[Test]
		public void RandomWorks() {
			for (int i = 0; i < 5; i++) {
				double d = Math.Random();
				Assert.IsTrue(d >= 0);
				Assert.IsTrue(d < 1);
			}
		}

		[Test]
		public void RoundWorks() {
			Assert.AreEqual(Math.Round(3.4), 3.0);
			Assert.AreEqual(Math.Round(3.6), 4.0);
		}

		[Test]
		public void SinWorks() {
			AssertAlmostEqual(Math.Sin(0.5), 0.479425538604203);
		}

		[Test]
		public void SqrtWorks() {
			AssertAlmostEqual(Math.Sqrt(3), 1.7320508075688772);
		}

		[Test]
		public void TanWorks() {
			AssertAlmostEqual(Math.Tan(0.5), 0.5463024898437905);
		}

		[Test]
		public void TruncateWorks() {
			Assert.AreEqual(Math.Truncate(3.9), 3.0);
			Assert.AreEqual(Math.Truncate(-3.9), -3.0);
		}
	}
}
