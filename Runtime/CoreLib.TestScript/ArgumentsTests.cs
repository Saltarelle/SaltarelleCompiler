using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ArgumentsTests {
		[ExpandParams]
		private void LengthHelper0(params object[] args) {
			Assert.AreEqual(Arguments.Length, 0);
		}

		[ExpandParams]
		private void LengthHelper1(params object[] args) {
			Assert.AreEqual(Arguments.Length, 1);
		}

		[ExpandParams]
		private void LengthHelper2(params object[] args) {
			Assert.AreEqual(Arguments.Length, 2);
		}

		[ExpandParams]
		private object GetArgumentHelper(int index, params object[] args) {
			return Arguments.GetArgument(index);
		}

		[ExpandParams]
		private object ToArrayHelper(params object[] args) {
			return Arguments.ToArray();
		}

		[Test]
		public void LengthWorks() {
			LengthHelper0();
			LengthHelper1(4);
			LengthHelper2(6, "x");
		}

		[Test]
		public void GetArgumentWorks() {
			Assert.AreEqual(GetArgumentHelper(0, "x", "y"), 0);
			Assert.AreEqual(GetArgumentHelper(1, "x", "y"), "x");
			Assert.AreEqual(GetArgumentHelper(2, "x", "y"), "y");
		}

		[Test]
		public void ToArrayWorks() {
			Assert.AreEqual(ToArrayHelper(), new object[0]);
			Assert.AreEqual(ToArrayHelper("x"), new object[] { "x" });
			Assert.AreEqual(ToArrayHelper("x", 1), new object[] { "x", 1 });
		}
	}
}
