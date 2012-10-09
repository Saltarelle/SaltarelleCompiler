using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulatorTests {
	[TestFixture]
	public class ClassTests : ScriptSharpOOPEmulatorTestBase {
		private JsFunctionDefinitionExpression CreateFunction(string id) {
			return JsExpression.FunctionDefinition(new[] { id }, new JsExpressionStatement(JsExpression.Identifier(id.ToUpper())));
		}

		[Test]
		public void NonGenericClassWithAllDataWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	X;
};
$SomeNamespace_InnerNamespace_MyClass.prototype = {
	m1: function(a) {
		A;
	},
	m2: function(b) {
		B;
	}
};
$SomeNamespace_InnerNamespace_MyClass.ctor1 = function(y) {
	Y;
};
$SomeNamespace_InnerNamespace_MyClass.ctor2 = function(z) {
	Z;
};
$SomeNamespace_InnerNamespace_MyClass.ctor1.prototype = $SomeNamespace_InnerNamespace_MyClass.ctor2.prototype = $SomeNamespace_InnerNamespace_MyClass.prototype;
$SomeNamespace_InnerNamespace_MyClass.s1 = function(s) {
	S;
};
$SomeNamespace_InnerNamespace_MyClass.s2 = function(t) {
	T;
};
{Type}.registerClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass, TheBaseClass, Interface1, Interface2, Interface3);
Q;
R;
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", null, CreateFunction("a")),
				                    new JsMethod(CreateMockMethod("M2"), "m2", null, CreateFunction("b")),
				                  },
				StaticMethods = { new JsMethod(CreateMockMethod("S1"), "s1", null, CreateFunction("s")),
				                  new JsMethod(CreateMockMethod("S2"), "s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}

		[Test]
		public void ClassWithoutInstanceMethodsOmitsAssignmentOfPrototype() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	X;
};
$SomeNamespace_InnerNamespace_MyClass.ctor1 = function(y) {
	Y;
};
$SomeNamespace_InnerNamespace_MyClass.ctor2 = function(z) {
	Z;
};
$SomeNamespace_InnerNamespace_MyClass.ctor1.prototype = $SomeNamespace_InnerNamespace_MyClass.ctor2.prototype = $SomeNamespace_InnerNamespace_MyClass.prototype;
$SomeNamespace_InnerNamespace_MyClass.s1 = function(s) {
	S;
};
$SomeNamespace_InnerNamespace_MyClass.s2 = function(t) {
	T;
};
{Type}.registerClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass, TheBaseClass, Interface1, Interface2, Interface3);
Q;
R;
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				StaticMethods = { new JsMethod(CreateMockMethod("S1"), "s1", null, CreateFunction("s")),
				                  new JsMethod(CreateMockMethod("S2"), "s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}

		[Test]
		public void ClassWithoutNamespaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	X;
};
{Type}.registerClass(global, 'MyClass', $MyClass, TheBaseClass, Interface1, Interface2, Interface3);
",			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutBaseClassButWithInterfacesPassesNullForTheBaseTypeInRegisterClass() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	X;
};
{Type}.registerClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass, null, Interface1, Interface2, Interface3);
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutInterfacesWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	X;
};
{Type}.registerClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass, TheBaseClass);
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new JsExpression[0]) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutBothBaseClassAndInterfacesOnlyPassTheNameToRegisterClass() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	X;
};
{Type}.registerClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass);
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void InterfaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface = function() {
};
$IMyInterface.prototype = { m1: null, m2: null };
{Type}.registerInterface(global, 'IMyInterface', $IMyInterface, [Interface1, Interface2, Interface3]);
",			new JsClass(CreateMockTypeDefinition("IMyInterface"), JsClass.ClassTypeEnum.Interface, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", null, null),
				                    new JsMethod(CreateMockMethod("M2"), "m2", null, null),
				                  },
			});
		}

		[Test]
		public void ClassWithoutUnnamedConstructorWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
{Type}.registerClass(global, 'MyClass', $MyClass);
",			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]));
		}

		[Test]
		public void GenericClassWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(T1, T2) {
	var $type = function(x) {
		X;
	};
	$type.prototype = {
		m1: function(a) {
			A;
		},
		m2: function(b) {
			B;
		}
	};
	$type.ctor1 = function(y) {
		Y;
	};
	$type.ctor2 = function(z) {
		Z;
	};
	$type.ctor1.prototype = $type.ctor2.prototype = $type.prototype;
	$type.s1 = function(s) {
		S;
	};
	$type.s2 = function(t) {
		T;
	};
	{Type}.registerGenericClassInstance($type, {MyClass}, [T1, T2], function() {
		return $InstantiateGenericType(TheBaseClass, T1);
	}, function() {
		return [Interface1, $InstantiateGenericType(Interface2, T2, Int32), Interface3];
	});
	Q;
	R;
	return $type;
};
{Type}.registerGenericClass(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass, 2);
",			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, new[] { "T1", "T2" }, JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("TheBaseClass"), JsExpression.Identifier("T1")), new JsExpression[] { JsExpression.Identifier("Interface1"), JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("T2"), JsExpression.Identifier("Int32")), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", null, CreateFunction("a")),
				                    new JsMethod(CreateMockMethod("M2"), "m2", null, CreateFunction("b")),
				                  },
				StaticMethods = { new JsMethod(CreateMockMethod("S1"), "s1", null, CreateFunction("s")),
				                  new JsMethod(CreateMockMethod("S2"), "s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}

		[Test]
		public void GenericInterfaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface = function(T1, T2) {
	var $type = function() {
	};
	$type.prototype = { m1: null, m2: null };
	{Type}.registerGenericInterfaceInstance($type, {IMyInterface}, [T1, T2], function() {
		return [Interface1, Interface2, Interface3];
	});
	return $type;
};
{Type}.registerGenericInterface(global, 'IMyInterface', $IMyInterface, 2);
",			new JsClass(CreateMockTypeDefinition("IMyInterface"), JsClass.ClassTypeEnum.Interface, new[] { "T1", "T2" }, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", null, null),
				                    new JsMethod(CreateMockMethod("M2"), "m2", null, null),
				                  },
			});
		}

		[Test]
		public void GenericInstanceMethodWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.prototype = {
	m1: function(T1, T2) {
		return function(x) {
			X;
		};
	}
};
{Type}.registerClass(global, 'MyClass', $MyClass);
",			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", new[] { "T1", "T2" }, CreateFunction("x")) }
			});
		}

		[Test]
		public void GenericStaticMethodWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.m1 = function(T1, T2) {
	return function(x) {
		X;
	};
};
{Type}.registerClass(global, 'MyClass', $MyClass);
",			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod(CreateMockMethod("M1"), "m1", new[] { "T1", "T2" }, CreateFunction("x")) }
			});
		}

		[Test]
		public void GlobalMethodsAttributeCausesGlobalMethodsToBeGenerated() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
