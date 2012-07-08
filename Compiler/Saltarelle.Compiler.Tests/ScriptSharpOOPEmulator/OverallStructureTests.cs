using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.CSharp;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.RuntimeLibrary;
using Is = NUnit.Framework.Is;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulator {
	[TestFixture]
	public class OverallStructureTests : ScriptSharpOOPEmulatorTestBase {
		[Test]
		public void TheOverallStructureIsCorrect() {
			AssertCorrect(
@"{Type}.registerNamespace('OuterNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeEnum
{SomeEnum} = function() {
};
{SomeEnum}.prototype = { Value1: 1, Value2: 2, Value3: 3 };
{SomeEnum}.registerEnum('OuterNamespace.InnerNamespace.SomeEnum', false);
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType
{SomeType} = function() {
	this.a = 0;
};
{SomeType}.prototype = {
	method1: function(x) {
		return x;
	},
	method2: function(x, y) {
		return x + y;
	}
};
{SomeType}.staticMethod = function() {
};
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType2
{SomeType2} = function() {
	this.b = 0;
};
{SomeType2}.prototype = {
	method1: function(x) {
		return x;
	}
};
{SomeType2}.otherStaticMethod = function() {
};
{Type}.registerNamespace('OuterNamespace.InnerNamespace2');
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherInterface
{OtherInterface} = function() {
};
{OtherInterface}.prototype = { interfaceMethod: null };
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherType
{OtherType} = function() {
};
{OtherType}.prototype = {
	method1: function(x) {
		return x;
	}
};
{SomeType}.registerClass('OuterNamespace.InnerNamespace.SomeType');
{SomeType2}.registerClass('OuterNamespace.InnerNamespace.SomeType2');
{OtherInterface}.registerInterface('OuterNamespace.InnerNamespace2.OtherInterface', []);
{OtherType}.registerClass('OuterNamespace.InnerNamespace2.OtherType', {SomeType2});
y = 1;
x = 1;
",

			new JsClass(CreateMockType("OuterNamespace.InnerNamespace.SomeType"), "OuterNamespace.InnerNamespace.SomeType", JsClass.ClassTypeEnum.Class, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(JsExpression.This, "a"), JsExpression.Number(0)))),
				InstanceMethods = { new JsMethod("method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))),
				                    new JsMethod("method2", null, JsExpression.FunctionDefinition(new[] { "x", "y" }, new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("x"), JsExpression.Identifier("y")))))
				                  },
				StaticMethods = { new JsMethod("staticMethod", null, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)) },
			},
			new JsClass(CreateMockType("OuterNamespace.InnerNamespace.SomeType2"), "OuterNamespace.InnerNamespace.SomeType2", JsClass.ClassTypeEnum.Class, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(JsExpression.This, "b"), JsExpression.Number(0)))),
				InstanceMethods = { new JsMethod("method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))) },
				StaticMethods = { new JsMethod("otherStaticMethod", null, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)) },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("y"), JsExpression.Number(1))) }
			},
			new JsEnum(CreateMockType("OuterNamespace.InnerNamespace.SomeEnum"), "OuterNamespace.InnerNamespace.SomeEnum", new[] {
				new JsEnumValue("Value1", 1),
				new JsEnumValue("Value2", 2),
				new JsEnumValue("Value3", 3),
			}),
			new JsClass(CreateMockType("OuterNamespace.InnerNamespace2.OtherType"), "OuterNamespace.InnerNamespace2.OtherType", JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(null, "OuterNamespace.InnerNamespace.SomeType2"), null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement),
				InstanceMethods = { new JsMethod("method1", null, JsExpression.FunctionDefinition(new[] { "x" }, new JsReturnStatement(JsExpression.Identifier("x")))), },
				StaticInitStatements = { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("x"), JsExpression.Number(1))) }
			},
			new JsClass(CreateMockType("OuterNamespace.InnerNamespace2.OtherInterface"), "OuterNamespace.InnerNamespace2.OtherInterface", JsClass.ClassTypeEnum.Interface, null, null, null) {
				UnnamedConstructor = JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement),
				InstanceMethods = { new JsMethod("interfaceMethod", null, null) },
				StaticInitStatements = {}
			});
		}

		[Test]
		public void TypesAppearInTheCorrectOrder() {
			var names = new[] {
				"SomeNamespace.InnerNamespace.OtherType1",
				"SomeNamespace.InnerNamespace.OtherType2",
				"SomeNamespace.SomeType",
				"SomeNamespace.SomeType.StrangeType1",
				"SomeNamespace.SomeType.StrangeType2",
				"SomeType",
				"SomeType2",
				"SomeType2.SomeType3",
				"SomeTYpe2.SomeType3.SomeType4"
			};

			var rnd = new Random(3);
			var unorderedNames = names.Select(n => new { n = n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => x.n).ToArray();
			
			var output = Process(unorderedNames.Select(n => new JsClass(CreateMockType(n), n, JsClass.ClassTypeEnum.Class, null, null, null)));

			var actual = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(l => l.StartsWith("// ")).Select(l => l.Substring(3)).ToList();

			Assert.That(actual, Is.EqualTo(names));
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypes() {
			var sourceFile = new MockSourceFile("file.cs", @"
class C3 {}
interface I1 {}
class C2 : C3 {}
class C1 : C2, I1 {}
");
			var nc = new MetadataImporter.ScriptSharpMetadataImporter(false);
            var er = new MockErrorReporter(true);
			var compilation = new Saltarelle.Compiler.Compiler.Compiler(nc, new MockRuntimeLibrary(), er).CreateCompilation(new[] { sourceFile }, new[] { Common.Mscorlib }, new string[0]);

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// C1
{C1} = function() {
};
////////////////////////////////////////////////////////////////////////////////
// C2
{C2} = function() {
};
////////////////////////////////////////////////////////////////////////////////
// C3
{C3} = function() {
};
////////////////////////////////////////////////////////////////////////////////
// I1
{I1} = function() {
};
{C3}.registerClass('C3');
{I1}.registerInterface('I1', []);
{C2}.registerClass('C2', {C3});
{C1}.registerClass('C1', {C2}, {I1});
",			
			
				new JsClass(ReflectionHelper.ParseReflectionName("C1").Resolve(compilation.Compilation).GetDefinition(), "C1", JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(compilation.Compilation.MainAssembly, "C2"), new JsExpression[] { new JsTypeReferenceExpression(compilation.Compilation.MainAssembly, "I1") }),
				new JsClass(ReflectionHelper.ParseReflectionName("C2").Resolve(compilation.Compilation).GetDefinition(), "C2", JsClass.ClassTypeEnum.Class, null, new JsTypeReferenceExpression(compilation.Compilation.MainAssembly, "C3"), new JsExpression[0]),
				new JsClass(ReflectionHelper.ParseReflectionName("C3").Resolve(compilation.Compilation).GetDefinition(), "C3", JsClass.ClassTypeEnum.Class, null, null, new JsExpression[0]),
				new JsClass(ReflectionHelper.ParseReflectionName("I1").Resolve(compilation.Compilation).GetDefinition(), "I1", JsClass.ClassTypeEnum.Interface, null, null, new JsExpression[0]));
		}
	}
}
