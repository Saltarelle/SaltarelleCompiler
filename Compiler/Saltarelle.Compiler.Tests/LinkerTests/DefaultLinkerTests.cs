using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.Linker;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.LinkerTests {
	[TestFixture]
	public class DefaultLinkerTests {
		private ITypeDefinition CreateMockType(string fullName, IAssembly parentAssembly) {
			var mock = Common.CreateTypeMock(fullName);
			mock.SetupGet(_ => _.ParentAssembly).Returns(parentAssembly);
			return mock.Object;
		}

		private IAssembly CreateMockAssembly() {
			return new Mock<IAssembly>().Object;
		}

		private string Process(IList<JsStatement> stmts, IScriptSharpMetadataImporter metadata = null, INamer namer = null, IAssembly mainAssembly = null) {
			var obj = new DefaultLinker(metadata ?? new MockScriptSharpMetadataImporter(), namer ?? new MockNamer());
			var processed = obj.Process(stmts, mainAssembly ?? new Mock<IAssembly>().Object);
			return string.Join("", processed.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImportingTypesFromGlobalNamespaceWorks() {
			var otherAsm = CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(CreateMockType("GlobalType", otherAsm))),
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(CreateMockType("Global.NestedNamespace.InnerNamespace.Type", otherAsm)), "x"), JsExpression.Number(1)))
			}, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"$GlobalType;
