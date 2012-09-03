using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Serialization;
using System.Testing;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class JsonTests {
		[Serializable]
		class TestClass1 {
			[PreserveName]
			public int i;
		}

		[Serializable]
		class TestClass2 {
			[PreserveName]
			public int i;
			[PreserveName]
			public string s;
		}

		[Test]
		public void NonGenericParseWorks() {
			var o = (TestClass2)Json.Parse("{ \"i\": 3, \"s\": \"test\" }");
			Assert.AreEqual(o.i, 3);
			Assert.AreEqual(o.s, "test");
		}

		[Test]
		public void GenericParseWorks() {
			var o = Json.Parse<TestClass2>("{ \"i\": 3, \"s\": \"test\" }");
			Assert.AreEqual(o.i, 3);
			Assert.AreEqual(o.s, "test");
		}

		[Test]
		public void NonGenericParseWithCallbackWorks() {
			var o = (TestClass2)Json.Parse("{ \"i\": 3, \"s\": \"test\" }", (s, x) => { ((TestClass2)x).i = 100; return x; });
			Assert.AreEqual(o.i, 100);
			Assert.AreEqual(o.s, "test");
		}

		[Test]
		public void GenericParseWithCallbackWorks() {
			var o = Json.Parse<TestClass2>("{ \"i\": 3, \"s\": \"test\" }", (s, x) => { ((TestClass2)x).i = 100; return x; });
			Assert.AreEqual(o.i, 100);
			Assert.AreEqual(o.s, "test");
		}

		[Test]
		public void StringifyWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass1 { i = 3 }), "{\"i\":3}");
		}

		[Test]
		public void StringifyWithSerializableMembersArrayWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, new[] { "i" }), "{\"i\":3}");
		}

		[Test]
		public void StringifyWithSerializableMembersArrayAndIntentCountWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, new[] { "i" }, 4), "{\n    \"i\": 3\n}");
		}

		[Test]
		public void StringifyWithSerializableMembersArrayAndIntentTextWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, new[] { "i" }, "    "), "{\n    \"i\": 3\n}");
		}

		[Test]
		public void StringifyWithCallbackWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, (key, value) => key == "s" ? Script.Undefined : value), "{\"i\":3}");
		}

		[Test]
		public void StringifyWithCallbackAndIndentCountWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, (key, value) => key == "s" ? Script.Undefined : value, 4), "{\n    \"i\": 3\n}");
		}

		[Test]
		public void StringifyWithCallbackAndIndentTextWorks() {
			Assert.AreEqual(Json.Stringify(new TestClass2 { i = 3, s = "test" }, (key, value) => key == "s" ? Script.Undefined : value, "    "), "{\n    \"i\": 3\n}");
		}
	}
}
