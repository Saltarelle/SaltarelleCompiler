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

		struct S2 {
			public readonly int I;
			public readonly double D;
			public readonly DateTime DT;
			public readonly object O;
			public readonly int T;
		}

		struct S2G<TT> {
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

		struct S6G<TT> {
			[ScriptName("i")]
			public readonly TT I;

			[InlineCode("{{ i: 42 }}")]
			public S6G(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) : this() {
			}
		}

		struct S7 {
			public readonly int I;

			public S7(int i) {
				I = i;
			}

			public static S7 operator+(S7 a, S7 b) {
				return new S7(a.I + b.I);
			}

			public static S7 operator-(S7 s) {
				return new S7(-s.I);
			}

			public static explicit operator int(S7 s) {
				return s.I;
			}
		}

		[Mutable]
		struct MS1 {
			public int i;
			public string P1 { get; set; }
			[IntrinsicProperty]
			public int P2 { get; set; }
			public event Action E;
			public MS2 N;

			public void RaiseE() {
				E();
			}
		}

		[Mutable]
		struct MS2 {
			public int i;
		}

		[Mutable]
		struct MS3<T> {
			public T t;
		}

		[Mutable]
		struct MS4 {
			public int i;

			[ScriptName("x")]
			public MS4(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) : this() {
			}
		}

#pragma warning restore 649

		private T Create<T>() where T : new() {
			return new T();
		}

		[Test]
		public void IsClassIsFalse() {
			Assert.IsFalse(typeof(S1).IsClass, "#1");
			Assert.IsFalse(typeof(S2G<int>).IsClass, "#2");
		}

		[Test]
		public void UserDefinedStructCanBeUsed() {
			var s1 = new S1(42);
			Assert.AreEqual(s1.I, 42);
		}

		[Test]
		public void DefaultConstructorOfStructReturnsInstanceWithAllMembersInitialized() {
			var s2 = default(S2);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultConstructorOfStructReturnsInstanceWithAllMembersInitializedGeneric() {
			var s2 = default(S2G<int>);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitialized() {
			var s2 = default(S2);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitializedGeneric() {
			var s2 = default(S2G<int>);
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitializedIndirect() {
			var s2 = Create<S2>();
			Assert.AreEqual(s2.I, 0, "I");
			Assert.AreEqual(s2.D, 0, "D");
			Assert.AreEqual(s2.DT, new DateTime(0), "DT");
			Assert.IsNull(s2.O, "O");
			Assert.AreEqual(s2.T, 0, "T");
		}

		[Test]
		public void DefaultValueOfStructIsInstanceWithAllMembersInitializedIndirectGeneric() {
			var s2 = Create<S2G<DateTime>>();
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
		public void DefaultValueOfStructWithInlineCodeDefaultConstructorWorksGeneric() {
			var s1 = default(S6G<int>);
			var s2 = Create<S6G<int>>();
			Assert.AreEqual(s1.I, 42, "#1");
			Assert.AreEqual(s2.I, 42, "#2");
		}

		[Test]
		public void DefaultConstructorOfStructWithInlineCodeDefaultConstructorWorks() {
			var s1 = new S6();
			Assert.AreEqual(s1.I, 42);
		}

		[Test]
		public void DefaultConstructorOfStructWithInlineCodeDefaultConstructorWorksGeneric() {
			var s1 = new S6G<int>();
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

		[Test]
		public void CanLiftUserDefinedBinaryOperator() {
			S7? a = new S7(42), b = new S7(32), c = null;
			Assert.AreEqual((a + b).Value.I, 74, "#1");
			Assert.IsNull((a + c), "#2");
		}

		[Test]
		public void CanLiftUserDefinedUnaryOperator() {
			S7? a = new S7(42), b = null;
			Assert.AreEqual(-a.Value.I, -42, "#1");
			Assert.IsNull(-b, "#2");
		}

		[Test]
		public void CanLiftUserDefinedConversionOperator() {
			S7? a = new S7(42), b = null;
			Assert.AreEqual((double?)a, 42, "#1");
			Assert.IsNull((double?)b, "#2");
		}

		[Test]
		public void ClonedValueTypeIsCorrectType() {
			var s1 = new MS1 { i = 42 };
			var s2 = s1;
			Assert.IsTrue((object)s2 is MS1);
		}

		[Test]
		public void FieldsAreClonedWhenValueTypeIsCopied() {
			var s1 = new MS1 { i = 42 };
			var s2 = s1;
			Assert.AreEqual(s2.i, 42);
			s2.i = 43;
			Assert.AreEqual(s1.i, 42);
			Assert.AreEqual(s2.i, 43);
		}

		[Test]
		public void AutoPropertyBackingFieldsAreClonedWhenValueTypeIsCopied() {
			var s1 = new MS1 { P1 = "hello" };
			var s2 = s1;
			Assert.AreEqual(s2.P1, "hello");
			s2.P1 = "world";
			Assert.AreEqual(s1.P1, "hello");
			Assert.AreEqual(s2.P1, "world");
		}

		[Test]
		public void PropertiesWithFieldImplementationAreClonedWhenValueTypeIsCopied() {
			var s1 = new MS1 { P2 = 42 };
			var s2 = s1;
			Assert.AreEqual(s2.P2, 42);
			s2.P2 = 43;
			Assert.AreEqual(s1.P2, 42);
			Assert.AreEqual(s2.P2, 43);
		}

		[Test]
		public void AutoEventBackingFieldsAreClonedWhenValueTypeIsCopied() {
			int count = 0;
			Action a = () => count++;
			var s1 = new MS1();
			s1.E += a;
			var s2 = s1;
			s2.E += a;

			s1.RaiseE();
			Assert.AreEqual(count, 1);

			s2.RaiseE();
			Assert.AreEqual(count, 3);
		}

		[Test]
		public void NestedStructsAreClonedWhenValueTypeIsCopied() {
			var s1 = new MS1 { N = new MS2 { i = 42 } };
			var s2 = s1;
			Assert.AreEqual(s2.N.i, 42);
			s2.N.i = 43;

			Assert.AreEqual(s1.N.i, 42);
			Assert.AreEqual(s2.N.i, 43);
		}

		[Test]
		public void GenericMutableValueTypeWorks() {
			var s1 = new MS3<int> { t = 42 };
			var s2 = s1;
			Assert.AreEqual(s2.t, 42);
			s2.t = 43;
			Assert.IsTrue((object)s2 is MS3<int>);
			Assert.AreEqual(s1.t, 42);
			Assert.AreEqual(s2.t, 43);
		}

		[Test]
		public void CloningValueTypeWithNamedDefaultConstructorWorks() {
			var s1 = new MS1 { i = 42 };
			var s2 = s1;
			s1.i = 10;
			Assert.AreEqual(s2.i, 42);
			Assert.IsTrue((object)s2 is MS1);
		}

		[Test]
		public void CloningNullableValueTypesWorks() {
			MS1? s1 = null;
			MS1? s2 = new MS1 { i = 42 };
			var s3 = s1;
			var s4 = s2;

			Assert.IsTrue(Script.IsNull(s3), "s3 should be null");
			Assert.AreEqual(s4.Value.i, 42, "s4.i should be 42");
			Assert.IsFalse(ReferenceEquals(s2, s4), "s2 and s4 should not be the same object");
		}
	}
}
