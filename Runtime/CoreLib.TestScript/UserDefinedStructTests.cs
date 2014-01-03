using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class UserDefinedStructTests {
#pragma warning disable 649
		struct S1 {
			public readonly int I;
			public S1(int i) {
				I = i;
			}
		}

		struct S2<TT> {
			public readonly int I;
			public readonly double D;
			public readonly DateTime DT;
			public readonly object O;
			public readonly TT T;
		}

		struct S3 {
			public readonly int I1, I2;
			public static int StaticField;
			public S3(int i1, int i2) {
				I1 = i1;
				I2 = i2;
			}
		}

		struct S4 {
			public readonly int I1, I2;
			public S4(int i1, int i2) {
				I1 = i1;
				I2 = i2;
			}
		}

		struct S5 {
			public readonly int I;

			public S5(int i) {
				I = i;
			}

			public override int GetHashCode() {
				return I + 1;
			}

			public override bool Equals(object o) {
				return Math.Abs(((S5)o).I - I) <= 1;
			}
		}

		struct S6 {
			[ScriptName("i")]
			public readonly int I;

			[InlineCode("{{ i: 42 }}")]
			public S6(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) : this() {
			}
		}
#pragma warning restore 649

		private T Create<T>() where T : new() {
			return new T();
		}

		[Test]
		public void IsClassIsFalse() {
			Assert.IsFalse(typeof(S1).IsClass, "#1");
			Assert.IsFalse(typeof(S2<int>).IsClass, "#2");
		}

		[Test]
		public void UserDefinedStructCanBeUsed() {
			var s1 = new S1(42);
			Assert.AreEqual(s1.I, 42);
		}

		[Test]
		public void DefaultConstructorOfStructReturnsInstanceWithAllMembersInitialized() {
			var s2 = default(S2<int>);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitialized() {
			var s2 = default(S2<int>);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitializedIndirect() {
			var s2 = Create<S2<DateTime>>();
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, new DateTime(0), "T");
		}

		[Test]
		public void DefaultValueOfStructWithInlineCodeDefaultConstructorWorks() {
			var s1 = default(S6);
			var s2 = Create<S6>();
			Assert.AreEqual(s1.I, 42, "#1");
			Assert.AreEqual(s2.I, 42, "#2");
		}

		[Test]
		public void DefaultConstructorOfStructWithInlineCodeDefaultConstructorWorks() {
			var s1 = new S6();
			Assert.AreEqual(s1.I, 42);
		}

		[Test]
		public void DefaultGetHashCodeGeneratesHashCodeBasedOnAllInstanceFields() {
			S3.StaticField = 10;
			var s1 = new S3(235, 45);
			var s2 = new S3(235, 45);
			var s3 = new S3(235, 44);
			Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), "#1");
			Assert.AreNotEqual(s1.GetHashCode(), s3.GetHashCode(), "#2");
			int hc = s1.GetHashCode();
			S3.StaticField = 20;
			Assert.AreEqual(s1.GetHashCode(), hc, "#3");
		}

		[Test]
		public void DefaultEqualsUsesValueEqualityForAllMembers() {
			var s1 = new S3(235, 45);
			var s2 = new S3(235, 45);
			var s3 = new S3(235, 44);
			var s4 = new S4(235, 45);
			Assert.IsTrue (s1.Equals(s2), "#1");
			Assert.IsFalse(s1.Equals(s3), "#2");
			Assert.IsFalse(s1.Equals(s4), "#3");
		}

		[Test]
		public void CanOverrideGetHashCode() {
			var s1 = new S5(42);
			Assert.AreEqual(s1.GetHashCode(), 43);
		}

		[Test]
		public void CanOverrideEquals() {
			var s1 = new S5(42);
			var s2 = new S5(43);
			var s3 = new S5(44);
			Assert.IsTrue (s1.Equals(s2), "#1");
			Assert.IsFalse(s1.Equals(s3), "#2");
		}
	}
}
