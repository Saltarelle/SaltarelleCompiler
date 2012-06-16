using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulator {
	[TestFixture]
	public class ClassTests : ScriptSharpOOPEmulatorTestBase {
		private JsFunctionDefinitionExpression CreateFunction(string id) {
			return JsExpression.FunctionDefinition(new[] { id }, new JsExpressionStatement(JsExpression.Identifier(id.ToUpper())));
		}

		[Test]
		public void NonGenericClassWithAllDataWorks() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(x) {
	X;
};
{MyClass}.prototype = {
	m1: function(a) {
		A;
	},
	m2: function(b) {
		B;
	}
};
{MyClass}.ctor1 = function(y) {
	Y;
};
{MyClass}.ctor2 = function(z) {
	Z;
};
{MyClass}.ctor1.prototype = {MyClass}.ctor2.prototype = {MyClass}.prototype;
{MyClass}.s1 = function(s) {
	S;
};
{MyClass}.s2 = function(t) {
	T;
};
{MyClass}.registerClass('SomeNamespace.InnerNamespace.MyClass', TheBaseClass, Interface1, Interface2, Interface3);
Q;
R;
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				InstanceMethods = { new JsMethod("m1", null, CreateFunction("a")),
				                    new JsMethod("m2", null, CreateFunction("b")),
				                  },
				StaticMethods = { new JsMethod("s1", null, CreateFunction("s")),
				                  new JsMethod("s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}

		[Test]
		public void ClassWithoutInstanceMethodsOmitsAssignmentOfPrototype() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(x) {
	X;
};
{MyClass}.ctor1 = function(y) {
	Y;
};
{MyClass}.ctor2 = function(z) {
	Z;
};
{MyClass}.ctor1.prototype = {MyClass}.ctor2.prototype = {MyClass}.prototype;
{MyClass}.s1 = function(s) {
	S;
};
{MyClass}.s2 = function(t) {
	T;
};
{MyClass}.registerClass('SomeNamespace.InnerNamespace.MyClass', TheBaseClass, Interface1, Interface2, Interface3);
Q;
R;
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				StaticMethods = { new JsMethod("s1", null, CreateFunction("s")),
				                  new JsMethod("s2", null, CreateFunction("t"))
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
{MyClass} = function(x) {
	X;
};
{MyClass}.registerClass('MyClass', TheBaseClass, Interface1, Interface2, Interface3);
",			new JsClass(null, "MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutBaseClassButWithInterfacesPassesNullForTheBaseTypeInRegisterClass() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(x) {
	X;
};
{MyClass}.registerClass('SomeNamespace.InnerNamespace.MyClass', null, Interface1, Interface2, Interface3);
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutInterfacesWorks() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(x) {
	X;
};
{MyClass}.registerClass('SomeNamespace.InnerNamespace.MyClass', TheBaseClass);
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new JsExpression[0]) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void ClassWithoutBothBaseClassAndInterfacesOnlyPassTheNameToRegisterClass() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(x) {
	X;
};
{MyClass}.registerClass('SomeNamespace.InnerNamespace.MyClass');
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				UnnamedConstructor = CreateFunction("x"),
			});
		}

		[Test]
		public void InterfaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// IMyInterface
{IMyInterface} = function() {
};
{IMyInterface}.prototype = { m1: null, m2: null };
{IMyInterface}.registerInterface('IMyInterface');
",			new JsClass(null, "IMyInterface", JsClass.ClassTypeEnum.Interface, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				InstanceMethods = { new JsMethod("m1", null, null),
				                    new JsMethod("m2", null, null),
				                  },
			});
		}

		[Test]
		public void ClassWithoutUnnamedConstructorWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
{MyClass} = function() {
};
{MyClass}.registerClass('MyClass');
",			new JsClass(null, "MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]));
		}

		[Test]
		public void GenericClassWorks() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
{MyClass} = function(T1, T2) {
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
	var $name = 'SomeNamespace.InnerNamespace.MyClass$' + T1.__typeName + '$' + T2.__typeName;
	$type.registerClass($name, $InstantiateGenericType(TheBaseClass, T1), Interface1, $InstantiateGenericType(Interface2, T2, Int32), Interface3);
	$type.registerGenericInstance($name, $type);
	Q;
	R;
	return $type;
};
{MyClass}.registerGenericClass('SomeNamespace.InnerNamespace.MyClass', 2);
",			new JsClass(null, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, new[] { "T1", "T2" }, JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("TheBaseClass"), JsExpression.Identifier("T1")), new JsExpression[] { JsExpression.Identifier("Interface1"), JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("T2"), JsExpression.Identifier("Int32")), JsExpression.Identifier("Interface3") }) {
				UnnamedConstructor = CreateFunction("x"),
				NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction("y")),
				                      new JsNamedConstructor("ctor2", CreateFunction("z")),
				                    },
				InstanceMethods = { new JsMethod("m1", null, CreateFunction("a")),
				                    new JsMethod("m2", null, CreateFunction("b")),
				                  },
				StaticMethods = { new JsMethod("s1", null, CreateFunction("s")),
				                  new JsMethod("s2", null, CreateFunction("t"))
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
{IMyInterface} = function(T1, T2) {
	var $type = function() {
	};
	$type.prototype = { m1: null, m2: null };
	var $name = 'IMyInterface$' + T1.__typeName + '$' + T2.__typeName;
	$type.registerInterface($name);
	$type.registerGenericInstance($name, $type);
	return $type;
};
{IMyInterface}.registerGenericInterface('IMyInterface', 2);
",			new JsClass(null, "IMyInterface", JsClass.ClassTypeEnum.Interface, new[] { "T1", "T2" }, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				InstanceMethods = { new JsMethod("m1", null, null),
				                    new JsMethod("m2", null, null),
				                  },
			});
		}

		[Test]
		public void GenericInstanceMethodWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void GenericStaticMethodWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void GlobalMethodsAttributeCausesGlobalMethodsToBeGenerated() {
			Assert.Fail("TODO");
		}
	}
}
