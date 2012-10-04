using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ReferenceImporter;

namespace Saltarelle.Compiler.Tests.ReferenceImporterTests {
	[TestFixture]
	public class GlobalNamespaceReferenceImporterTests {
		private string Process(IList<JsStatement> stmts) {
			var obj = new GlobalNamespaceReferenceImporter(new MockScriptSharpMetadataImporter());
			var processed = obj.ImportReferences(stmts);
			return string.Join("", processed.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
		}

		[Test]
		public void Works() {
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(Common.CreateMockType("GlobalType"))),
			    new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockType("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)))
			});

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(
@"GlobalType;
return Global.NestedNamespace.InnerNamespace.Type.x + 1;
".Replace("\r\n", "\n")));
		}
	}
}
