using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ScriptTests {
		public class TestType {
			public TestType() {
				i = 42;
				P = 42;
			}

			[PreserveCase]
			public int i;

			[PreserveCase]
			public int P { get; set; }

			[PreserveCase]
			public int P2 { get { return 0; } }

			[PreserveCase]
			public int P3 { set {} }

			[PreserveCase]
			public event EventHandler Evt;
		
			public void Raise() { if (Evt != null) Evt(this, null); }

			[PreserveCase]
			public void InstanceMethod() {}

			[PreserveCase]
			public static void StaticMethod() {}

			[PreserveCase]
			public int F1() { return 42; }
			[PreserveCase]
			public int F2(int i) { return i + 10; }
			[PreserveCase]
			public int F3(int i, int j) { return i + j; }
		}

		[Test]
		public void BooleanWorks() {
			Assert.AreStrictEqual(Script.Boolean(0), false);
			Assert.AreStrictEqual(Script.Boolean(""), false);
			Assert.AreStrictEqual(Script.Boolean("1"), true);
		}

		[Test]
		public void EvalWorks() {
			Assert.AreEqual(Script.Eval("2 + 3"), 5);
		}

		private static object Undefined { [InlineCode("undefined")] get { return null; } }

		[Test]
		public void IsNullWorks() {
			Assert.IsTrue(Script.IsNull(null));
			Assert.IsFalse(Script.IsNull(Undefined));
			Assert.IsFalse(Script.IsNull(3));
		}

		[Test]
		public void IsNullOrUndefinedWorks() {
			Assert.IsTrue(Script.IsNullOrUndefined(null));
			Assert.IsTrue(Script.IsNullOrUndefined(Undefined));
			Assert.IsFalse(Script.IsNullOrUndefined(3));
		}

		[Test]
		public void IsValueWorks() {
			Assert.IsFalse(Script.IsValue(null));
			Assert.IsFalse(Script.IsValue(Undefined));
			Assert.IsTrue(Script.IsValue(3));
		}

		[Test]
		public void UndefinedWorks() {
			Assert.IsTrue(Script.IsUndefined(Script.Undefined));
		}

		[Test]
		public void TypeOfWorks() {
			Assert.AreEqual(Script.TypeOf(Script.Undefined), "undefined", "#1");
			Assert.AreEqual(Script.TypeOf(null), "object", "#2");
			Assert.AreEqual(Script.TypeOf(true), "boolean", "#3");
			Assert.AreEqual(Script.TypeOf(0), "number", "#4");
			Assert.AreEqual(Script.TypeOf(double.MaxValue), "number", "#5");
			Assert.AreEqual(Script.TypeOf("X"), "string", "#6");
			Assert.AreEqual(Script.TypeOf(new Function("","")), "function", "#7");
			Assert.AreEqual(Script.TypeOf(new {}), "object", "#8");
		}

		[Test]
		public void DeleteWorks() {
			var c = new TestType();
			Assert.AreEqual(c.i, 42);
			Assert.IsTrue(Script.Delete(c, "i"));
			Assert.AreEqual(Script.TypeOf(c.i), "undefined");
		}

		[Test]
		public void InWorks() {
			var c = new TestType();
			Assert.IsTrue(Script.In(c, "i"));
			Assert.IsFalse(Script.In(c, "x"));
			Assert.IsFalse(Script.In(c, "P"));
		}

		[Test]
		public void InvokeMethodWorks() {
			var c = new TestType();
			Assert.AreEqual(Script.InvokeMethod(c, "F1"), 42);
			Assert.AreEqual(Script.InvokeMethod(c, "F2", 17), 27);
			Assert.AreEqual(Script.InvokeMethod(c, "F3", 19, 2), 21);
		}

		[Test]
		public void ParseIntWithoutRadixWorks() {
			Assert.AreEqual(Script.ParseInt("234"), 234);
		}

		[Test]
		public void ParseIntWithRadixWorks() {
			Assert.AreEqual(Script.ParseInt("234", 16),0x234);
		}
	}
}
