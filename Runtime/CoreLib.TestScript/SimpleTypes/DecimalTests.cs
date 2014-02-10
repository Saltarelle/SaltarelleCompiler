using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.SimpleTypes {
	[TestFixture]
	public class DecimalTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.IsTrue((object)(decimal)0.5 is decimal);
			Assert.AreEqual(typeof(decimal).FullName, "Number");
			Assert.IsFalse(typeof(decimal).IsClass);
			Assert.IsTrue(typeof(IComparable<decimal>).IsAssignableFrom(typeof(decimal)));
			Assert.IsTrue(typeof(IEquatable<decimal>).IsAssignableFrom(typeof(decimal)));
			Assert.IsTrue(typeof(IFormattable).IsAssignableFrom(typeof(decimal)));
			object d = (decimal)0;
			Assert.IsTrue(d is decimal);
			Assert.IsTrue(d is IComparable<decimal>);
			Assert.IsTrue(d is IEquatable<decimal>);
			Assert.IsTrue(d is IFormattable);

			var interfaces = typeof(decimal).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 4);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<decimal>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<decimal>)));
            Assert.IsTrue(interfaces.Contains(typeof(IFormattable)));
            Assert.IsTrue(interfaces.Contains(typeof(ILocaleFormattable)));
        }

		[IncludeGenericArguments]
		private T GetDefaultValue<T>() {
			return default(T);
		}

		[Test]
		public void DefaultValueIs0() {
			Assert.AreStrictEqual(GetDefaultValue<decimal>(), 0);
		}

		[Test]
		public void CreatingInstanceReturnsZero() {
			Assert.AreStrictEqual(Activator.CreateInstance<decimal>(), 0);
		}

		[Test]
		public void ConstantsWork() {
			Assert.AreEqual(decimal.One, 1);
			Assert.AreEqual(decimal.Zero, 0);
			Assert.AreEqual(decimal.MinusOne, -1);
		}

		[Test]
		public void DefaultConstructorReturnsZero() {
			Assert.AreStrictEqual(new Decimal(), 0);
		}

		[Test]
		public void ConvertingConstructorsWork() {
			Assert.AreEqual(new decimal((double)0.5), 0.5);
			Assert.AreEqual(new decimal((float)1.5), 1.5);
			Assert.AreEqual(new decimal((int)2), 2);
			Assert.AreEqual(new decimal((long)3), 3);
			Assert.AreEqual(new decimal((uint)4), 4);
			Assert.AreEqual(new decimal((ulong)5), 5);
		}

		[Test]
		public void FormatWorks() {
			Assert.AreEqual(291m.Format("x"), "123");
		}

		[Test]
		public void IFormattableToStringWorks() {
			Assert.AreEqual(291m.ToString("x"), "123");
			Assert.AreEqual(((IFormattable)291m).ToString("x"), "123");
		}

		[Test]
		public void LocaleFormatWorks() {
			Assert.AreEqual(291m.LocaleFormat("x"), "123");
		}

		[Test]
		public void ToStringWithoutRadixWorks() {
			Assert.AreEqual(123m.ToString(), "123");
		}

		[Test]
		public void ToStringWithRadixWorks() {
			Assert.AreEqual(291m.ToString(10), "291");
			Assert.AreEqual(291m.ToString(16), "123");
		}

		[Test]
		public void ToExponentialWorks() {
			Assert.AreEqual(123m.ToExponential(), "1.23e+2");
		}

		[Test]
		public void ToExponentialWithFractionalDigitsWorks() {
			Assert.AreEqual(123m.ToExponential(1), "1.2e+2");
		}

		[Test]
		public void ToFixed() {
			Assert.AreEqual(123m.ToFixed(), "123");
		}

		[Test]
		public void ToFixedWithFractionalDigitsWorks() {
			Assert.AreEqual(123m.ToFixed(1), "123.0");
		}

		[Test]
		public void ToPrecisionWorks() {
			Assert.AreEqual(12345m.ToPrecision(), "12345");
		}

		[Test]
		public void ToPrecisionWithPrecisionWorks() {
			Assert.AreEqual(12345m.ToPrecision(2), "1.2e+4");
		}

		[Test]
		public void ConversionsToDecimalWork() {
			int x = 0;
			Assert.AreEqual((decimal)(byte)(x + 1), 1m);
			Assert.AreEqual((decimal)(sbyte)(x + 2), 2m);
			Assert.AreEqual((decimal)(short)(x + 3), 3m);
			Assert.AreEqual((decimal)(ushort)(x + 4), 4m);
			Assert.AreEqual((decimal)(char)(x + '\x5'), 5m);
			Assert.AreEqual((decimal)(int)(x + 6), 6m);
			Assert.AreEqual((decimal)(uint)(x + 7), 7m);
			Assert.AreEqual((decimal)(long)(x + 8), 8m);
			Assert.AreEqual((decimal)(ulong)(x + 9), 9m);
			Assert.AreEqual((decimal)(float)(x + 10.5), 10.5m);
			Assert.AreEqual((decimal)(double)(x + 11.5), 11.5m);
		}

		[Test]
		public void ConversionsFromDecimalWork() {
			int x = 0;
			Assert.AreEqual((byte)(decimal)(x + 1), 1);
			Assert.AreEqual((sbyte)(decimal)(x + 2), 2);
			Assert.AreEqual((short)(decimal)(x + 3), 3);
			Assert.AreEqual((ushort)(decimal)(x + 4), 4);
			Assert.AreEqual((int)(char)(decimal)(x + '\x5'), 5);
			Assert.AreEqual((int)(decimal)(x + 6), 6);
			Assert.AreEqual((uint)(decimal)(x + 7), 7);
			Assert.AreEqual((long)(decimal)(x + 8), 8);
			Assert.AreEqual((ulong)(decimal)(x + 9), 9);
			Assert.AreEqual((float)(decimal)(x + 10.5), 10.5);
			Assert.AreEqual((double)(decimal)(x + 11.5), 11.5);
		}

		[Test]
		public void OperatorsWork() {
			decimal x = 3;
			Assert.AreEqual(+x, 3);
			Assert.AreEqual(-x, -3);
			Assert.AreEqual(x + 4m, 7);
			Assert.AreEqual(x - 2m, 1);
			Assert.AreEqual(x++, 3);
			Assert.AreEqual(++x, 5);
			Assert.AreEqual(x--, 5);
			Assert.AreEqual(--x, 3);
			Assert.AreEqual(x * 3m, 9);
			Assert.AreEqual(x / 2m, 1.5);
			Assert.AreEqual(14m % x, 2);
			Assert.IsTrue(x == 3m);
			Assert.IsFalse(x == 4m);
			Assert.IsFalse(x != 3m);
			Assert.IsTrue(x != 4m);
			Assert.IsTrue(x > 1m);
			Assert.IsFalse(x > 3m);
			Assert.IsTrue(x >= 3m);
			Assert.IsFalse(x >= 4m);
			Assert.IsTrue(x < 4m);
			Assert.IsFalse(x < 3m);
			Assert.IsTrue(x <= 3m);
			Assert.IsFalse(x <= 2m);
		}

		[Test]
		public void AddWorks() {
			Assert.AreEqual(decimal.Add(3m, 4m), 7m);
		}

		[Test]
		public void CeilingWorks() {
			Assert.AreEqual(decimal.Ceiling(3.4m), 4);
		}

		[Test]
		public void DivideWorks() {
			Assert.AreEqual(decimal.Divide(3m, 4m), 0.75);
		}

		[Test]
		public void FloorWorks() {
			Assert.AreEqual(decimal.Floor(3.2m), 3);
		}

		[Test]
		public void RemainderWorks() {
			Assert.AreEqual(decimal.Remainder(14m, 3m), 2);
		}

		[Test]
		public void MultiplyWorks() {
			Assert.AreEqual(decimal.Multiply(3m, 2m), 6);
		}

		[Test]
		public void NegateWorks() {
			Assert.AreEqual(decimal.Negate(3m), -3);
		}

		[Test]
		public void RoundWorks() {
			Assert.AreEqual(decimal.Round(3.2m), 3);
		}

		[Test]
		public void SubtractWorks() {
			Assert.AreEqual(decimal.Subtract(7m, 3m), 4);
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual   (((decimal)0).GetHashCode(), ((decimal)0).GetHashCode());
			Assert.AreEqual   (((decimal)1).GetHashCode(), ((decimal)1).GetHashCode());
			Assert.AreNotEqual(((decimal)0).GetHashCode(), ((decimal)1).GetHashCode());
			Assert.AreNotEqual(((decimal)0).GetHashCode(), ((decimal)0.5).GetHashCode());
		}

		[Test]
		public void EqualsWorks() {
			Assert.IsTrue (((decimal)0).Equals((object)(decimal)0));
			Assert.IsFalse(((decimal)1).Equals((object)(decimal)0));
			Assert.IsFalse(((decimal)0).Equals((object)(decimal)0.5));
			Assert.IsTrue (((decimal)1).Equals((object)(decimal)1));
		}

		[Test]
		public void IEquatableEqualsWorks() {
			Assert.IsTrue (((decimal)0).Equals((decimal)0));
			Assert.IsFalse(((decimal)1).Equals((decimal)0));
			Assert.IsFalse(((decimal)0).Equals((decimal)0.5));
			Assert.IsTrue (((decimal)1).Equals((decimal)1));

			Assert.IsTrue (((IEquatable<decimal>)((decimal)0)).Equals((decimal)0));
			Assert.IsFalse(((IEquatable<decimal>)((decimal)1)).Equals((decimal)0));
			Assert.IsFalse(((IEquatable<decimal>)((decimal)0)).Equals((decimal)0.5));
			Assert.IsTrue (((IEquatable<decimal>)((decimal)1)).Equals((decimal)1));
		}

		[Test]
		public void CompareToWorks() {
			Assert.IsTrue(((decimal)0).CompareTo((decimal)0) == 0);
			Assert.IsTrue(((decimal)1).CompareTo((decimal)0) > 0);
			Assert.IsTrue(((decimal)0).CompareTo((decimal)0.5) < 0);
			Assert.IsTrue(((decimal)1).CompareTo((decimal)1) == 0);
		}

		[Test]
		public void IComparableCompareToWorks() {
			Assert.IsTrue(((IComparable<decimal>)((decimal)0)).CompareTo((decimal)0) == 0);
			Assert.IsTrue(((IComparable<decimal>)((decimal)1)).CompareTo((decimal)0) > 0);
			Assert.IsTrue(((IComparable<decimal>)((decimal)0)).CompareTo((decimal)0.5) < 0);
			Assert.IsTrue(((IComparable<decimal>)((decimal)1)).CompareTo((decimal)1) == 0);
		}
	}
}
