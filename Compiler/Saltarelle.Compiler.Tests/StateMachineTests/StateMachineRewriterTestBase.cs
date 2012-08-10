using System;
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
			int finallyHandlerIndex = 0;
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(orig));
			var result = StateMachineRewriter.Rewrite(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), () => string.Format("$finally" + (++finallyHandlerIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), isIteratorBlock: isIteratorBlock);
			var actual = string.Join("", result.FinallyHandlers.Select(h => new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(h.Item1), h.Item2))).Concat(result.Disposer != null ? new JsStatement[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("disposer"), new JsFunctionDefinitionExpression(new string[0], result.Disposer))), result.MainBlock } : new JsStatement[] { result.MainBlock }).Select(s => OutputFormatter.Format(s)));
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}
	}
}
