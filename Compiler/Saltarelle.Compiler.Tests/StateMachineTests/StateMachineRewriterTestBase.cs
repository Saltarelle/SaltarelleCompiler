using System.Globalization;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.GotoRewrite;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	public class StateMachineRewriterTestBase {
		protected void AssertCorrect(string orig, string expected, bool isIteratorBlock = false) {
			int tempIndex = 0;
			int loopLabelIndex = 0;
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(orig));
			var result = StateMachineRewriter.Rewrite(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), isIteratorBlock: isIteratorBlock);
			var actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}
	}
}