return $Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
");
		}

		[Test]
		public void ImportingTypeFromOwnAssemblyUsesTheTypeVariable() {
			var asm = CreateMockAssembly();
			var type = CreateMockType("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(CreateMockType("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1)))
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"$$GlobalType;
return $$Global_$NestedNamespace_$InnerNamespace_$Type.x + 1;
");
		}

		[Test]
		public void ImportingImportedTypeFromOwnAssemblyWorks() {
			var asm = CreateMockAssembly();
			var type = CreateMockType("MyImportedType", asm);
			var actual = Process(new JsStatement[] {
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(type), "x"), JsExpression.Number(1)))
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.FullName), IsImported = t => ReferenceEquals(t, type) });

			AssertCorrect(actual, "return $MyImportedType.x + 1;\n");
		}

		[Test]
		public void ImportingTypeFromOwnAssemblyButOtherModuleNameResultsInARequire() {
			var asm = CreateMockAssembly();
			var type = CreateMockType("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(CreateMockType("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1)))
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))), GetModuleName = t => "some-module", MainModuleName = "main-module" });

			AssertCorrect(actual,
@"require('mscorlib');
var $somemodule = require('some-module');
$somemodule.$GlobalType;
return $somemodule.$Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyWithModuleNameUsesExports() {
			var asm = CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsMemberAccessExpression(new JsTypeReferenceExpression(CreateMockType("GlobalType", asm)), "x")),
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(""), GetModuleName = t => "my-module", MainModuleName = "my-module" });

			AssertCorrect(actual, "exports.x;\n");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyButWithDifferentModuleNameResultsInARequire() {
			var asm = CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsMemberAccessExpression(new JsTypeReferenceExpression(CreateMockType("GlobalType", asm)), "x")),
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(""), GetModuleName = t => "my-module", MainModuleName = "main-module" });

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('my-module');
$mymodule.x;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameResultsInGlobalAccess() {
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsMemberAccessExpression(new JsTypeReferenceExpression(CreateMockType("GlobalType", CreateMockAssembly())), "x")),
			}, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual, "x;\n");
		}

		[Test]
		public void ImportingTypesFromModulesWorks() {
			var asm = CreateMockAssembly();
			var t1 = CreateMockType("SomeNamespace.InnerNamespace.Type1", asm);
			var t2 = CreateMockType("SomeNamespace.InnerNamespace.Type2", asm);
			var t3 = CreateMockType("SomeNamespace.Type3", asm);
			var t4 = CreateMockType("Type4", asm);
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => t.Name == "Type1" || t.Name == "Type3" ? "module1" : (t.Name == "Type2" ? "module2" : "module3") };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f"))),
			}, metadata: md);

			AssertCorrect(actual,
@"require('mscorlib');
var $module1 = require('module1');
var $module2 = require('module2');
var $module3 = require('module3');
$module1.SomeNamespace.InnerNamespace.Type1.a + $module2.SomeNamespace.InnerNamespace.Type2.b;
$module1.SomeNamespace.Type3.c + $module3.Type4.d;
$module1.SomeNamespace.InnerNamespace.Type1.e + $module3.Type4.f;
");
		}

		[Test]
		public void ImportingGlobalMethodsFromModulesWorks() {
			var t1 = CreateMockType("Type1", CreateMockAssembly());
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => "mymodule", GetTypeSemantics = t => TypeScriptSemantics.NormalType("") };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t1), "a")),
			}, metadata: md);

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('mymodule');
$mymodule.a;
");
		}

		[Test]
		public void AsyncModuleWithoutReferencesWorks() {
			var asm = CreateMockAssembly();
			var type = CreateMockType("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
			    new JsExpressionStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(CreateMockType("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1))), 
			}, mainAssembly: asm, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))), IsAsyncModule = true });

			AssertCorrect(actual,
@"define(['mscorlib'], function($_) {
	var exports = {};
	$$GlobalType;
	$$Global_$NestedNamespace_$InnerNamespace_$Type.x + 1;
	return exports;
});
");
		}

		[Test]
		public void AsyncModuleWithReferencesWorks() {
			var asm = CreateMockAssembly();
			var t1 = CreateMockType("SomeNamespace.InnerNamespace.Type1", asm);
			var t2 = CreateMockType("SomeNamespace.InnerNamespace.Type2", asm);
			var t3 = CreateMockType("SomeNamespace.Type3", asm);
			var t4 = CreateMockType("Type4", asm);
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => t.Name == "Type1" || t.Name == "Type3" ? "module1" : (t.Name == "Type2" ? "module2" : "module3"), IsAsyncModule = true };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f"))),
			}, metadata: md);

			AssertCorrect(actual,
@"define(['mscorlib', 'module1', 'module2', 'module3'], function($_, $module1, $module2, $module3) {
	var exports = {};
	$module1.SomeNamespace.InnerNamespace.Type1.a + $module2.SomeNamespace.InnerNamespace.Type2.b;
	$module1.SomeNamespace.Type3.c + $module3.Type4.d;
	$module1.SomeNamespace.InnerNamespace.Type1.e + $module3.Type4.f;
	return exports;
});
");
		}

		[Test]
		public void GeneratedModuleAliasesAreValidAndDoNotClashWithEachOtherOrUsedSymbols() {
			var asm = CreateMockAssembly();
			var t1 = CreateMockType("Type1", asm);
			var t2 = CreateMockType("Type2", asm);
			var t3 = CreateMockType("Type3", asm);
			var t4 = CreateMockType("Type4", asm);
			var t5 = CreateMockType("Type5", asm);
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => { switch (t.Name) {
			                                                                          case "Type1": return "mymodule";
			                                                                          case "Type2": return "mymodule+";
			                                                                          case "Type3": return "mymodule-";
			                                                                          case "Type4": return "+";
			                                                                          case "Type5": return "-";
			                                                                          default: throw new InvalidOperationException();
			                                                                      } }
			                                             };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t2), "b")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t3), "c")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t4), "d")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t5), "e")),
				new JsVariableDeclarationStatement("mymodule", null),
			}, metadata: md, namer: new MockNamer(prefixWithDollar: false));

			AssertCorrect(actual,
@"require('mscorlib');
var _2 = require('-');
var _ = require('+');
var mymodule2 = require('mymodule');
var mymodule4 = require('mymodule-');
var mymodule3 = require('mymodule+');
mymodule2.Type1.a;
mymodule3.Type2.b;
mymodule4.Type3.c;
_.Type4.d;
_2.Type5.e;
var mymodule;
");
		}

		[Test]
		public void UsedSymbolsGathererWorks() {
			var program = JavaScriptParser.Parser.ParseProgram(@"
function a(b, c) {
	function d(e) {
		var f = g, h = i + j;
		for (var k in l) {
		}
		for (m in a) {}
		var n = function o(p, q) {
			r;
		}
	}
}
s + (t * -u);
try {
}
catch (v) {
}
for (w = 0, x; w < 1; w++) {
}
for (var y = 0, z; w < 1; w++) {
}
");
			var actual = DefaultLinker.UsedSymbolsGatherer.Analyze(program);
			Assert.That(actual, Is.EquivalentTo(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }));
		}
	}
}
