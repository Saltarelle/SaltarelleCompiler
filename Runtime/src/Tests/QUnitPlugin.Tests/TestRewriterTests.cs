using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using QUnit.Plugin;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.RuntimeLibrary;
using Saltarelle.Compiler.Tests;

namespace QUnitPlugin.Tests {
	[TestFixture]
    public class TestRewriterTests {
		public static readonly string MscorlibPath = Path.GetFullPath(@"..\..\..\..\bin\mscorlib.dll");
		private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(MscorlibPath));
		internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

		public static readonly string QUnitPath = Path.GetFullPath(@"..\..\..\..\bin\Script.QUnit.dll");
		private static readonly Lazy<IAssemblyReference> _qunitLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(QUnitPath));
		internal static IAssemblyReference QUnit { get { return _qunitLazy.Value; } }

		private Tuple<JsClass, MockErrorReporter> Compile(string source, bool expectErrors = false) {
			var sourceFile = new MockSourceFile("file.cs", source);
            var er = new MockErrorReporter(!expectErrors);
			var md = new ScriptSharpMetadataImporter(er);
			var n = new DefaultNamer();
            var references = new[] { Mscorlib, QUnit };
			var compilation = PreparedCompilation.CreateCompilation(new[] { sourceFile }, references, null);
			var rtl = new ScriptSharpRuntimeLibrary(md, er, n, compilation.Compilation);
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions(), false, compilation.Compilation.MainAssembly);
            var compiler = new Compiler(md, n, rtl, er);

			var result = compiler.Compile(compilation).ToList();
			Assert.That(result, Has.Count.EqualTo(1), "Should compile exactly one type");
            Assert.That(er.AllMessages, Is.Empty, "Compile should not generate errors");

			result = new TestRewriter(er, rtl).Rewrite(result).ToList();
			Assert.That(result, Has.Count.EqualTo(1), "Should have one type after rewrite");
			Assert.That(result[0], Is.InstanceOf<JsClass>(), "Compiled type should be a class after rewrite");

			if (expectErrors) {
				Assert.That(er.AllMessages, Is.Not.Empty);
			}
			else {
				Assert.That(er.AllMessages, Is.Empty);
			}

			return Tuple.Create((JsClass)result[0], er);
		}

		private void AssertEqual(string expected, string actual) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected " + expected + ", was " + actual);
		}

		private void AssertCorrect(string source, string expectedRunTestsMethod) {
			var type = Compile(source).Item1;
			AssertEqual(expectedRunTestsMethod, OutputFormatter.Format(type.InstanceMethods.Single(m => m.Name == "runTests").Definition, allowIntermediates: true));
		}

		[Test]
		public void TestFixtureClassHasARunMethodThatRunsAllTestMethodsInTheClass() {
			var type = Compile(
@"using QUnit;

[TestFixture]
public class MyClass {
	static int _s;
	int _i;

	MyClass() {
		_i = 3;
	}

	MyClass(int i) {
		_i = i;
	}

	[Test]
	public void TestMethod() {
		int x1 = 0;
	}

	[Test(IsAsync = true)]
	public void AsyncTestMethod() {
		int x2 = 0;
	}

	[Test(ExpectedAssertionCount = 3)]
	public void TestMethodWithAssertionCount() {
		int x3 = 0;
	}

	public void NormalMethod(object o) {
		int n = 0;
	}

	public static void StaticMethod(int n, string x) {
		int s = 0;
	}
}");

		AssertEqual(
@"function() {
	this.$_i = 0;
	this.$_i = 3;
}", OutputFormatter.Format(type.Item1.UnnamedConstructor, allowIntermediates: true));

		Assert.That(type.Item1.NamedConstructors, Has.Count.EqualTo(1));
		AssertEqual(
@"function(i) {
	this.$_i = 0;
	this.$_i = i;
}", OutputFormatter.Format(type.Item1.NamedConstructors[0].Definition, allowIntermediates: true));

		Assert.That(type.Item1.StaticMethods.Select(m => m.Name).ToList(), Is.EquivalentTo(new[] { "staticMethod" }));
		AssertEqual(
@"function(n, x) {
	var s = 0;
}", OutputFormatter.Format(type.Item1.StaticMethods[0].Definition, allowIntermediates: true));

		AssertEqual("{MyClass}.$_s = 0;\n", string.Join("", type.Item1.StaticInitStatements.Select(s => OutputFormatter.Format(s, allowIntermediates: true))));

		Assert.That(type.Item1.InstanceMethods.Select(m => m.Name).ToList(), Is.EquivalentTo(new[] { "normalMethod", "runTests" }));
		AssertEqual(@"function(o) {
	var n = 0;
}", OutputFormatter.Format(type.Item1.InstanceMethods.Single(m => m.Name == "normalMethod").Definition, allowIntermediates: true));

		AssertEqual(
@"function() {
	test('TestMethod', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
	asyncTest('AsyncTestMethod', {Delegate}.mkdel(this, function() {
		var x2 = 0;
	}));
	test('TestMethodWithAssertionCount', 3, {Delegate}.mkdel(this, function() {
		var x3 = 0;
	}));
}", OutputFormatter.Format(type.Item1.InstanceMethods.Single(m => m.Name == "runTests").Definition, allowIntermediates: true));
		}

		[Test]
		public void TestMethodsAreGroupedByCategoryWithTestsWithoutCategoryFirst() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test]
	public void Test1() {
		int x1 = 0;
	}

	[Test(Category = ""Category1"")]
	public void Test2() {
		int x2 = 0;
	}

	[Test(Category = ""Category2"")]
	public void Test3() {
		int x3 = 0;
	}

	[Test]
	public void Test4() {
		int x4 = 0;
	}

	[Test(Category = ""Category1"")]
	public void Test5() {
		int x5 = 0;
	}

	[Test(Category = ""Category2"")]
	public void Test6() {
		int x6 = 0;
	}
}",
@"function() {
	test('Test1', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
	test('Test4', {Delegate}.mkdel(this, function() {
		var x4 = 0;
	}));
	module('Category1');
	test('Test2', {Delegate}.mkdel(this, function() {
		var x2 = 0;
	}));
	test('Test5', {Delegate}.mkdel(this, function() {
		var x5 = 0;
	}));
	module('Category2');
	test('Test3', {Delegate}.mkdel(this, function() {
		var x3 = 0;
	}));
	test('Test6', {Delegate}.mkdel(this, function() {
		var x6 = 0;
	}));
}");
		}

		[Test]
		public void TestAttributeWithAllDefaultValuesWorks() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test]
	public void Test1() {
		int x1 = 0;
	}
}",
@"function() {
	test('Test1', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
}");
		}

		[Test]
		public void TestAttributeCanSpecifyDescription() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test(""Some description"")]
	public void Test1() {
		int x1 = 0;
	}
}",
@"function() {
	test('Some description', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
}");
		}

		[Test]
		public void TestAttributeCanSpecifyCategory() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test(Category = ""Some category"")]
	public void Test1() {
		int x1 = 0;
	}
}",
@"function() {
	module('Some category');
	test('Test1', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
}");
		}

		[Test]
		public void TestAttributeCanSpecifyExpectedAssertionCount() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test(ExpectedAssertionCount = 3)]
	public void Test1() {
		int x1 = 0;
	}
}",
@"function() {
	test('Test1', 3, {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
}");
		}

		[Test]
		public void TestAttributeCanSpecifyIsAsync() {
			AssertCorrect(@"
using QUnit;

[TestFixture]
public class MyClass {
	[Test(IsAsync = true)]
	public void Test1() {
		int x1 = 0;
	}
}",
@"function() {
	asyncTest('Test1', {Delegate}.mkdel(this, function() {
		var x1 = 0;
	}));
}");
		}

		[Test]
		public void TestFixtureClassCannotDeclareMethodWithScriptNameRunTests() {
			var res = Compile(
@"using QUnit;

[TestFixture]
public class C1 {
	public void RunTests() {
	}
}", expectErrors: true);
			Assert.That(res.Item2.AllMessages, Has.Count.EqualTo(1));
			Assert.That(res.Item2.AllMessages.Any(m => m.Code == 7019 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("TestFixtureAttribute") && m.FormattedMessage.Contains("runTests")));
		}

		[Test]
		public void MethodWithTestAttributeMustBeAPublicNonGenericParameterInstanceMethodReturningVoid() {
			var defs = new[] { "private void M()", "public int M()", "public void M<T>()", "public void M(int x)" };

			foreach (var def in defs) {
				var res = Compile("using QUnit; [TestFixture] public class C1 { [Test] " + def + " {} }", expectErrors: true);
				Assert.That(res.Item2.AllMessages, Has.Count.EqualTo(1));
				Assert.That(res.Item2.AllMessages.Any(m => m.Code == 7020));
			}
		}
    }
}
