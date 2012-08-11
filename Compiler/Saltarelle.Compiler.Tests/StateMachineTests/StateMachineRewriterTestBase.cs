using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	public class StateMachineRewriterTestBase {
		protected void AssertCorrect(string orig, string expected, bool isIteratorBlock = false) {
			int tempIndex = 0;
			int loopLabelIndex = 0;
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(orig));
			JsBlockStatement result;
			if (isIteratorBlock) {
				int finallyHandlerIndex = 0;
				result = StateMachineRewriter.RewriteIteratorBlock(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), () => string.Format("$finally" + (++finallyHandlerIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), sm => {
					var body = new List<JsStatement>();
					if (sm.Variables.Count > 0)
						body.Add(new JsVariableDeclarationStatement(sm.Variables));
					body.AddRange(sm.FinallyHandlers.Select(h => new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(h.Item1), h.Item2))));
					if (sm.Disposer != null)
						body.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("dispose"), new JsFunctionDefinitionExpression(new string[0], sm.Disposer))));
					body.Add(sm.MainBlock);
					return new JsBlockStatement(body);
				});
			}
			else {
				result = StateMachineRewriter.Rewrite(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)));
			}
			var actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}
	}
}
