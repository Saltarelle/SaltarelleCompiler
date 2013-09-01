using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript.Reflection {
	[TestFixture]
	public class AssemblyTests {
		class C {}

		private Assembly ImportedModuleTestCase {
			[InlineCode(@"
(function() {{
	var x = {{
		Foo: {{
			Bar: {{
				Inner: {{
					OtherFunction: function() {{ }}
				}},
				Something: function() {{ }}
			}},
			baz: function() {{
			}},
			Bar2: 0
		}
	};
	x.Foo.baz.Test = function() {};
	return x;
}})()")]
			get { return null; }
		}

		[Test]
		public void GetExecutingAssemblyWorks() {
			Assert.AreEqual(Assembly.GetExecutingAssembly().FullName, "CoreLib.TestScript");
		}

		[Test]
		public void GetAssemblyForTypeWorks() {
			Assert.AreEqual(Assembly.GetAssembly(typeof(int)).FullName, "mscorlib");
			Assert.AreEqual(Assembly.GetAssembly(typeof(AssemblyTests)).FullName, "CoreLib.TestScript");
		}

		[Test]
		public void FullNameWorks() {
			Assert.AreEqual(typeof(int).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(AssemblyTests).Assembly.FullName, "CoreLib.TestScript");
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual(typeof(int).Assembly.ToString(), "mscorlib");
			Assert.AreEqual(typeof(AssemblyTests).Assembly.ToString(), "CoreLib.TestScript");
		}

		[Test]
		public void GetTypesWorks() {
			var types = new List<string>();
			foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
				types.Add(t.FullName);
			Assert.IsTrue(types.Contains(typeof(AssemblyTests).FullName));
			Assert.IsTrue(types.Contains(typeof(ReflectionTests.C1).FullName));
		}

		[Test]
		public void GetTypesWithImportedModuleWorks() {
			var asm = ImportedModuleTestCase;
			var types = asm.GetTypes();
			Assert.AreEqual(types.Length, 3, "Length");
			Assert.IsTrue(types.Contains((Type)((dynamic)asm).Foo.Bar.Inner.OtherFunction), "#1");
			Assert.IsTrue(types.Contains((Type)((dynamic)asm).Foo.Bar.Something), "#2");
			Assert.IsTrue(types.Contains((Type)((dynamic)asm).Foo.baz.Test), "#3");
		}

		[Test]
		public void GetTypeWorks() {
			Assert.IsTrue(Assembly.GetExecutingAssembly().GetType(typeof(AssemblyTests).FullName) == typeof(AssemblyTests));
			Assert.IsTrue(Assembly.GetExecutingAssembly().GetType(typeof(Dictionary<,>).FullName) == null);
			Assert.IsTrue(typeof(int).Assembly.GetType(typeof(Dictionary<,>).FullName) == typeof(Dictionary<,>));
		}

		[Test]
		public void GetTypeWithImportedModuleWorks() {
			var asm = ImportedModuleTestCase;
			Assert.IsTrue(asm.GetType("Foo.Bar.Inner.OtherFunction") == (Type)((dynamic)asm).Foo.Bar.Inner.OtherFunction, "#1");
			Assert.IsTrue(asm.GetType("Foo.Bar.Something") == (Type)((dynamic)asm).Foo.Bar.Something, "#2");
			Assert.IsTrue(asm.GetType("Foo.baz.Test") == (Type)((dynamic)asm).Foo.baz.Test, "#3");
			Assert.IsTrue(asm.GetType("Foo.Bar") == null, "#4");
		}

		[Test]
		public void AssemblyOfBuiltInTypes() {
			Assert.AreEqual(typeof(DateTime).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(double).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(bool).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(string).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(Delegate).Assembly.FullName, "mscorlib");
			Assert.AreEqual(typeof(int[]).Assembly.FullName, "mscorlib");
		}

		[Test]
		public void CreateInstanceWorks() {
			Assert.IsTrue(typeof(C).Assembly.CreateInstance(typeof(C).FullName) is C, "#1");
			Assert.AreEqual(typeof(int).Assembly.CreateInstance(typeof(int).FullName), 0, "#2");
			Assert.IsTrue(typeof(C).Assembly.CreateInstance("NonExistentType") == null, "#3");
		}
	}
}
