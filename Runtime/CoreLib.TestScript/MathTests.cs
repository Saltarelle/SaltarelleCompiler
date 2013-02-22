using System;
using QUnit;

namespace CoreLib.TestScript {
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
		public void AbsOfSbyteWorks()
		{
			Assert.AreEqual(Math.Abs((sbyte)-15), (sbyte)15);
		}

		[Test]
		public void AbsOfShortWorks()
		{
			Assert.AreEqual(Math.Abs((short)-15), (short)15);
		}

		[Test]
		public void AbsOfFloatWorks()
		{
			Assert.AreEqual(Math.Abs(-17.5f), 17.5f);
		}

		[Test]
		public void AbsOfDecimalWorks()
		{
			Assert.AreEqual(Math.Abs(-10.0m), 10.0m);
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
		public void CeilingOfDoubleWorks() {
			Assert.AreEqual(Math.Ceiling(3.2), 4.0);
			Assert.AreEqual(Math.Ceiling(-3.2), -3.0);
		}

		[Test]
		public void CeilingOfDecimalWorks()
		{
			Assert.AreEqual(Math.Ceiling(3.2m), 4.0m);
			Assert.AreEqual(Math.Ceiling(-3.2m), -3.0m);
		}

		[Test]
		public void CosWorks() {
			AssertAlmostEqual(Math.Cos(0.5), 0.8775825618903728);
		}

		[Test]
		public void CoshWorks()
		{
			AssertAlmostEqual(Math.Cosh(0.1), 1.0050041680558035E+000);
		}

		[Test]
		public void SinhWorks()
		{
			AssertAlmostEqual(Math.Sinh(-0.98343), -1.1497925156481d);
		}

		[Test]
		public void TanhWorks()
		{
			AssertAlmostEqual(Math.Tanh(5.4251848), 0.999961205877d);
		}

		[Test]
		public void ExpWorks() {
			AssertAlmostEqual(Math.Exp(0.5), 1.6487212707001282);
		}

		[Test]
		public void FloorOfDoubleWorks() {
			Assert.AreEqual(Math.Floor(3.6), 3.0);
			Assert.AreEqual(Math.Floor(-3.6), -4.0);
		}

		[Test]
		public void FloorOfDecimalWorks()
		{
			Assert.AreEqual(Math.Floor(3.6m), 3.0m);
			Assert.AreEqual(Math.Floor(-3.6m), -4.0m);
		}

		[Test]
		public void LogWorks() {
			AssertAlmostEqual(Math.Log(0.5), -0.6931471805599453);
		}

		[Test]
		public void LogWithBaseWorks()
		{
			Assert.AreEqual(Math.Log(16, 2), 4.0);
			Assert.AreEqual(Math.Log(16, 4), 2.0);
		}

		[Test]
		public void Log10Works()
		{
			Assert.AreEqual(Math.Log10(10), 1.0);
			Assert.AreEqual(Math.Log10(100), 2.0);
		}

		[Test]
		public void MaxOfByteWorks()
		{
			Assert.AreEqual(Math.Max((byte)1, (byte)3), 3.0);
			Assert.AreEqual(Math.Max((byte)5, (byte)3), 5.0);
		}

		[Test]
		public void MaxOfDecimalWorks()
		{
			Assert.AreEqual(Math.Max(-14.5m, 3.0m), 3.0m);
			Assert.AreEqual(Math.Max(5.4m, 3.0m), 5.4m);
		}

		[Test]
		public void MaxOfDoubleWorks()
		{
			Assert.AreEqual(Math.Max(1.0, 3.0), 3.0);
			Assert.AreEqual(Math.Max(4.0, 3.0), 4.0);
		}

		[Test]
		public void MaxOfShortWorks()
		{
			Assert.AreEqual(Math.Max((short)1, (short)3), (short)3);
			Assert.AreEqual(Math.Max((short)4, (short)3), (short)4);
		}

		[Test]
		public void MaxOfIntWorks()
		{
			Assert.AreEqual(Math.Max(1, 3), 3);
			Assert.AreEqual(Math.Max(4, 3), 4);
		}

		[Test]
		public void MaxOfLongWorks()
		{
			Assert.AreEqual(Math.Max(1L, 3L), 3L);
			Assert.AreEqual(Math.Max(4L, 3L), 4L);
		}

		[Test]
		public void MaxOfSByteWorks()
		{
			Assert.AreEqual(Math.Max((sbyte)-1, (sbyte)3), (sbyte)3);
			Assert.AreEqual(Math.Max((sbyte)5, (sbyte)3), (sbyte)5);
		}

		[Test]
		public void MaxOfFloatWorks()
		{
			Assert.AreEqual(Math.Max(-14.5f, 3.0f), 3.0f);
			Assert.AreEqual(Math.Max(5.4f, 3.0f), 5.4f);
		}

		[Test]
		public void MaxOfUShortWorks()
		{
			Assert.AreEqual(Math.Max((ushort)1, (ushort)3), (ushort)3);
			Assert.AreEqual(Math.Max((ushort)5, (ushort)3), (ushort)5);
		}

		[Test]
		public void MaxOfUIntWorks()
		{
			Assert.AreEqual(Math.Max((uint)1, (uint)3), (uint)3);
			Assert.AreEqual(Math.Max((uint)5, (uint)3), (uint)5);
		}

		[Test]
		public void MaxOfULongWorks()
		{
			Assert.AreEqual(Math.Max((ulong)100, (ulong)300), (ulong)300);
			Assert.AreEqual(Math.Max((ulong)500, (ulong)300), (ulong)500);
		}

		[Test]
		public void MinOfByteWorks()
		{
			Assert.AreEqual(Math.Min((byte)1, (byte)3), 1.0);
			Assert.AreEqual(Math.Min((byte)5, (byte)3), 3.0);
		}

		[Test]
		public void MinOfDecimalWorks()
		{
			Assert.AreEqual(Math.Min(-14.5m, 3.0m), -14.5m);
			Assert.AreEqual(Math.Min(5.4m, 3.0m), 3.0m);
		}

		[Test]
		public void MinOfDoubleWorks()
		{
			Assert.AreEqual(Math.Min(1.0, 3.0), 1.0);
			Assert.AreEqual(Math.Min(4.0, 3.0), 3.0);
		}

		[Test]
		public void MinOfShortWorks()
		{
			Assert.AreEqual(Math.Min((short)1, (short)3), (short)1);
			Assert.AreEqual(Math.Min((short)4, (short)3), (short)3);
		}

		[Test]
		public void MinOfIntWorks()
		{
			Assert.AreEqual(Math.Min(1, 3), 1);
			Assert.AreEqual(Math.Min(4, 3), 3);
		}

		[Test]
		public void MinOfLongWorks()
		{
			Assert.AreEqual(Math.Min(1L, 3L), 1L);
			Assert.AreEqual(Math.Min(4L, 3L), 3L);
		}

		[Test]
		public void MinOfSByteWorks()
		{
			Assert.AreEqual(Math.Min((sbyte)-1, (sbyte)3), (sbyte)-1);
			Assert.AreEqual(Math.Min((sbyte)5, (sbyte)3), (sbyte)3);
		}

		[Test]
		public void MinOfFloatWorks()
		{
			Assert.AreEqual(Math.Min(-14.5f, 3.0f), -14.5f);
			Assert.AreEqual(Math.Min(5.4f, 3.0f), 3.0f);
		}

		[Test]
		public void MinOfUShortWorks()
		{
			Assert.AreEqual(Math.Min((ushort)1, (ushort)3), (ushort)1);
			Assert.AreEqual(Math.Min((ushort)5, (ushort)3), (ushort)3);
		}

		[Test]
		public void MinOfUIntWorks()
		{
			Assert.AreEqual(Math.Min((uint)1, (uint)3), (uint)1);
			Assert.AreEqual(Math.Min((uint)5, (uint)3), (uint)3);
		}

		[Test]
		public void MinOfULongWorks()
		{
			Assert.AreEqual(Math.Min((ulong)100, (ulong)300), (ulong)100);
			Assert.AreEqual(Math.Min((ulong)500, (ulong)300), (ulong)300);
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
		public void RoundOfDoubleWorks()
		{
			Assert.AreEqual(Math.Round(3.432), 3.0);
			Assert.AreEqual(Math.Round(3.6), 4.0);
		}

		[Test]
		public void RoundOfDecimalWorks()
		{
			Assert.AreEqual(Math.Round(3.432m), 3.0m);
			Assert.AreEqual(Math.Round(3.6m), 4.0m);
		}

		[Test]
		public void RoundOfDoubleWithDigitsWorks()
		{
			Assert.AreEqual(Math.Round(3.432, 2), 3.43);
			Assert.AreEqual(Math.Round(3.6, 0), 4.0);
			Assert.AreEqual(Math.Round(3.35, 1), 3.4);
		}

		[Test]
		public void RoundOfDecimalWithDigitsWorks()
		{
			Assert.AreEqual(Math.Round(3.432m, 2), 3.43m);
			Assert.AreEqual(Math.Round(3.6m, 0), 4.0m);
			Assert.AreEqual(Math.Round(3.35m, 1), 3.4m);
		}

		[Test]
		public void SignWithDecimalWorks()
		{
			Assert.AreEqual(Math.Sign(-0.5m), -1);
			Assert.AreEqual(Math.Sign(0.0m), 0);
			Assert.AreEqual(Math.Sign(3.35m), 1);
		}

		[Test]
		public void SignWithDoubleWorks()
		{
			Assert.AreEqual(Math.Sign(-0.5), -1);
			Assert.AreEqual(Math.Sign(0.0), 0);
			Assert.AreEqual(Math.Sign(3.35), 1);
		}

		[Test]
		public void SignWithShortWorks()
		{
			Assert.AreEqual(Math.Sign((short)-15), -1);
			Assert.AreEqual(Math.Sign((short)0), 0);
			Assert.AreEqual(Math.Sign((short)4), 1);
		}

		[Test]
		public void SignWithIntWorks()
		{
			Assert.AreEqual(Math.Sign(-15), -1);
			Assert.AreEqual(Math.Sign(0), 0);
			Assert.AreEqual(Math.Sign(4), 1);
		}

		[Test]
		public void SignWithLongWorks()
		{
			Assert.AreEqual(Math.Sign(-15L), -1);
			Assert.AreEqual(Math.Sign(0L), 0);
			Assert.AreEqual(Math.Sign(4L), 1);
		}

		[Test]
		public void SignWithSByteWorks()
		{
			Assert.AreEqual(Math.Sign((sbyte)-15), -1);
			Assert.AreEqual(Math.Sign((sbyte)0), 0);
			Assert.AreEqual(Math.Sign((sbyte)4), 1);
		}

		[Test]
		public void SignWithFloatWorks()
		{
			Assert.AreEqual(Math.Sign(-0.5f), -1);
			Assert.AreEqual(Math.Sign(0.0f), 0);
			Assert.AreEqual(Math.Sign(3.35f), 1);
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
		public void TruncateWithDoubleWorks() {
			Assert.AreEqual(Math.Truncate(3.9), 3.0);
			Assert.AreEqual(Math.Truncate(-3.9), -3.0);
		}

		[Test]
		public void TruncateWithDecimalWorks()
		{
			Assert.AreEqual(Math.Truncate(3.9m), 3.0m);
			Assert.AreEqual(Math.Truncate(-3.9m), -3.0m);
		}

		[Test]
		public void IEEERemainderWorks()
		{
			Assert.AreEqual(Math.IEEERemainder(3.0, 2.0), -1.0);
			Assert.AreEqual(Math.IEEERemainder(4.0, 2.0), 0.0);
			Assert.AreEqual(Math.IEEERemainder(10.0, 3.0), 1.0);
			Assert.AreEqual(Math.IEEERemainder(11.0, 3.0), -1.0);
			Assert.AreEqual(Math.IEEERemainder(27.0, 4.0), -1.0);
			Assert.AreEqual(Math.IEEERemainder(28.0, 5.0), -2.0);
			AssertAlmostEqual(Math.IEEERemainder(17.8, 4.0), 1.8);
			AssertAlmostEqual(Math.IEEERemainder(17.8, 4.1), 1.4);
			AssertAlmostEqual(Math.IEEERemainder(-16.3, 4.1), 0.0999999999999979);
			AssertAlmostEqual(Math.IEEERemainder(17.8, -4.1), 1.4);
			AssertAlmostEqual(Math.IEEERemainder(-17.8, -4.1), -1.4);
		}

		[Test]
		public void RoundOfDoubleWithMidpointRoundingWorks()
		{
			Assert.AreEqual(Math.Round(3.432, MidpointRounding.AwayFromZero), 3.0);
			Assert.AreEqual(Math.Round(3.432, MidpointRounding.ToEven), 3.0);
			Assert.AreEqual(Math.Round(3.5, MidpointRounding.AwayFromZero), 4.0);
			Assert.AreEqual(Math.Round(3.5, MidpointRounding.ToEven), 4.0);
			Assert.AreEqual(Math.Round(3.64, MidpointRounding.AwayFromZero), 4.0);
			Assert.AreEqual(Math.Round(3.64, MidpointRounding.ToEven), 4.0);
			Assert.AreEqual(Math.Round(2.5, MidpointRounding.AwayFromZero), 3.0);
			Assert.AreEqual(Math.Round(2.5, MidpointRounding.ToEven), 2.0);
			Assert.AreEqual(Math.Round(-2.5, MidpointRounding.AwayFromZero), -3.0);
			Assert.AreEqual(Math.Round(-2.5, MidpointRounding.ToEven), -2.0);
		}

		[Test]
		public void RoundOfDecimalWithMidpointRoundingWorks()
		{
			Assert.AreEqual(Math.Round(3.432m, MidpointRounding.AwayFromZero), 3.0m);
			Assert.AreEqual(Math.Round(3.432m, MidpointRounding.ToEven), 3.0m);
			Assert.AreEqual(Math.Round(3.5m, MidpointRounding.AwayFromZero), 4.0m);
			Assert.AreEqual(Math.Round(3.5m, MidpointRounding.ToEven), 4.0m);
			Assert.AreEqual(Math.Round(3.64m, MidpointRounding.AwayFromZero), 4.0m);
			Assert.AreEqual(Math.Round(3.64m, MidpointRounding.ToEven), 4.0m);
			Assert.AreEqual(Math.Round(2.5m, MidpointRounding.AwayFromZero), 3.0m);
			Assert.AreEqual(Math.Round(2.5m, MidpointRounding.ToEven), 2.0m);
			Assert.AreEqual(Math.Round(-2.5m, MidpointRounding.AwayFromZero), -3.0m);
			Assert.AreEqual(Math.Round(-2.5m, MidpointRounding.ToEven), -2.0m);
		}

		[Test]
		public void RoundOfDoubleWithDigitsAndMidpointRoundingWorks()
		{
			Assert.AreEqual(Math.Round(3.45, 1, MidpointRounding.AwayFromZero), 3.5);
			Assert.AreEqual(Math.Round(3.45, 1, MidpointRounding.ToEven), 3.4);
			Assert.AreEqual(Math.Round(3.5, 0, MidpointRounding.AwayFromZero), 4.0);
			Assert.AreEqual(Math.Round(3.5, 0, MidpointRounding.ToEven), 4.0);
			Assert.AreEqual(Math.Round(3.645, 2, MidpointRounding.AwayFromZero), 3.65);
			Assert.AreEqual(Math.Round(3.645, 2, MidpointRounding.ToEven), 3.64);
			Assert.AreEqual(Math.Round(2.5, 0, MidpointRounding.AwayFromZero), 3.0);
			Assert.AreEqual(Math.Round(2.5, 0, MidpointRounding.ToEven), 2.0);
			Assert.AreEqual(Math.Round(-2.5, 1, MidpointRounding.AwayFromZero), -2.5);
			Assert.AreEqual(Math.Round(-2.5, 1, MidpointRounding.ToEven), -2.5);
		}

		[Test]
		public void RoundOfDecimalWithDigitsAndMidpointRoundingWorks()
		{
			Assert.AreEqual(Math.Round(3.45m, 1, MidpointRounding.AwayFromZero), 3.5m);
			Assert.AreEqual(Math.Round(3.45m, 1, MidpointRounding.ToEven), 3.4m);
			Assert.AreEqual(Math.Round(3.5m, 0, MidpointRounding.AwayFromZero), 4.0m);
			Assert.AreEqual(Math.Round(3.5m, 0, MidpointRounding.ToEven), 4.0m);
			Assert.AreEqual(Math.Round(3.645m, 2, MidpointRounding.AwayFromZero), 3.65m);
			Assert.AreEqual(Math.Round(3.645m, 2, MidpointRounding.ToEven), 3.64m);
			Assert.AreEqual(Math.Round(2.5m, 0, MidpointRounding.AwayFromZero), 3.0m);
			Assert.AreEqual(Math.Round(2.5m, 0, MidpointRounding.ToEven), 2.0m);
			Assert.AreEqual(Math.Round(-2.5m, 1, MidpointRounding.AwayFromZero), -2.5m);
			Assert.AreEqual(Math.Round(-2.5m, 1, MidpointRounding.ToEven), -2.5m);
		}

		[Test]
		public void BigMulWorks()
		{
			// TODO: doesn't work cause of wrong long type
			//Assert.AreEqual(Math.BigMul(Int32.MaxValue, Int32.MaxValue), 4611686014132420609L);
			Assert.AreEqual(Math.BigMul(214748364, 214748364), 46116859840676496L);
		}

		[Test]
		public void DivRemWorks()
		{
			int result;
			Assert.AreEqual(Math.DivRem(2147483647, 2, out result), 1073741823);
			Assert.AreEqual(result, 1);
			long longResult;
			// TODO: doesn't work cause of wrong long type
			//Assert.AreEqual(Math.DivRem(9223372036854775807L, 4L, out longResult), 2305843009213693951L);
			//Assert.AreEqual(longResult, 3L);
			Assert.AreEqual(Math.DivRem(92233720368547L, 4L, out longResult), 23058430092136L);
			Assert.AreEqual(longResult, 3L);
		}
	}
}
