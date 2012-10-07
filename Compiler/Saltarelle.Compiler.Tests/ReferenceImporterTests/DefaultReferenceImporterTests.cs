using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ReferenceImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ReferenceImporterTests {
	[TestFixture]
	public class DefaultReferenceImporterTests {
		private string Process(IList<JsStatement> stmts, IScriptSharpMetadataImporter metadata = null, INamer namer = null) {
			var obj = new DefaultReferenceImporter(metadata ?? new MockScriptSharpMetadataImporter(), namer ?? new MockNamer());
			var processed = obj.ImportReferences(stmts);
			return string.Join("", processed.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImportingTypesFromGlobalNamespaceWorks() {
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(Common.CreateMockType("GlobalType"))),
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockType("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)))
			}, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"$GlobalType;
return $Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameResultsInGlobalAccess() {
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsMemberAccessExpression(new JsTypeReferenceExpression(Common.CreateMockType("GlobalType")), "x")),
			}, metadata: new MockScriptSharpMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual, "x;\n");
		}

		[Test]
		public void ImportingTypesFromModulesWorks() {
			var t1 = Common.CreateMockType("SomeNamespace.InnerNamespace.Type1");
			var t2 = Common.CreateMockType("SomeNamespace.InnerNamespace.Type2");
			var t3 = Common.CreateMockType("SomeNamespace.Type3");
			var t4 = Common.CreateMockType("Type4");
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => t.Name == "Type1" || t.Name == "Type3" ? "module1" : (t.Name == "Type2" ? "module2" : "module3") };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Add(JsExpression.MemberAccess(new JsTypeReferenceExpression(t1), "a"), JsExpression.MemberAccess(new JsTypeReferenceExpression(t2), "b"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.MemberAccess(new JsTypeReferenceExpression(t3), "c"), JsExpression.MemberAccess(new JsTypeReferenceExpression(t4), "d"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.MemberAccess(new JsTypeReferenceExpression(t1), "e"), JsExpression.MemberAccess(new JsTypeReferenceExpression(t4), "f"))),
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
			var t1 = Common.CreateMockType("Type1");
			var md = new MockScriptSharpMetadataImporter { GetModuleName = t => "mymodule", GetTypeSemantics = t => TypeScriptSemantics.NormalType("") };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t1), "a")),
			}, metadata: md);

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('mymodule');
$mymodule.a;
");
		}

		[Test]
		public void GeneratedModuleAliasesAreValidAndDoNotClashWithEachOtherOrUsedSymbols() {
			var t1 = Common.CreateMockType("Type1");
			var t2 = Common.CreateMockType("Type2");
			var t3 = Common.CreateMockType("Type3");
			var t4 = Common.CreateMockType("Type4");
			var t5 = Common.CreateMockType("Type5");
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
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t1), "a")),
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t2), "b")),
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t3), "c")),
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t4), "d")),
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(t5), "e")),
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
			var actual = DefaultReferenceImporter.UsedSymbolsGatherer.Analyze(program);
			Assert.That(actual, Is.EquivalentTo(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }));
		}
	}
}