global.s1 = function(s) {
	S;
};
global.s2 = function(t) {
	T;
};
Q;
R;
", new MockScriptSharpMetadataImporter() { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name == "MyClass" ? "" : t.FullName) },
			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod(CreateMockMethod("S1"), "s1", null, CreateFunction("s")),
				                  new JsMethod(CreateMockMethod("S2"), "s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}

		[Test]
		public void ResourcesAttributeCausesAResourcesClassToBeGenerated() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = { Field1: 'the value', Field2: 123, Field3: null };
{Type}.registerType(global, 'SomeNamespace.InnerNamespace.MyClass', $SomeNamespace_InnerNamespace_MyClass);
",          new MockScriptSharpMetadataImporter { IsResources = t => t.FullName == "SomeNamespace.InnerNamespace.MyClass" },
			new JsClass(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod(CreateMockMethod("S1"), "s1", null, CreateFunction("s")),
				                  new JsMethod(CreateMockMethod("S2"), "s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockType("SomeNamespace.InnerNamespace.MyClass")), "Field1"), JsExpression.String("the value"))),
				                         new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockType("SomeNamespace.InnerNamespace.MyClass")), "Field2"), JsExpression.Number(123))),
				                         new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockType("SomeNamespace.InnerNamespace.MyClass")), "Field3"), JsExpression.Null)),
				                       }
			});
		}

		[Test]
		public void MixinAttributeWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
$.fn.method1 = function(x) {
	X;
};
$.fn.method2 = function(y) {
	Y;
};
",          new MockScriptSharpMetadataImporter { IsMixin = t => t.FullName == "MyClass", GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.FullName == "MyClass" ? "$.fn" : t.FullName) },
			new JsClass(CreateMockTypeDefinition("MyClass"), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod(CreateMockMethod("Method1"), "method1", null, CreateFunction("x")),
				                  new JsMethod(CreateMockMethod("Method2"), "method2", null, CreateFunction("y")) }
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
	}
}
