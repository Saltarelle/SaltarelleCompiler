using System;
using System.Collections.Generic;
using QUnit;

#pragma warning disable 183, 184

namespace CoreLib.TestScript.Collections.Generic {
	[TestFixture]
	public class KeyValuePairTests {
		[Test]
		public void TheConstructorWithParametersCanBeUsed() {
			var v = new KeyValuePair<string, int>("Hello", 42);
			Assert.IsTrue(v is KeyValuePair<string, int>, "is KeyValuePair");
			Assert.AreEqual(v.Key, "Hello");
			Assert.AreEqual(v.Value, 42);
		}

		[Test]
		public void TypeTestWorks() {
			Assert.IsTrue(new KeyValuePair<int, string>(42, "Hello") is KeyValuePair<int, string>, "#1");
			Assert.IsFalse(5 is KeyValuePair<int, string>, "#2");
		}

		private bool RunCheck<T>(object o) {
			return o is T;
		}

		[Test]
		public void TypeTestWorksGeneric() {
			Assert.IsTrue(RunCheck<KeyValuePair<int, string>>(new KeyValuePair<int, string>()), "#1");
			Assert.IsFalse(RunCheck<KeyValuePair<int, string>>(5), "#2");
		}

		[Test]
		public void TheDefaultConstructorCanBeUsed() {
			var v = new KeyValuePair<DateTime, int>();
			Assert.IsTrue(v is KeyValuePair<DateTime, int>, "is KeyValuePair");
			Assert.IsTrue(v.Key is DateTime);
			Assert.AreEqual(v.Value, 0);
		}

		[Test]
		public void CreatingADefaultKeyValuePairCreatesAnInstanceThatIsNotNull() {
			var v = default(KeyValuePair<string, string>);
			Assert.IsTrue(v is KeyValuePair<string, string>, "is KeyValuePair");
			Assert.IsTrue(Script.In(v, "key"));
			Assert.IsTrue(Script.In(v, "value"));
		}

		[Test]
		public void ActivatorCreateInstanceWorks() {
			var v = Activator.CreateInstance<KeyValuePair<string, string>>();
			Assert.IsTrue(v is KeyValuePair<string, string>, "is KeyValuePair");
			Assert.IsTrue(Script.In(v, "key"));
			Assert.IsTrue(Script.In(v, "value"));
		}
	}
}
