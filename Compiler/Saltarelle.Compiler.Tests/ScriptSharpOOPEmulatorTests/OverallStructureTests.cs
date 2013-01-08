using System;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.OOPEmulator;
using Saltarelle.Compiler.ScriptSemantics;
using Is = NUnit.Framework.Is;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulatorTests {
	[TestFixture]
	public class OverallStructureTests : ScriptSharpOOPEmulatorTestBase {
		[Test]
		public void TheOverallStructureIsCorrect() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeEnum
var $OuterNamespace_InnerNamespace_SomeEnum = function() {
};
$OuterNamespace_InnerNamespace_SomeEnum.prototype = { Value1: 1, Value2: 2, Value3: 3 };
{Type}.registerEnum(global, 'OuterNamespace.InnerNamespace.SomeEnum', $OuterNamespace_InnerNamespace_SomeEnum, false);
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType
var $OuterNamespace_InnerNamespace_SomeType = function() {
	this.a = 0;
};
$OuterNamespace_InnerNamespace_SomeType.prototype = {
	method1: function(x) {
		return x;
	},
	method2: function(x, y) {
		return x + y;
	}
};
$OuterNamespace_InnerNamespace_SomeType.staticMethod = function() {
};
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType2
var $OuterNamespace_InnerNamespace_SomeType2 = function() {
	this.b = 0;
};
$OuterNamespace_InnerNamespace_SomeType2.prototype = {
	method1: function(x) {
		return x;
	}
};
$OuterNamespace_InnerNamespace_SomeType2.otherStaticMethod = function() {
};
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherInterface
var $OuterNamespace_InnerNamespace2_OtherInterface = function() {
};
$OuterNamespace_InnerNamespace2_OtherInterface.prototype = { interfaceMethod: null };
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherType
var $OuterNamespace_InnerNamespace2_OtherType = function() {
};
$OuterNamespace_InnerNamespace2_OtherType.prototype = {
	method1: function(x) {
		return x;
	}
};
{Type}.registerClass(global, 'OuterNamespace.InnerNamespace.SomeType', $OuterNamespace_InnerNamespace_SomeType);
{Type}.registerClass(global, 'OuterNamespace.InnerNamespace.SomeType2', $OuterNamespace_InnerNamespace_SomeType2);
{Type}.registerInterface(global, 'OuterNamespace.InnerNamespace2.OtherInterface', $OuterNamespace_InnerNamespace2_OtherInterface, []);
{Type}.registerClass(global, 'OuterNamespace.InnerNamespace2.OtherType', $OuterNamespace_InnerNamespace2_OtherType, {SomeType2});
x = 1;
y = 1;
",

			new JsClass(CreateMockTypeDefinition("OuterNamespace.InnerNamespace.SomeType"), JsClass.ClassTypeEnum.Class, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(JsExpression.This, "a"), JsExpression.Number(0)))),
				InstanceMethods = { new JsMethod(CreateMockMethod("Method1"), "method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))),
				                    new JsMethod(CreateMockMethod("Method2"), "method2", null, JsExpression.FunctionDefinition(new[] { "x", "y" }, new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("x"), JsExpression.Identifier("y")))))
				                  },
				StaticMethods = { new JsMethod(CreateMockMethod("StaticMethod"), "staticMethod", null, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)) },
			},
			new JsClass(CreateMockTypeDefinition("OuterNamespace.InnerNamespace.SomeType2"), JsClass.ClassTypeEnum.Class, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(JsExpression.This, "b"), JsExpression.Number(0)))),
				InstanceMethods = { new JsMethod(CreateMockMethod("Method1"), "method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))) },
				StaticMethods = { new JsMethod(CreateMockMethod("OtherStaticMethod"), "otherStaticMethod", null, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)) },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("y"), JsExpression.Number(1))) }
			},
			new JsEnum(CreateMockTypeDefinition("OuterNamespace.InnerNamespace.SomeEnum"), new[] {
				new JsEnumValue("Value1", 1),
				new JsEnumValue("Value2", 2),
				new JsEnumValue("Value3", 3),
			}),
			new JsClass(CreateMockTypeDefinition("OuterNamespace.InnerNamespace2.OtherType"), JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(Common.CreateMockType("OuterNamespace.InnerNamespace.SomeType2")), null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement),
				InstanceMethods = { new JsMethod(CreateMockMethod("Method1"), "method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))), },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("x"), JsExpression.Number(1))) }
			},
			new JsClass(CreateMockTypeDefinition("OuterNamespace.InnerNamespace2.OtherInterface"), JsClass.ClassTypeEnum.Interface, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement),
				InstanceMethods = { new JsMethod(CreateMockMethod("InterfaceMethod"), "interfaceMethod", null, null) },
				StaticInitStatements = {}
			});
		}

		[Test]
		public void TypesAppearInTheCorrectOrder() {
			var names = new[] {
				"SomeType",
				"SomeType2",
				"SomeNamespace.SomeType",
				"SomeNamespace.InnerNamespace.OtherType1",
				"SomeNamespace.InnerNamespace.OtherType2",
				"SomeNamespace.SomeType.StrangeType1",
				"SomeNamespace.SomeType.StrangeType2",
				"SomeType2.SomeType3",
				"SomeTYpe2.SomeType3.SomeType4",
			};

			var rnd = new Random(3);
			var unorderedNames = names.Select(n => new { n = n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => x.n).ToArray();
			
			var output = Process(unorderedNames.Select(n => new JsClass(CreateMockTypeDefinition(n), JsClass.ClassTypeEnum.Class, null, null, null)));

			var actual = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(l => l.StartsWith("// ")).Select(l => l.Substring(3)).ToList();

			Assert.That(actual, Is.EqualTo(names));
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypes() {
			var sourceFile = new MockSourceFile("file.cs", @"
public class C3 {}
public interface I1 {}
public class C2 : C3 {}
public class C1 : C2, I1 {}
");
			var nc = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var n = new MockNamer();
            var er = new MockErrorReporter(true);
			var compilation = new Saltarelle.Compiler.Compiler.Compiler(nc, n, new MockRuntimeLibrary(), er, allowUserDefinedStructs: false).CreateCompilation(new[] { sourceFile }, new[] { Common.Mscorlib }, new string[0]);

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// C1
var $C1 = function() {
};
////////////////////////////////////////////////////////////////////////////////
// C2
var $C2 = function() {
};
////////////////////////////////////////////////////////////////////////////////
// C3
var $C3 = function() {
};
////////////////////////////////////////////////////////////////////////////////
// I1
var $I1 = function() {
};
{Type}.registerClass(global, 'C3', $C3);
{Type}.registerInterface(global, 'I1', $I1, []);
{Type}.registerClass(global, 'C2', $C2, {C3});
{Type}.registerClass(global, 'C1', $C1, {C2}, {I1});
",			
			
				new JsClass(ReflectionHelper.ParseReflectionName("C1").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(Common.CreateMockType("C2")), new JsExpression[] { new JsTypeReferenceExpression(Common.CreateMockType("I1")) }),
				new JsClass(ReflectionHelper.ParseReflectionName("C2").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(Common.CreateMockType("C3")), new JsExpression[0]),
				new JsClass(ReflectionHelper.ParseReflectionName("C3").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]),
				new JsClass(ReflectionHelper.ParseReflectionName("I1").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Interface, null, null, new JsExpression[0]));
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypesGeneric() {
			var sourceFile = new MockSourceFile("file.cs", @"
public class B<T> {}
public interface I<T> {}
public class A : B<int>, I<int> {}
");
			var nc = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var n = new MockNamer();
            var er = new MockErrorReporter(true);
			var compilation = new Saltarelle.Compiler.Compiler.Compiler(nc, n, new MockRuntimeLibrary(), er, allowUserDefinedStructs: false).CreateCompilation(new[] { sourceFile }, new[] { Common.Mscorlib }, new string[0]);

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// A
var $A = function() {
};
////////////////////////////////////////////////////////////////////////////////
// B
var $B = function() {
};
////////////////////////////////////////////////////////////////////////////////
// I
var $I = function() {
};
{Type}.registerClass(global, 'B', $B);
{Type}.registerInterface(global, 'I', $I, []);
{Type}.registerClass(global, 'A', $A, {C}, {I});
",			
			
				new JsClass(ReflectionHelper.ParseReflectionName("A").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(Common.CreateMockType("C")), new JsExpression[] { new JsTypeReferenceExpression(Common.CreateMockType("I")) }),
				new JsClass(ReflectionHelper.ParseReflectionName("B`1").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]),
				new JsClass(ReflectionHelper.ParseReflectionName("I`1").Resolve(compilation.Compilation).GetDefinition(), JsClass.ClassTypeEnum.Interface, null, null, new JsExpression[0]));
		}

		[Test]
		public void ByNamespaceComparerOrdersTypesCorrectly() {
			var orig = new[] { "A", "B", "C", "A.B", "A.BA", "A.C", "A.BAA.A", "B.A", "B.B", "B.C", "B.A.A", "B.A.B", "B.B.A" };
			var rnd = new Random();
			var shuffled = orig.Select(n => new { n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => x.n).ToList();
			var actual = ScriptSharpOOPEmulator.OrderByNamespace(shuffled, s => s).ToList();
			Assert.That(actual, Is.EqualTo(orig));
		}

		[Test]
		public void ProgramWithEntryPointWorks() {
			var type = CreateMockTypeDefinition("MyClass");
			var main = new Mock<IMethod>(MockBehavior.Strict);
			main.SetupGet(_ => _.DeclaringTypeDefinition).Returns(type);
			main.SetupGet(_ => _.Name).Returns("Main");
			main.SetupGet(_ => _.Parameters).Returns(EmptyList<IParameter>.Instance);

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.$main = function() {
	X;
};
{Type}.registerClass(global, 'MyClass', $MyClass);
{MyClass}.$Main();
",			new MockScriptSharpMetadataImporter() { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) },
			main.Object,
			new JsClass(type, JsClass.ClassTypeEnum.Class, null, null, null) {
				StaticMethods = { new JsMethod(main.Object, "$main", new string[0], JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Identifier("X")))) }
			});
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodHasParameters() {
			var type = CreateMockTypeDefinition("MyClass");
			var main = new Mock<IMethod>(MockBehavior.Strict);
			main.SetupGet(_ => _.DeclaringTypeDefinition).Returns(type);
			main.SetupGet(_ => _.Name).Returns("Main");
			main.SetupGet(_ => _.FullName).Returns("MyClass.Main");
			main.SetupGet(_ => _.Parameters).Returns(new[] { new Mock<IParameter>().Object });
			main.SetupGet(_ => _.Region).Returns(DomRegion.Empty);

			var er = new MockErrorReporter();

			Process(
				new[] {
					new JsClass(type, JsClass.ClassTypeEnum.Class, null, null, null) {
						StaticMethods = { new JsMethod(main.Object, "$Main", new string[0], JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Identifier("X")))) }
					}
				},
				new MockScriptSharpMetadataImporter() { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) },
				er,
				main.Object
			);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7800 && (string)m.Args[0] == "MyClass.Main"));
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodIsNotImplementedAsANormalMethod() {
			var type = CreateMockTypeDefinition("MyClass");
			var main = new Mock<IMethod>(MockBehavior.Strict);
			main.SetupGet(_ => _.DeclaringTypeDefinition).Returns(type);
			main.SetupGet(_ => _.Name).Returns("Main");
			main.SetupGet(_ => _.FullName).Returns("MyClass.Main");
			main.SetupGet(_ => _.Parameters).Returns(EmptyList<IParameter>.Instance);
			main.SetupGet(_ => _.Region).Returns(DomRegion.Empty);

			var er = new MockErrorReporter();

			Process(
				new[] {
					new JsClass(type, JsClass.ClassTypeEnum.Class, null, null, null) {
						StaticMethods = { new JsMethod(main.Object, "$Main", new string[0], JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Identifier("X")))) }
					}
				},
				new MockScriptSharpMetadataImporter() { GetMethodSemantics = m => ReferenceEquals(m, main.Object) ? MethodScriptSemantics.InlineCode("X") : MethodScriptSemantics.NormalMethod("$" + m.Name) },
				er,
				main.Object
			);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7801 && (string)m.Args[0] == "MyClass.Main"));
		}
	}
}
