using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests {
	[TestFixture]
	public class TypeReferenceFinderTests {
		[Test]
		public void CanAnalyzeStatements() {
			var asm = Common.CreateMockAssembly();
			var t1 = Common.CreateMockTypeDefinition("Type", asm);
			var t2 = Common.CreateMockTypeDefinition("Type", asm);
			var t3 = Common.CreateMockTypeDefinition("Type", asm);

			var ast = new JsStatement[] {
				JsStatement.If(JsExpression.Member(new JsTypeReferenceExpression(t1), "X"), JsStatement.Block(
					JsExpression.Add(new JsTypeReferenceExpression(t2), new JsTypeReferenceExpression(t3))
				),
				null),
				JsExpression.Add(JsExpression.Number(1), new JsTypeReferenceExpression(t1)),
			};

			var refs = TypeReferenceFinder.Analyze(ast);
			Assert.That(refs, Has.Count.EqualTo(3));
			Assert.That(refs.Contains(t1));
			Assert.That(refs.Contains(t2));
			Assert.That(refs.Contains(t3));
		}

		[Test]
		public void CanAnalyzeExpression() {
			var asm = Common.CreateMockAssembly();
			var t1 = Common.CreateMockTypeDefinition("Type", asm);
			var t2 = Common.CreateMockTypeDefinition("Type", asm);
			var t3 = Common.CreateMockTypeDefinition("Type", asm);

			var expr = JsExpression.Add(new JsTypeReferenceExpression(t1), JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "X"), JsExpression.Add(new JsTypeReferenceExpression(t2), new JsTypeReferenceExpression(t3))));

			var refs = TypeReferenceFinder.Analyze(expr);
			Assert.That(refs, Has.Count.EqualTo(3));
			Assert.That(refs.Contains(t1));
			Assert.That(refs.Contains(t2));
			Assert.That(refs.Contains(t3));
		}
	}
}
