using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Moq;
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
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
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
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
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
",			new JsClass(CreateMockType(), "MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
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
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
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
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, JsExpression.Identifier("TheBaseClass"), new JsExpression[0]) {
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
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
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
{IMyInterface}.registerInterface('IMyInterface', [Interface1, Interface2, Interface3]);
",			new JsClass(CreateMockType(), "IMyInterface", JsClass.ClassTypeEnum.Interface, null, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
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
",			new JsClass(CreateMockType(), "MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]));
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
	$type.registerGenericClassInstance($type, {MyClass}, [T1, T2], function() {
		return $InstantiateGenericType(TheBaseClass, T1);
	}, function() {
		return [Interface1, $InstantiateGenericType(Interface2, T2, Int32), Interface3];
	});
	Q;
	R;
	return $type;
};
{MyClass}.registerGenericClass('SomeNamespace.InnerNamespace.MyClass', 2);
",			new JsClass(CreateMockType(), "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, new[] { "T1", "T2" }, JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("TheBaseClass"), JsExpression.Identifier("T1")), new JsExpression[] { JsExpression.Identifier("Interface1"), JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("T2"), JsExpression.Identifier("Int32")), JsExpression.Identifier("Interface3") }) {
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
	$type.registerGenericInterfaceInstance($type, {IMyInterface}, [T1, T2], function() {
		return [Interface1, Interface2, Interface3];
	});
	return $type;
};
{IMyInterface}.registerGenericInterface('IMyInterface', 2);
",			new JsClass(CreateMockType(), "IMyInterface", JsClass.ClassTypeEnum.Interface, new[] { "T1", "T2" }, null, new[] { JsExpression.Identifier("Interface1"), JsExpression.Identifier("Interface2"), JsExpression.Identifier("Interface3") }) {
				InstanceMethods = { new JsMethod("m1", null, null),
				                    new JsMethod("m2", null, null),
				                  },
			});
		}

		[Test]
		public void GenericInstanceMethodWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
{MyClass} = function() {
};
{MyClass}.prototype = {
	m1: function(T1, T2) {
		return function(x) {
			X;
		};
	}
};
{MyClass}.registerClass('MyClass');
",			new JsClass(CreateMockType(), "MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				InstanceMethods = { new JsMethod("m1", new[] { "T1", "T2" }, CreateFunction("x")) }
			});
		}

		[Test]
		public void GenericStaticMethodWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
{MyClass} = function() {
};
{MyClass}.m1 = function(T1, T2) {
	return function(x) {
		X;
	};
};
{MyClass}.registerClass('MyClass');
",			new JsClass(CreateMockType(), "MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod("m1", new[] { "T1", "T2" }, CreateFunction("x")) }
			});
		}

		[Test]
		public void GlobalMethodsAttributeCausesGlobalMethodsToBeGenerated() {
			var typeDef = new Mock<ITypeDefinition>(MockBehavior.Strict);
			var attr = new Mock<IAttribute>(MockBehavior.Strict);
			var attrType = new Mock<ITypeDefinition>();
			typeDef.SetupGet(_ => _.Attributes).Returns(new[] { attr.Object });
			attr.Setup(_ => _.AttributeType).Returns(attrType.Object);
			attr.Setup(_ => _.PositionalArguments).Returns(new ResolveResult[0]);
			attrType.SetupGet(_ => _.FullName).Returns("System.Runtime.CompilerServices.GlobalMethodsAttribute");

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
window.s1 = function(s) {
	S;
};
window.s2 = function(t) {
	T;
};
Q;
R;
",			new JsClass(typeDef.Object, "SomeNamespace.InnerNamespace.MyClass", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]) {
				StaticMethods = { new JsMethod("s1", null, CreateFunction("s")),
				                  new JsMethod("s2", null, CreateFunction("t"))
				                },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("Q")),
				                         new JsExpressionStatement(JsExpression.Identifier("R")),
				                       }
			});
		}
	}
}
