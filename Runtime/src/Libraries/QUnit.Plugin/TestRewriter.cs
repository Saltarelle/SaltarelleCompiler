using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace QUnit.Plugin {
    public class TestRewriter : IJSTypeSystemRewriter {
	    private readonly IErrorReporter _errorReporter;
	    private readonly IRuntimeLibrary _runtimeLibrary;

	    public TestRewriter(IErrorReporter errorReporter, IRuntimeLibrary runtimeLibrary) {
		    _errorReporter  = errorReporter;
		    _runtimeLibrary = runtimeLibrary;
	    }

	    private JsType ConvertType(JsClass type) {
			var instanceMethods = new List<JsMethod>();
			var tests = new List<Tuple<string, string, bool, int?, JsFunctionDefinitionExpression>>();

			foreach (var method in type.InstanceMethods) {
				var testAttr = AttributeReader.ReadAttribute<TestAttribute>(method.CSharpMember);
				var asyncTestAttr = AttributeReader.ReadAttribute<TestAttribute>(method.CSharpMember);
				if (testAttr != null || asyncTestAttr != null) {
					string description     = (testAttr != null ? testAttr.Description            : asyncTestAttr.Description) ?? method.CSharpMember.Name;
					string category        = (testAttr != null ? testAttr.Category               : asyncTestAttr.Category);
					int    expectedAsserts = (testAttr != null ? testAttr.ExpectedAssertionCount : asyncTestAttr.ExpectedAssertionCount);
					bool   isAsync         = (testAttr == null);

					tests.Add(Tuple.Create(description, category, isAsync, expectedAsserts >= 0 ? (int?)expectedAsserts : null, method.Definition));
				}
				else
					instanceMethods.Add(method);
			}

			var testInvocations = new List<JsExpression>();

			foreach (var category in tests.GroupBy(t => t.Item2).Select(g => new { Category = g.Key, Tests = g.Select(x => new { Description = x.Item1, IsAsync = x.Item3, ExpectedAssertionCount = x.Item4, Function = x.Item5 }) }).OrderBy(x => x.Category)) {
				if (category.Category != null)
					testInvocations.Add(JsExpression.Invocation(JsExpression.Identifier("module"), JsExpression.String(category.Category)));
				testInvocations.AddRange(category.Tests.Select(t => JsExpression.Invocation(JsExpression.Identifier(t.IsAsync ? "asyncTest" : "test"), t.ExpectedAssertionCount != null ? new JsExpression[] { JsExpression.String(t.Description), JsExpression.Number(t.ExpectedAssertionCount.Value), _runtimeLibrary.Bind(t.Function, JsExpression.This) } : new JsExpression[] { JsExpression.String(t.Description), _runtimeLibrary.Bind(t.Function, JsExpression.This) })));
			}

			instanceMethods.Add(new JsMethod(null, "runTests", null, JsExpression.FunctionDefinition(new string[0], new JsBlockStatement(testInvocations.Select(t => new JsExpressionStatement(t))))));

			var result = type.Clone();
			result.InstanceMethods.Clear();
			foreach (var m in instanceMethods)
				result.InstanceMethods.Add(m);
			return result;

/*
			public class TestMethodData {
				public string Description { get; private set; }
				public string Category { get; private set; }
				public int? ExpectedAssertionCount { get; private set; }
				public bool IsAsync { get; private set; }

				public TestMethodData(string description, string category, bool isAsync, int? expectedAssertionCount) {
					Description = description;
					Category = category;
					IsAsync = isAsync;
					ExpectedAssertionCount = expectedAssertionCount;
				}
			}


			if (_typeSemantics[method.DeclaringTypeDefinition].IsTestFixture && name == "runTests") {
				Message(7019, method);
			}
			var sta = method.Attributes.FirstOrDefault(a => a.AttributeType.FullName == TestAttribute);
			var ata = method.Attributes.FirstOrDefault(a => a.AttributeType.FullName == AsyncTestAttribute);
			if (sta != null && ata != null) {
				Message(7021, method);
			}
			else if (sta != null || ata != null) {
				if (!_typeSemantics[method.DeclaringTypeDefinition].IsTestFixture) {
					Message(7022, method);
				}
				if (!method.ReturnType.Equals(_compilation.FindType(KnownTypeCode.Void)) || method.TypeParameters.Any() || method.Parameters.Any() || method.IsStatic || !method.IsPublic) {
					Message(7020, method);
				}
				else {
					var ta = sta ?? ata;
					bool isAsync = ata != null;
					string description = (ta.PositionalArguments.Count > 0 ? (string)ta.PositionalArguments[0].ConstantValue : null) ?? method.Name;
					string category = GetNamedArgument<string>(ta, CategoryPropertyName);
					int? expectedAssertionCount = GetNamedArgument<int?>(ta, ExpectedAssertionCountPropertyName) ?? -1;
					_methodTestData[method] = new TestMethodData(description, category, isAsync, expectedAssertionCount >= 0 ? expectedAssertionCount : (int?)null);
				}
			}

			if (_metadataImporter.IsTestFixture(c.CSharpTypeDefinition)) {
				var tests = new List<Tuple<string, string, bool, int?, JsFunctionDefinitionExpression>>();
				var instanceMethodList = new List<JsMethod>();
				foreach (var m in c.InstanceMethods) {
					var td = (m.CSharpMember is IMethod ? _metadataImporter.GetTestData((IMethod)m.CSharpMember) : null);
					if (td != null) {
						tests.Add(Tuple.Create(td.Description, td.Category, td.IsAsync, td.ExpectedAssertionCount, m.Definition));
					}
					else {
						instanceMethodList.Add(m);
					}
				}
				var testInvocations = new List<JsExpression>();
				foreach (var category in tests.GroupBy(t => t.Item2).Select(g => new { Category = g.Key, Tests = g.Select(x => new { Description = x.Item1, IsAsync = x.Item3, ExpectedAssertionCount = x.Item4, Function = x.Item5 }) }).OrderBy(x => x.Category)) {
					if (category.Category != null)
						testInvocations.Add(JsExpression.Invocation(JsExpression.Identifier("module"), JsExpression.String(category.Category)));
					testInvocations.AddRange(category.Tests.Select(t => JsExpression.Invocation(JsExpression.Identifier(t.IsAsync ? "asyncTest" : "test"), t.ExpectedAssertionCount != null ? new JsExpression[] { JsExpression.String(t.Description), JsExpression.Number(t.ExpectedAssertionCount.Value), _runtimeLibrary.Bind(t.Function, JsExpression.This) } : new JsExpression[] { JsExpression.String(t.Description), _runtimeLibrary.Bind(t.Function, JsExpression.This) })));
				}

				instanceMethodList.Add(new JsMethod(null, "runTests", null, JsExpression.FunctionDefinition(new string[0], new JsBlockStatement(testInvocations.Select(t => new JsExpressionStatement(t))))));

				instanceMethods = instanceMethodList;
			}
			else {
				instanceMethods = c.InstanceMethods;
			}
*/
		}

	    public IEnumerable<JsType> Rewrite(IEnumerable<JsType> types) {
		    foreach (var type in types) {
				var cls = type as JsClass;
				if (cls != null) {
					var attr = AttributeReader.ReadAttribute<TestFixtureAttribute>(type.CSharpTypeDefinition);
					yield return attr != null ? ConvertType(cls) : type;
				}
				else {
					yield return type;
				}
			}
	    }
    }

/*
		[Test]
		public void TestMethodsAreGroupedByCategoryWithTestsWithoutCategoryFirst() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.prototype = {
	runTests: function() {
		test('Test1 description', $Bind(function(x1) {
			X1;
		}, this));
		test('Test4 description', $Bind(function(x4) {
			X4;
		}, this));
		module('Category1');
		test('Test2 description', $Bind(function(x2) {
			X2;
		}, this));
		test('Test5 description', $Bind(function(x5) {
			X5;
		}, this));
		module('Category2');
		test('Test3 description', $Bind(function(x3) {
			X3;
		}, this));
		test('Test6 description', $Bind(function(x6) {
			X6;
		}, this));
	}
};
{Type}.registerClass(global, 'MyClass', $MyClass);
",          new MockScriptSharpMetadataImporter { IsTestFixture = t => t.FullName == "MyClass", GetTestData = m => { int idx = m.Name.IndexOf("X"); return new TestMethodData(m.Name.Substring(idx + 1) + " description", idx >= 0 ? m.Name.Substring(0, idx) : null, false, null); } },
			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				InstanceMethods = { new JsMethod(CreateMockMethod("Test1"), "test1", null, CreateFunction("x1")),
				                    new JsMethod(CreateMockMethod("Category1XTest2"), "category1XTest2", null, CreateFunction("x2")),
				                    new JsMethod(CreateMockMethod("Category2XTest3"), "category2XTest3", null, CreateFunction("x3")),
				                    new JsMethod(CreateMockMethod("Test4"), "test4", null, CreateFunction("x4")),
				                    new JsMethod(CreateMockMethod("Category1XTest5"), "category1XTest5", null, CreateFunction("x5")),
				                    new JsMethod(CreateMockMethod("Category2XTest6"), "category2XTest6", null, CreateFunction("x6")),
				                  }
			});
		}

		[Test]
		public void TestFixtureClassHasARunMethodThatRunsAllTestMethodsInTheClass() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.prototype = {
	normalMethod: function(y) {
		Y;
	},
	runTests: function() {
		test('TestMethod description', $Bind(function(x1) {
			X1;
		}, this));
		asyncTest('AsyncTestMethod description', $Bind(function(x2) {
			X2;
		}, this));
		test('TestMethodWithAssertionCount description', 3, $Bind(function(x3) {
			X3;
		}, this));
		asyncTest('AsyncTestMethodWithAssertionCount description', 3, $Bind(function(x4) {
			X4;
		}, this));
	}
};
{Type}.registerClass(global, 'MyClass', $MyClass);
",          new MockScriptSharpMetadataImporter { IsTestFixture = t => t.FullName == "MyClass", GetTestData = m => m.Name.Contains("TestMethod") ? new TestMethodData(m.Name + " description", null, m.Name.Contains("Async"), m.Name.Contains("AssertionCount") ? 3 : (int?)null) : null },
			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				InstanceMethods = { new JsMethod(CreateMockMethod("TestMethod"), "testMethod", null, CreateFunction("x1")),
				                    new JsMethod(CreateMockMethod("AsyncTestMethod"), "asyncTestMethod", null, CreateFunction("x2")),
				                    new JsMethod(CreateMockMethod("TestMethodWithAssertionCount"), "testMethodWithAssertionCount", null, CreateFunction("x3")),
				                    new JsMethod(CreateMockMethod("AsyncTestMethodWithAssertionCount"), "asyncTestMethodWithAssertionCount", null, CreateFunction("x4")),
				                    new JsMethod(CreateMockMethod("NormalMethod"), "normalMethod", null, CreateFunction("y"))
				                  }
			});
		}

 
	[TestFixture]
	public class TestingTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void IsTestFixtureReturnsTrueForTypesDecoratedWithTestFixtureAttribute() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {}

public class C2 {}
");
			Assert.That(Metadata.IsTestFixture(AllTypes["C1"]), Is.True);
			Assert.That(Metadata.IsTestFixture(AllTypes["C2"]), Is.False);
		}

		[Test]
		public void TestFixtureClassCannotDeclareMethodWithScriptNameRunTests() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	public void RunTests() {
	}
}
", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestFixtureAttribute") && m.Contains("runTests")));
		}

		[Test]
		public void TestAttributeWorks() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	[Test]
	public void M1() {}

	[Test(""M2 description"")]
	public void M2() {}

	[Test(ExpectedAssertionCount = 2)]
	public void M3() {}

	[Test(""M4 description"", ExpectedAssertionCount = -1)]
	public void M4() {}

	[Test(Category = ""My category"")]
	public void M5() {}

	public void M6() {}
}
");
			var m1 = Metadata.GetTestData(FindMethods("C1.M1").Single().Item1);
			Assert.That(m1, Is.Not.Null);
			Assert.That(m1.IsAsync, Is.False);
			Assert.That(m1.Description, Is.EqualTo("M1"));
			Assert.That(m1.Category, Is.Null);
			Assert.That(m1.ExpectedAssertionCount, Is.Null);

			var m2 = Metadata.GetTestData(FindMethods("C1.M2").Single().Item1);
			Assert.That(m2, Is.Not.Null);
			Assert.That(m2.IsAsync, Is.False);
			Assert.That(m2.Description, Is.EqualTo("M2 description"));
			Assert.That(m2.Category, Is.Null);
			Assert.That(m2.ExpectedAssertionCount, Is.Null);

			var m3 = Metadata.GetTestData(FindMethods("C1.M3").Single().Item1);
			Assert.That(m3, Is.Not.Null);
			Assert.That(m3.IsAsync, Is.False);
			Assert.That(m3.Description, Is.EqualTo("M3"));
			Assert.That(m3.Category, Is.Null);
			Assert.That(m3.ExpectedAssertionCount, Is.EqualTo(2));

			var m4 = Metadata.GetTestData(FindMethods("C1.M4").Single().Item1);
			Assert.That(m4, Is.Not.Null);
			Assert.That(m4.IsAsync, Is.False);
			Assert.That(m4.Description, Is.EqualTo("M4 description"));
			Assert.That(m4.Category, Is.Null);
			Assert.That(m4.ExpectedAssertionCount, Is.Null);

			var m5 = Metadata.GetTestData(FindMethods("C1.M5").Single().Item1);
			Assert.That(m5, Is.Not.Null);
			Assert.That(m5.IsAsync, Is.False);
			Assert.That(m5.Description, Is.EqualTo("M5"));
			Assert.That(m5.Category, Is.EqualTo("My category"));
			Assert.That(m5.ExpectedAssertionCount, Is.Null);

			var m6 = Metadata.GetTestData(FindMethods("C1.M6").Single().Item1);
			Assert.That(m6, Is.Null);
		}

		[Test]
		public void AsyncTestAttributeWorks() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	[AsyncTest]
	public void M1() {}

	[AsyncTest(""M2 description"")]
	public void M2() {}

	[AsyncTest(ExpectedAssertionCount = 2)]
	public void M3() {}

	[AsyncTest(""M4 description"", ExpectedAssertionCount = null)]
	public void M4() {}

	[AsyncTest(Category = ""My category"")]
	public void M5() {}

	public void M6() {}
}
");
			var m1 = Metadata.GetTestData(FindMethods("C1.M1").Single().Item1);
			Assert.That(m1, Is.Not.Null);
			Assert.That(m1.IsAsync, Is.True);
			Assert.That(m1.Description, Is.EqualTo("M1"));
			Assert.That(m1.Category, Is.Null);
			Assert.That(m1.ExpectedAssertionCount, Is.Null);

			var m2 = Metadata.GetTestData(FindMethods("C1.M2").Single().Item1);
			Assert.That(m2, Is.Not.Null);
			Assert.That(m2.IsAsync, Is.True);
			Assert.That(m2.Description, Is.EqualTo("M2 description"));
			Assert.That(m2.Category, Is.Null);
			Assert.That(m2.ExpectedAssertionCount, Is.Null);

			var m3 = Metadata.GetTestData(FindMethods("C1.M3").Single().Item1);
			Assert.That(m3, Is.Not.Null);
			Assert.That(m3.IsAsync, Is.True);
			Assert.That(m3.Description, Is.EqualTo("M3"));
			Assert.That(m3.Category, Is.Null);
			Assert.That(m3.ExpectedAssertionCount, Is.EqualTo(2));

			var m4 = Metadata.GetTestData(FindMethods("C1.M4").Single().Item1);
			Assert.That(m4, Is.Not.Null);
			Assert.That(m4.IsAsync, Is.True);
			Assert.That(m4.Description, Is.EqualTo("M4 description"));
			Assert.That(m4.Category, Is.Null);
			Assert.That(m4.ExpectedAssertionCount, Is.Null);

			var m5 = Metadata.GetTestData(FindMethods("C1.M5").Single().Item1);
			Assert.That(m5, Is.Not.Null);
			Assert.That(m5.IsAsync, Is.True);
			Assert.That(m5.Description, Is.EqualTo("M5"));
			Assert.That(m5.Category, Is.EqualTo("My category"));
			Assert.That(m5.ExpectedAssertionCount, Is.Null);

			var m6 = Metadata.GetTestData(FindMethods("C1.M6").Single().Item1);
			Assert.That(m6, Is.Null);
		}

		[Test]
		public void MethodWithTestOrAsyncTestAttributeMustBeAPublicNonGenericParameterInstanceMethodReturningVoid() {
			var defs = new[] { "private void M()", "public int M()", "public void M<T>()", "public void M(int x)", "public static void M()" };

			foreach (var def in defs) {
				foreach (var attr in new[] { "Test", "AsyncTest" }) {
					Prepare("using System.Testing; [TestFixture] public class C1 { [" + attr + "] " + def + " {} }", expectErrors: true);
					Assert.That(AllErrors, Has.Count.EqualTo(1));
					Assert.That(AllErrors[0].Code, Is.EqualTo(7020));
				}
			}
		}

		[Test]
		public void TestAttributeAndAsyncTestAttributeOnTheSameMethodIsAnError() {
			Prepare("using System.Testing; [TestFixture] public class C1 { [Test][AsyncTest] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("AsyncTestAttribute")));
		}

		[Test]
		public void TestOrAsyncTestAttributeCannotBeSpecifiedOnTypeThatIsNotATestFixture() {
			Prepare("using System.Testing; public class C1 { [Test] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("TestFixtureAttribute")));

			Prepare("using System.Testing; public class C1 { [AsyncTest] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("TestFixtureAttribute")));
		}
	}
*/
}
