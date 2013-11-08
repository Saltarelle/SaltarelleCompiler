using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using QUnit;

[assembly: CoreLib.TestScript.Reflection.AssemblyTests.A1(41, P = 10)]
[assembly: CoreLib.TestScript.Reflection.AssemblyTests.A2(64, P = 23)]
[assembly: CoreLib.TestScript.Reflection.AssemblyTests.A3(15, P = 45)]

namespace CoreLib.TestScript.Reflection {
	[TestFixture]
	public class AssemblyTests {
		[NonScriptable]
		public class A1Attribute : Attribute {
			public int X { get; private set; }
			public int P { get; set; }
			public A1Attribute() {}
			public A1Attribute(int x) { X = x; }
		}

		public class A2Attribute : Attribute {
			public int X { get; private set; }
			public int P { get; set; }
			public A2Attribute() {}
			public A2Attribute(int x) { X = x; }
		}

		public class A3Attribute : Attribute {
			public int X { get; private set; }
			public int P { get; set; }
			public A3Attribute() {}
			public A3Attribute(int x) { X = x; }
		}

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

		[Test]
		public void GetCustomAttributesWorks() {
			var asm = Assembly.GetExecutingAssembly();
			foreach (var a in new[] { asm.GetCustomAttributes(), asm.GetCustomAttributes(true), asm.GetCustomAttributes(false) }) {
				Assert.IsFalse(a.Some(x => x.GetType().Name == "A1Attribute"));
				var a2 = a.Filter(x => x is A2Attribute);
				Assert.AreEqual(a2.Length, 1);
				Assert.IsTrue(((A2Attribute)a2[0]).X == 64);
				Assert.IsTrue(((A2Attribute)a2[0]).P == 23);

				var a3 = a.Filter(x => x is A3Attribute);
				Assert.AreEqual(a3.Length, 1);
				Assert.IsTrue(((A3Attribute)a3[0]).X == 15);
				Assert.IsTrue(((A3Attribute)a3[0]).P == 45);
			}

			foreach (var a in new[] { asm.GetCustomAttributes(typeof(A2Attribute)), asm.GetCustomAttributes(typeof(A2Attribute), true), asm.GetCustomAttributes(typeof(A2Attribute), false) }) {
				Assert.AreEqual(a.Length, 1);
				Assert.IsTrue(((A2Attribute)a[0]).X == 64);
				Assert.IsTrue(((A2Attribute)a[0]).P == 23);
			}
		}

		[Test]
		public void LoadCanReturnReferenceToLoadedAssembly() {
			Assert.IsTrue(Assembly.Load("CoreLib.TestScript") == Assembly.GetExecutingAssembly(), "TestScripts");
			Assert.IsTrue(Assembly.Load("mscorlib") == typeof(int).Assembly, "mscorlib");
		}

		[Test]
		public void GetManifestResourceNamesWorks() {
			var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			names.Sort((a, b) => a.CompareTo(b));
			Assert.AreEqual(names, new[] { "CoreLib.TestScript.Reflection.Resource1.bin", "CoreLib.TestScript.Reflection.Resource2.bin" });
		}

		[Test]
		public void GetManifestResourceDataAsBase64WithoutTypeWorks() {
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64("CoreLib.TestScript.Reflection.Resource1.bin"), "AAECAwQFBgc=", "#1");
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64("CoreLib.TestScript.Reflection.Resource2.bin"), "EBESExQV", "#2");
			Assert.IsTrue(Script.IsNull(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64("NonExistent")), "#3");
		}

		[Test]
		public void GetManifestResourceDataAsBase64WithTypeWorks() {
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64(typeof(AssemblyTests), "Resource1.bin"), "AAECAwQFBgc=", "#1");
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64(typeof(AssemblyTests), "Resource2.bin"), "EBESExQV", "#2");
			Assert.IsTrue(Script.IsNull(Assembly.GetExecutingAssembly().GetManifestResourceDataAsBase64(typeof(AssemblyTests), "NonExistent")), "#3");
		}

		[Test]
		public void GetManifestResourceDataWithoutTypeWorks() {
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceData("CoreLib.TestScript.Reflection.Resource1.bin"), new[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }, "#1");
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceData("CoreLib.TestScript.Reflection.Resource2.bin"), new[] { 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 }, "#2");
			Assert.IsTrue(Script.IsNull(Assembly.GetExecutingAssembly().GetManifestResourceData("NonExistent")), "#3");
		}

		[Test]
		public void GetManifestResourceDataWithTypeWorks() {
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceData(typeof(AssemblyTests), "Resource1.bin"), new[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }, "#1");
			Assert.AreEqual(Assembly.GetExecutingAssembly().GetManifestResourceData(typeof(AssemblyTests), "Resource2.bin"), new[] { 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 }, "#2");
			Assert.IsTrue(Script.IsNull(Assembly.GetExecutingAssembly().GetManifestResourceData(typeof(AssemblyTests), "NonExistent")), "#3");
		}
	}
}
