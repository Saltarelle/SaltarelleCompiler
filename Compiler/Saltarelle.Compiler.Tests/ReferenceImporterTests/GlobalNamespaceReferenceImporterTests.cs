using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ReferenceImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ReferenceImporterTests {
	[TestFixture]
	public class GlobalNamespaceReferenceImporterTests {
		private string Process(IList<JsStatement> stmts, IScriptSharpMetadataImporter metadata = null) {
			var obj = new GlobalNamespaceReferenceImporter(metadata ?? new MockScriptSharpMetadataImporter());
			var processed = obj.ImportReferences(stmts);
			return string.Join("", processed.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImprtingTypesFromGlobalNamespaceWorks() {
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
	}
}
