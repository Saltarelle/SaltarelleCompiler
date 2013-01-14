using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLibTests {
	[TestFixture]
	public class TypeUtilityMethodsTests {
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
		public void AddHandlerWorks() {
			bool invoked = false;
			var c = new TestType();
			Type.AddHandler(c, "Evt", (EventHandler)((s, e) => invoked = true));
			c.Raise();
			Assert.AreEqual(invoked, true);
		}

		[Test]
		public void DeleteFieldWorks() {
			var c = new TestType();
			Assert.AreEqual(c.i, 42);
			Type.DeleteField(c, "i");
			Assert.AreEqual(Type.GetScriptType(c.i), "undefined");
		}

		[Test]
		public void GetFieldWorks() {
			var c = new TestType();
			Assert.AreEqual(Type.GetField(c, "i"), 42);
		}

		[Test]
		public void GetPropertyWorks() {
			var c = new TestType();
			Assert.AreEqual(Type.GetProperty(c, "P"), 42);
		}

		[Test]
		public void GetScriptTypeWorks() {
			Assert.AreEqual(Type.GetScriptType(null), "object");
			Assert.AreEqual(Type.GetScriptType(new object()), "object");
			Assert.AreEqual(Type.GetScriptType("X"), "string");
			Assert.AreEqual(Type.GetScriptType(1), "number");
			Assert.AreEqual(Type.GetScriptType(true), "boolean");
		}

		[Test]
		public void StaticGetTypeMethodWorks() {
			Assert.AreEqual(Type.GetType("CoreLibTests.TypeUtilityMethodsTests"), typeof(TypeUtilityMethodsTests));
		}

		[Test]
		public void HasFieldWorks() {
			var c = new TestType();
			Assert.IsTrue(Type.HasField(c, "i"));
			Assert.IsFalse(Type.HasField(c, "x"));
			Assert.IsFalse(Type.HasField(c, "P"));
		}

		[Test]
		public void HasMethodWorks() {
			var c = new TestType();
			Assert.IsTrue (Type.HasMethod(c, "InstanceMethod"));
			Assert.IsFalse(Type.HasMethod(c, "StaticMethod"));
			Assert.IsFalse(Type.HasMethod(typeof(TestType), "InstanceMethod"));
			Assert.IsTrue (Type.HasMethod(typeof(TestType), "StaticMethod"));
		}

		[Test]
		public void HasPropertyWorks() {
			var c = new TestType();
			Assert.IsTrue (Type.HasProperty(c, "P"));
			Assert.IsTrue (Type.HasProperty(c, "P2"));
			Assert.IsTrue (Type.HasProperty(c, "P3"));
			Assert.IsFalse(Type.HasProperty(c, "X"));
			Assert.IsFalse(Type.HasProperty(c, "i"));
		}

		[Test]
		public void InvokeMethodWorks() {
			var c = new TestType();
			#pragma warning disable 612,618
			Assert.AreEqual(Type.InvokeMethod(c, "F1"), 42);
			Assert.AreEqual(Type.InvokeMethod(c, "F2", 17), 27);
			Assert.AreEqual(Type.InvokeMethod(c, "F3", 19, 2), 21);
			#pragma warning restore 612,618
		}

		[Test]
		public void RemoveHandlerWorks() {
			bool invoked = false;
			var handler = (EventHandler)((s, e) => invoked = true);
			var c = new TestType();
			c.Evt += handler;
			c.Raise();
			Assert.IsTrue(invoked);
			invoked = false;
			Type.RemoveHandler(c, "Evt", handler);
			c.Raise();
			Assert.IsFalse(invoked);
		}

		[Test]
		public void SetFieldWorks() {
			var c = new TestType();
			Type.SetField(c, "i", 546);
			Assert.AreEqual(c.i, 546);
		}

		[Test]
		public void SetPropertyWorks() {
			var c = new TestType();
			Type.SetProperty(c, "P", 543);
			Assert.AreEqual(c.P, 543);
		}
	}
}
